using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Models.Domain;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
//using System.Data.Entity;

namespace WebApplication1.Controllers
{
    public class PostController : Controller
	{

        private readonly DataContext db;

        public PostController(DataContext context)
        {
            db = context;
        }
        public async Task<IActionResult> Index(DateTime? date, int? year, int? month, int? author, string? search, int? page, int pageSize = 3)
		{
            // Начальный запрос к базе
            var query = db.Posts
                .Include(p => p.User) // Используем навигационные свойства для связи с пользователем
                .AsQueryable();

            // Применение фильтров
            if (date.HasValue)
            {
                query = query.Where(p => p.Created.Date == date.Value.Date);
            }
            else
            {
                if (year.HasValue)
                {
                    query = query.Where(p => p.Created.Year == year.Value);
                    ViewData["year"] = year.Value;
                }

                if (month.HasValue)
                {
                    query = query.Where(p => p.Created.Month == month.Value);
                    ViewData["month"] = month.Value;
                }
            }

            if (author.HasValue)
            {
                query = query.Where(p => p.AuthorID == author.Value);
                ViewData["author"] = author.Value;
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.ShortDescription.Contains(search));
                ViewData["search"] = search;
            }
            if(!User.IsInRole("Admin"))
                query = query.Where(p => p.IsVisible == true);
            // Подсчет общего количества записей
            int totalPosts = await query.CountAsync();

            int totalPages = (int)Math.Ceiling((double)totalPosts / pageSize); // общее количество страниц

            int pageNumber = page ?? 1; // текущая страница

            // Применение пагинации
            var paginatedPosts = await query
                .OrderByDescending(p => p.Created)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.ShortDescription,
                    p.AuthorID,
                    AuthorName = p.User.Username,
                    Avatar = p.User.Avatar,
                    p.Created,
                    p.LastUpdated,
                    p.IsVisible,
                    p.ImagePath
                })
                .ToListAsync();
            int range = 3; // Отображаем 3 страницы до и после текущей
            int startPage = Math.Max(1, pageNumber - range);
            int endPage = Math.Min(totalPages, pageNumber + range);
            // Формирование модели для представления
            var model = new PaginatedViewModel
            {
                Posts = paginatedPosts,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                StartPage = startPage,
                EndPage = endPage
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PostView(int id)
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
                    Avatar = p.User.Avatar,
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

        public IActionResult Post()
		{
			return View();
		}
	}
}
