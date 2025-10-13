using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoSQLproject.Models;
using NoSQLproject.Services.Interfaces;

namespace NoSQLproject.Controllers
{
    // Only managers can access this controller (adjust policy/role as needed)
    [Authorize(Roles = "manager")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }



        // Show all users in a table
        
        public IActionResult Index()
        {
            var users = _userService.GetAll();
            return View(users);
        }

        // Show form to add a new user
    
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new CreateUserVm();
            return View(vm);
        }

       
        // Handle form submission for creating a new user
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateUserVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var newUser = new User
            {
                FullName = vm.FullName,
                EmployeeNumber = vm.EmployeeNumber,
                Email = vm.Email,
                Phone = vm.Phone,
                Location = vm.Location ?? "",
                TypeOfUser = vm.TypeOfUser,
                Password = vm.Password   // service layer hashes it
            };

            bool created = _userService.Create(newUser);
            if (!created)
            {
                ModelState.AddModelError("", "A user with this email or employee number already exists.");
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        
        // Show form to edit an existing user
        
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var user = _userService.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            var vm = new EditUserVm
            {
                Id = user.Id,
                FullName = user.FullName,
                EmployeeNumber = user.EmployeeNumber,
                Email = user.Email,
                Phone = user.Phone,
                Location = user.Location,
                TypeOfUser = user.TypeOfUser
            };

            return View(vm);
        }

        
        // Handle form submission for editing an existing user
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditUserVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = _userService.GetById(vm.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = vm.FullName;
            user.EmployeeNumber = vm.EmployeeNumber;
            user.Email = vm.Email;
            user.Phone = vm.Phone;
            user.Location = vm.Location ?? "";
            user.TypeOfUser = vm.TypeOfUser;

            // If a new password was entered, update it (service will hash)
            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                user.Password = vm.NewPassword;
            }

            bool updated = _userService.Update(user);
            if (!updated)
            {
                ModelState.AddModelError("", "Email or employee number already exists.");
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        // Show confirmation page before deleting a user
      
        [HttpGet]
        public IActionResult Delete(string id)
        {
            var user = _userService.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

       
        // Confirm deletion of a user
        
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            bool deleted = _userService.Delete(id);
            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
