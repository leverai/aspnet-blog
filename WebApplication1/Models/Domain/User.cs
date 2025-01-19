using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(8, ErrorMessage = "Минимальная длина пароля 8 символов")]
        public string Password { get; set; }
        public string? Avatar {  get; set; }
        public string Role { get; set; }

        public ICollection<Post> Posts { get; set; }

    }
}
