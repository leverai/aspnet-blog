using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Controllers
{
    [Route("upload-image")]
    [ApiController]
    public class ImageUploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public ImageUploadController(IWebHostEnvironment env)
        {
            _env = env;
        }
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            // Проверяем, был ли файл отправлен
            if (image == null || image.Length == 0)
                return BadRequest(new { error = "Файл не загружен." });

            // Проверяем формат файла
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(image.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { error = "Неподдерживаемый формат файла." });

            try
            {
                // Создаем путь для сохранения файла
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Генерация уникального имени файла
                var uniqueFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Сохранение файла на сервер
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Возвращаем URL изображения в JSON формате
                var imageUrl = $"/uploads/{uniqueFileName}";
                return Ok(new { link = imageUrl }); // Froala требует поле "link"
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ошибка при загрузке файла: " + ex.Message });
            }
        }
    }

    [Route("delete-image")]
    [ApiController]
    public class ImageDeleteController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ImageDeleteController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public IActionResult DeleteImage([FromForm] string src)
        {
            try
            {
                // Убираем ведущий слеш, если он есть
                src = src.TrimStart('/');

                // Абсолютный путь к файлу
                var filePath = Path.Combine(_env.WebRootPath, src);

                // Проверяем, существует ли файл
                if (System.IO.File.Exists(filePath))
                {
                    // Удаляем файл
                    System.IO.File.Delete(filePath);
                    return Ok(new { message = "Изображение успешно удалено." });
                }
                else
                {
                    return NotFound(new { error = "Файл не найден." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ошибка при удалении файла: " + ex.Message });
            }
        }
    }
}
