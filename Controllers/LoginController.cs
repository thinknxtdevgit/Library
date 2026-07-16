using lib.DtoModel.LoginDto;
using lib.Interface;
using lib.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace lib.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        // =====================================================
        // LOGIN VIEW
        // =====================================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // =====================================================
        // LOGIN TYPES
        // =====================================================

        [HttpGet("api/Login/GetLoginTypes")]
        public async Task<IActionResult> GetLoginTypes()
        {
            var data = await _loginService.GetLoginTypesAsync();
            return Ok(data);
        }

        // =====================================================
        // LOGIN API
        // =====================================================
        [HttpPost("api/Login/EnterOk")]
        public async Task<IActionResult> EnterOk([FromBody] LoginRequest model)
        {
            try
            {
                HttpContext.Session.Clear();

                var result = await _loginService.LoginAsync(model);

                if (!result.Success)
                    return Unauthorized(result);

                HttpContext.Session.SetString("UserName", result.UserName ?? "");
                HttpContext.Session.SetString("LoginType", result.LoginType ?? "");
                HttpContext.Session.SetString("Colleges",
                    JsonSerializer.Serialize(result.Colleges));
                HttpContext.Session.SetString("MenuData",
                    JsonSerializer.Serialize(result.MenuItems));

                HttpContext.Session.SetString("CollegeName",
                    result.Colleges.FirstOrDefault() ?? "");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        // =====================================================
        // MENU
        // =====================================================

        [HttpGet("api/Login/GetDynamicMenu")]
        public async Task<IActionResult> GetDynamicMenu()
        {
            var result = await _loginService.GetDynamicMenuAsync();
            return Ok(result);
        }

        // =====================================================
        // LOGOUT
        // =====================================================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Login");
        }

    }
}