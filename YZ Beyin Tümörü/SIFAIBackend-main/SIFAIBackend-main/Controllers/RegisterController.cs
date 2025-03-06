using Microsoft.AspNetCore.Mvc;
using muguremreCVBackend.Business;
using SIFAIBackend.Business;
using SIFAIBackend.Entities;
using System.Threading.Tasks;

namespace SIFAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly IRegisterManager _registerManager;

        public RegisterController(IRegisterManager registerManager)
        {
            _registerManager = registerManager;
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            // IRegisterManager.RegisterAsync metodu, kayıt işlemini gerçekleştirir ve sonuç olarak kullanıcının ID'sini döndürür
            int id = await _registerManager.RegisterAsync(user);

            if (id > 0)
            {
                // Başarılı giriş
                return Ok("Kayıt oluşturuldu!");
            }
            else
            {
                // Tekrar kontrol edin
                return BadRequest("Tekrar kontrol edin!");
            }
        }

    }
}
