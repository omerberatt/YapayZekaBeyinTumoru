using Microsoft.AspNetCore.Mvc;
using SIFAIBackend.Business;
using SIFAIBackend.Entities;
using System;
using System.Threading.Tasks;

namespace SIFAIBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                var (token, user) = await _authService.AuthenticateAsync(model.Email, model.Password);
                if (!string.IsNullOrEmpty(token) && user != null)
                {
                    // Başarılı giriş, kullanıcı bilgileri ile yanıt döndür
                    return Ok(new
                    {
                        Token = token,
                        UserId = user.Id,
                        Name = user.Name,
                        Message = "Başarılı giriş"
                    });
                }
                else
                {
                    return BadRequest(new { message = "Geçersiz e-posta veya şifre" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
