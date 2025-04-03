// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using TentecimApi.Models;
using TentecimApi.Services;

namespace TentecimApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AuthService _authService;

        public AdminController(AuthService authService)
        {
            _authService = authService;
        }

        // 📌 Giriş işlemi: /api/admin/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _authService.LoginAsync(model.Email, model.Password, "admin", model.DeviceToken);

            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(new
            {
                message = result.Message,
                user = new
                {
                    id = result.User.Id,
                    email = result.User.Email,
                    username = result.User.Username,
                    role = result.User.Role
                }
            });
        }
    }
}
