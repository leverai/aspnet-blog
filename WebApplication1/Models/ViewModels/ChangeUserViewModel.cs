using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.ViewModels
{
    public class ChangeUserViewModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Username { get; set; }
        public string? Avatar { get; set; }
    }
}
