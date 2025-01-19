namespace WebApplication1.Models.ViewModels
{
    public class PaginatedViewModel
    {
        public IEnumerable<object> Posts { get; set; } // Публикации (из Join)
        public int CurrentPage { get; set; } // Текущая страница
        public int TotalPages { get; set; } // Общее количество страниц

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartPage { get; set; }
        public int EndPage { get; set; } 
    }
}
