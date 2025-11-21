using Microsoft.AspNetCore.Mvc;

namespace Teacher_Evaluation_System__Golden_Success_College_.Controllers.Api
{
    public class TeachersApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
