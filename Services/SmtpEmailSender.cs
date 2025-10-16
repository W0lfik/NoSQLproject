using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using NoSQLproject.Services.Interfaces;

namespace NoSQLproject.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<SmtpEmailSender> _log;
        public SmtpEmailSender(IConfiguration cfg, ILogger<SmtpEmailSender> log)
        {
            _cfg = cfg;
            _log = log;
        }

        public void Send(string toEmail, string subject, string htmlBody)
        {
            var host = _cfg["Smtp:Host"];
            var portStr = _cfg["Smtp:Port"];
            var user = _cfg["Smtp:User"];
            var pass = _cfg["Smtp:Pass"];
            var from = _cfg["Smtp:From"];

            if (string.IsNullOrWhiteSpace(host)) throw new InvalidOperationException("Smtp:Host missing.");
            if (!int.TryParse(portStr, out var port)) port = 587;
            if (string.IsNullOrWhiteSpace(user)) throw new InvalidOperationException("Smtp:User missing.");
            if (string.IsNullOrWhiteSpace(pass)) throw new InvalidOperationException("Smtp:Pass missing.");
            if (string.IsNullOrWhiteSpace(from)) throw new InvalidOperationException("Smtp:From missing.");

            // Fail fast if host cannot resolve (clearer than generic SmtpException)
            try { _ = Dns.GetHostEntry(host); }
            catch (SocketException ex)
            {
                throw new InvalidOperationException($"SMTP host '{host}' could not be resolved.", ex);
            }

            using var client = new SmtpClient(host, port)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true,          // Gmail: STARTTLS on 587, implicit SSL on 465 also works
                Timeout = 15000
            };

            using var msg = new MailMessage(from, toEmail, subject, htmlBody) { IsBodyHtml = true };

            try
            {
                client.Send(msg); // synchronous
            }
            catch (SmtpException ex)
            {
                _log.LogError(ex,
                    "SMTP send failed. Host:{Host} Port:{Port} User:{User} From:{From} Status:{Status}",
                    host, port, user, from, ex.StatusCode);

                var hint = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException(
                    $"Failed sending mail via {host}:{port}. Status: {ex.StatusCode}. {hint}", ex);
            }
        }
    }
}
