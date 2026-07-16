using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("Profile")]
    public class ProfileController : Controller
    {

        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Login");
            }

            var result = await _profileService.GetProfileAsync(userName);
            return View(result);
        }

        [HttpGet("api/Profile/GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            string userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Session Expired"
                });
            }

            var result = await _profileService.GetProfileAsync(userName);

            return Ok(result);
        }
    

    }
}
