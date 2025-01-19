using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Файл не загружен или пуст");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName)?.ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest("Недопустимый формат файла");

                var fileName = Guid.NewGuid() + fileExtension;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = Url.Content($"~/uploads/{fileName}");
                return Ok(new { location = fileUrl });
            }
            catch (Exception ex)
            {
                // Логирование исключения
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }
        [HttpPost("DeleteImage")]
        public IActionResult DeleteImage([FromBody] ImageDeleteRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ImageUrl))
                    return BadRequest("URL изображения отсутствует");

                // Преобразование полного URL в относительный путь
                var baseUrl = $"{Request.Scheme}://{Request.Host}/";
                if (!request.ImageUrl.StartsWith(baseUrl))
                    return BadRequest("Некорректный URL изображения");

                var relativePath = request.ImageUrl.Replace(baseUrl, "").Replace('/', Path.DirectorySeparatorChar);

                // Преобразуем виртуальный путь в физический
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
                Console.WriteLine(filePath);

                if (!System.IO.File.Exists(filePath))
                    return NotFound("Файл не найден");

                // Удаляем файл
                System.IO.File.Delete(filePath);
                return Ok(new { message = "Изображение успешно удалено" });
            }
            catch (Exception ex)
            {
                // Логирование ошибки (опционально)
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        public class ImageDeleteRequest
        {
            public string ImageUrl { get; set; }
        }
    }
    
}
