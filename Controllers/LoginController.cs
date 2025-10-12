using Microsoft.AspNetCore.Mvc;

namespace NoSQLproject.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
