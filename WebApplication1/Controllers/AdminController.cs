using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.Domain;
using WebApplication1.Models.ViewModels;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
	{

        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(IWebHostEnvironment webHostEnvironment, DataContext context)
        {
            db = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
			return View();
		}
		[HttpGet]
        public IActionResult CreatePost()
		{
			return View();
		}
        // Метод для сохранения новой публикации
        [HttpPost]
        public async Task<IActionResult> CreatePost(Post model, IFormFile? uploadedImage)
		{
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == model.AuthorID);
            Post post = new Post
			{
				Name = model.Name,
				Description = model.Description,
				AuthorID = model.AuthorID,
				ShortDescription = model.ShortDescription,
				Created = DateTime.Now,
				LastUpdated = DateTime.Now,
                IsVisible = model.IsVisible,
                User = user,
            };
            
            db.Posts.Add(post);
            await db.SaveChangesAsync();
            //Если загружено новое изображение
            if (uploadedImage != null)
            {
                // Удаляем старое изображение, если оно есть
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Сохраняем новое изображение
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/posts/", post.Id.ToString());
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    uploadedImage.CopyTo(fileStream);
                }

                // Сохраняем путь к новому изображению
                post.ImagePath = "/uploads/posts/" + post.Id.ToString() + "/" + uniqueFileName;
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Index","Post");
		}

        // Метод для отображения формы редактирования
        [HttpGet]
        public async Task<IActionResult> EditPost(int id)
        {
            var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // Метод для сохранения изменений
        [HttpPost]
        public async Task<IActionResult> EditPost(int id, Post model, IFormFile? uploadedImage)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == model.AuthorID);
            var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            // Обновляем данные поста
            post.Name = model.Name;
            post.Description = model.Description;
            post.AuthorID = model.AuthorID;
            post.ShortDescription = model.ShortDescription;
            post.LastUpdated = DateTime.Now;
            post.IsVisible = model.IsVisible;
            post.User = user;

            //Если загружено новое изображение
            if (uploadedImage != null)
            {
                // Удаляем старое изображение, если оно есть
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Сохраняем новое изображение
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/posts/", post.Id.ToString());
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    uploadedImage.CopyTo(fileStream);
                }

                // Сохраняем путь к новому изображению
                post.ImagePath = "/uploads/posts/" + post.Id.ToString() + "/" + uniqueFileName;
            }

            await db.SaveChangesAsync();

            return RedirectToAction("Index", "Post");
        }

        // Загрузка картинки из редактора
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile? uploadedImage)
        {
            //Если загружено новое изображение
            if (uploadedImage != null)
            {

                // Сохраняем новое изображение
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/posts/");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    uploadedImage.CopyTo(fileStream);
                }
            }

            await db.SaveChangesAsync();

            return RedirectToAction("Index", "Post");
        }

        // GET: Удалить публикацию (форма подтверждения удаления)
        [HttpGet]
        public async Task<IActionResult> DeletePost(int id)
        {

            var posts = db.Posts.Join(db.Users, // второй набор
                p => p.AuthorID, // свойство-селектор объекта из первого набора
                u => u.Id, // свойство-селектор объекта из второго набора
                (p, u) => new // результат
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ShortDescription = p.ShortDescription,
                    AuthorID = p.AuthorID,
                    AuthorName = u.Username,
                    Created = p.Created,
                    LastUpdated = p.LastUpdated,
                    IsVisible = p.IsVisible,
                    ImagePath = p.ImagePath
                });
            var post = await posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // POST: Удалить публикацию
        [HttpPost]
        public async Task<IActionResult> DeletePostConfirmed(int id)
        {
            var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            db.Posts.Remove(post); // Удаляем публикацию
            await db.SaveChangesAsync(); // Сохраняем изменения в базе данных

            return RedirectToAction("Index", "Post"); // Возвращаемся к списку публикаций
        }
    }
}
