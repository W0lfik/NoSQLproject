using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;
using NoSQLproject.Services.Interfaces;

namespace NoSQLproject.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUserRepository _users;
        private readonly IPasswordResetRepository _tokens;
        private readonly IEmailSender _email;
        private readonly IConfiguration _cfg;

        public PasswordResetService(IUserRepository users,
                                    IPasswordResetRepository tokens,
                                    IEmailSender email,
                                    IConfiguration cfg)
        {
            _users = users;
            _tokens = tokens;
            _email = email;
            _cfg = cfg;
        }

        public void RequestPasswordReset(string emailOrEmployeeNumber)
        {
            // 1) Find the user silently (don’t disclose existence)
            User? user = null;

            if (int.TryParse(emailOrEmployeeNumber, out var empNo))
                user = _users.GetByEmployeeNumber(empNo);
            else
                user = _users.GetByEmail(emailOrEmployeeNumber);

            // Always behave the same whether user exists or not
            if (user == null) return;

            // 2) Invalidate old tokens (optional but nice)
            _tokens.InvalidateAllForUser(user.Id);

            // 3) Create secure token (256-bit), URL-safe Base64
            var token = GenerateUrlSafeToken();

            var prt = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
                Used = false
            };
            _tokens.Create(prt);

            // 4) Send email with reset link
            var baseUrl = _cfg["Smtp:PublicBaseUrl"]?.TrimEnd('/') ?? "";
            var link = $"{baseUrl}/Login/ResetPassword?token={token}";
            var html = $@"
                <p>Hello {user.FullName},</p>
                <p>You requested a password reset. Click the link below to set a new password:</p>
                <p><a href=""{link}"">Reset your password</a></p>
                <p>This link expires in 30 minutes. If you didn’t request this, ignore this email.</p>";
            _email.Send(user.Email, "Password reset", html);
        }

        public void ResetPassword(string token, string newPassword)
        {
            var t = _tokens.GetByToken(token);
            if (t == null || t.Used || t.ExpiresAtUtc < DateTime.UtcNow)
                throw new ValidationException("This password reset link is invalid or has expired.");

            var user = _users.GetById(t.UserId);
            if (user == null)
                throw new ValidationException("User not found for this token.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _users.UpdateUser(user);
            _tokens.MarkUsed(t.Id);
        }

        private static string GenerateUrlSafeToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32); // 256-bit
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}
