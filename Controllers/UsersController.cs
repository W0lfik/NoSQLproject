using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NoSQLproject.Models;
using NoSQLproject.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NoSQLproject.Controllers
{
    [Authorize(Roles = "manager")]
    public class UsersController : Controller
    {
        private readonly IUserService _users;
        private readonly ILogger<UsersController> _log;

        public UsersController(IUserService users, ILogger<UsersController> log)
        {
            _users = users;
            _log = log;
        }

        public IActionResult Index(string? search)
        {
            try
            {
                var users = _users.Search(search);
                ViewBag.Search = search;
                return View(users);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Index failed");
                return Problem("Failed to load users.");
            }
        }


        [HttpGet]
        public IActionResult Create() => View(new CreateUserVm());

        // Overposting protection: only bind fields that CreateUserVm should accept.
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(CreateUserVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                _users.Create(vm);
                TempData["Success"] = "User created.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException vex)
            {
                ModelState.AddModelError(string.Empty, vex.Message);
                return View(vm);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Create failed");
                ModelState.AddModelError(string.Empty, "Unexpected error.");
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            try
            {
                var vm = _users.BuildEditVmOrThrow(id);
                return View(vm);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Edit GET failed for {Id}", id);
                return Problem("Failed to load form.");
            }
        }

        // Overposting protection: EmployeeNumber is immutable; do not bind it from the request.
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(EditUserVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                _users.Update(vm);
                TempData["Success"] = "User updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException vex)
            {
                ModelState.AddModelError(string.Empty, vex.Message);
                return View(vm);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Edit POST failed for {Id}", vm.Id);
                ModelState.AddModelError(string.Empty, "Unexpected error.");
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            try
            {
                var u = _users.GetByIdOrThrow(id);
                return View(u);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Delete GET failed for {Id}", id);
                return Problem("Failed to load confirmation.");
            }
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            try
            {
                _users.Delete(id);
                TempData["Success"] = "User deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Delete POST failed for {Id}", id);
                return Problem("Failed to delete user.");
            }
        }
    }
}
