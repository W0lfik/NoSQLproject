using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NoSQLproject.Extensions;
using NoSQLproject.Models;
using NoSQLproject.Services.Interfaces;
using System.Security.Claims;

namespace NoSQLproject.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
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
            var user = _loginService.GetAllUsers()
                .FirstOrDefault(u => u.EmployeeNumber == employeeNumber);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                // Create claims for authentication
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("EmployeeNumber", user.EmployeeNumber.ToString()),
            new Claim(ClaimTypes.Role, user.TypeOfUser.ToString())
        };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CookieAuth", principal);

                //  Redirect based on role
                if (user.TypeOfUser == TypeOfUser.manager)
                {
                    return RedirectToAction("Index", "Users");
                }

                // Default: go to Ticket Overview
                return RedirectToAction("Index", "Ticket");
            }

            ViewBag.Error = "Invalid employee number or password";
            return View();
        }


        // GET: Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }
    }
}
