using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Models.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using WebApplication1.Models.ViewModels;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {

        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(IWebHostEnvironment webHostEnvironment, DataContext context)
        {
            db = context;
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: Страница входа
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl; // Передаём URL в представление
            return View();
        }

        // POST: Обработка данных входа
        [HttpPost]
        public async Task<IActionResult> Login(string login, string password, string returnUrl)
        {
            var hasher = new PasswordHasher<string>();
            var hashedPassword = hasher.HashPassword(null, password);
            //var user = db.Users.FirstOrDefault(u => 
            //    (u.Login != null && u.Login == login || u.Email != null && u.Email == login) 
            //    && u.Password != null && u.Password == hashedPassword);

            var user = db.Users.FirstOrDefault(u => u.Login == login);

            if (user != null)
            {
                var verificationResult = hasher.VerifyHashedPassword(null, user.Password, password);

                if (verificationResult != PasswordVerificationResult.Success)
                {
                    ViewBag.ErrorMessage = "Неверный логин или пароль";
                    return View();
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Неверный логин или пароль";
                return View();
            }

            
            
            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Выполняем вход
            await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Перенаправляем пользователя в зависимости от его роли
            if (user.Role == "Admin")
            {
                return RedirectToAction("CreatePost", "Admin");
            }
            //user.Password = hashedPassword;
            //await db.SaveChangesAsync();

            return RedirectToAction("Index", "Post");
        }

        // GET: Выход пользователя
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }

        // GET: Доступ запрещён
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            ChangeUserViewModel model = new ChangeUserViewModel
            {
                Id = user.Id,
                Login = user.Login,
                Username = user.Username,
                Avatar = user.Avatar
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(int id, ChangeUserViewModel model, IFormFile? uploadedImage)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            // Обновляем данные поста
            user.Login = model.Login;
            user.Username = model.Username;

            //Если загружено новое изображение
            if (uploadedImage != null)
            {
                // Удаляем старое изображение, если оно есть
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, user.Avatar.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Сохраняем новое изображение
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/avatars/");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var FileName = "user"+ user.Id + Path.GetExtension(uploadedImage.FileName);
                var filePath = Path.Combine(uploadsFolder, FileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    uploadedImage.CopyTo(fileStream);
                }

                // Сохраняем путь к новому изображению
                user.Avatar = "/uploads/avatars/" + FileName;
            }

            await db.SaveChangesAsync();
            ViewBag.IsProfileChanged = true;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string OldPassword, string NewPassword, string ConfirmPassword)
        {
            if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword) || NewPassword.Length < 8 )
            {
                ViewBag.ErrorMessage = "Заполните форму корректно. Новый пароль должен состоять минимум из 8 символов.";
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.ErrorMessage = "Пароли не соответствуют.";
                return View();
            }
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var hasher = new PasswordHasher<string>();

            var verificationResult = hasher.VerifyHashedPassword(null, user.Password, OldPassword);

            if (verificationResult != PasswordVerificationResult.Success)
            {
                ViewBag.ErrorMessage = "Неверный пароль";
                return View();
            }
            var hashedNewPassword = hasher.HashPassword(null, NewPassword);
            user.Password = hashedNewPassword;
            await db.SaveChangesAsync();
            ViewBag.IsPasswordChanged = true;
            return View();
        }
    }
}
