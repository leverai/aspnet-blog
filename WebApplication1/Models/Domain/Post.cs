﻿using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Domain
{
	public class Post
	{
		public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно для заполнения")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
		public string Name { get; set; }

        [Required(ErrorMessage = "Описание обязательно для заполнения")]
        [MinLength(50, ErrorMessage = "Описание должно содержать не менее 50 символов")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Краткое описание обязательно для заполнения")]
        [StringLength(150, ErrorMessage = "Краткое описание не должно превышать 150 символов")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Поле автора обязательно для заполнения")]
        public int AuthorID { get; set; }
		public DateTime Created {  get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public bool IsVisible { get; set; }
        public string? ImagePath { get; set; }

        public User? User { get; set; }
    }
}
