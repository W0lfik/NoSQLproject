using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NoSQLproject.Extensions;
using NoSQLproject.Models;
using NoSQLproject.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;


namespace NoSQLproject.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;
        private readonly IPasswordResetService _resetService;
        public LoginController(ILoginService loginService, IPasswordResetService resetService)
        {
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
            _resetService = resetService ?? throw new ArgumentNullException(nameof(resetService));
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (!Enum.IsDefined(typeof(TypeOfUser), user.TypeOfUser))
            {
                ViewBag.Error = "Please select a valid role";
                return View(user);
            }
            // 1. Generate a unique 6-digit EmployeeNumber
            int newEmployeeNumber;
            do
            {
                newEmployeeNumber = new Random().Next(100000, 999999); // Generates 6-digit number
            }
            while (_loginService.EmployeeNumberExists(newEmployeeNumber)); // Ensure it's unique

            user.EmployeeNumber = newEmployeeNumber;

            // 2. Hash the password
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // 3. Save to DB
            _loginService.Register(user);

            // 4. Redirect to confirmation page
            return RedirectToAction("Confirmation", new
            {
                fullName = user.FullName,
                employeeNumber = user.EmployeeNumber,
                role = user.TypeOfUser
            });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Confirmation(string fullName, int employeeNumber, TypeOfUser role)
        {
            ViewBag.FullName = fullName;
            ViewBag.EmployeeNumber = employeeNumber;
            ViewBag.Role = role;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(int employeeNumber, string password)
        {
            var user = _loginService.ValidateCredentials(employeeNumber, password);
            if (user == null)
            {
                ViewBag.Error = "Invalid employee number or password";
                return View();
            }

            var principal = _loginService.CreatePrincipal(user);
            await HttpContext.SignInAsync("CookieAuth", principal);

            return user.TypeOfUser == TypeOfUser.manager
                ? RedirectToAction("Index", "Users")
                : RedirectToAction("Index", "Ticket");
        }



        // GET: Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }
        ////////////////////////////////////////////////////////////////////////
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string emailOrEmployeeNumber)
        {
            // Always show the same confirmation to avoid user enumeration
            _resetService.RequestPasswordReset(emailOrEmployeeNumber);
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();

        // --- Reset Password via emailed link ---

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest();
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest();

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Token = token;
                ViewBag.Error = "Password must be at least 6 characters.";
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ViewBag.Token = token;
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            try
            {
                _resetService.ResetPassword(token, newPassword);
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            catch (ValidationException vex)
            {
                ViewBag.Token = token;
                ViewBag.Error = vex.Message;
                return View();
            }
            catch (Exception)
            {
                ViewBag.Token = token;
                ViewBag.Error = "Unexpected error. Please try again.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation() => View();
    }
}

