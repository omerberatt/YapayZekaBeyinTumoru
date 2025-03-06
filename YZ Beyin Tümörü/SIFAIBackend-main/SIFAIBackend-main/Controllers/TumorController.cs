using Microsoft.AspNetCore.Mvc;
using SIFAIBackend.Business;
using SIFAIBackend.Entities;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SIFAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TumorController : ControllerBase
    {
        private readonly string flaskApiUrl = "http://127.0.0.1:5000/predict"; // Flask API URL
        private readonly ITumorService _tumorService;

        public TumorController(ITumorService tumorService)
        {
            _tumorService = tumorService;
        }

        // Yeni bir tespit kaydı ekle ve Flask API'den sonucu al
        [HttpPost("detect")]
        public async Task<IActionResult> DetectTumor([FromForm] TumorDetectionRequest request)
        {
            if (request.Image == null || request.Image.Length == 0 || request.UserId <= 0)
            {
                return BadRequest("Geçersiz talep. Lütfen tüm alanları doldurun.");
            }

            // Görüntüyü "uploads" klasörüne kaydet
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{request.Image.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            using (var httpClient = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    // Flask API'ye göndermek için kaydedilen dosyayı okuyun
                    using (var stream = System.IO.File.OpenRead(filePath))
                    {
                        var fileContent = new StreamContent(stream);
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                        formData.Add(fileContent, "image", request.Image.FileName);

                        // Flask API'ye isteği gönder
                        var response = await httpClient.PostAsync(flaskApiUrl, formData);
                        if (response.IsSuccessStatusCode)
                        {
                            var tumorTypeJson = await response.Content.ReadAsStringAsync();
                            var tumorTypeResponse = JsonSerializer.Deserialize<TumorTypeResponse>(tumorTypeJson);
                            var tumorType = tumorTypeResponse?.TumorType;

                            if (string.IsNullOrWhiteSpace(tumorType))
                            {
                                return BadRequest("Flask API geçersiz bir yanıt döndürdü.");
                            }

                            // Veritabanına kaydet
                            await _tumorService.SaveDetectionAsync(request.UserId, uniqueFileName, tumorType);

                            return Ok(new { TumorType = tumorType, Message = "Tespit kaydedildi." });
                        }
                        else
                        {
                            return StatusCode((int)response.StatusCode, "Flask API'ye bağlanılamadı.");
                        }
                    }
                }
            }
        }


        // Kullanıcının geçmiş tespitlerini getir
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetHistory(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Geçersiz kullanıcı ID'si.");
            }

            var history = await _tumorService.GetHistoryByUserIdAsync(userId);

            if (history == null || history.Count == 0)
            {
                return NotFound("Bu kullanıcı için herhangi bir geçmiş tespiti bulunamadı.");
            }

            return Ok(history);
        }
    }

    // TumorDetectionRequest modeli
    public class TumorDetectionRequest
    {
        public int UserId { get; set; } // Kullanıcı ID'si
        public IFormFile Image { get; set; } // Yüklenen dosya
    }

    // Flask API'den dönen JSON'u temsil eden model
    public class TumorTypeResponse
    {
        [JsonPropertyName("tumor_type")]
        public string TumorType { get; set; } // Örnek: "meningioma", "glioma", vb.
    }
}
