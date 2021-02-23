using Microsoft.AspNetCore.Mvc;

namespace PasswordPoliciesDemo.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/docs");
        }
    }
}
