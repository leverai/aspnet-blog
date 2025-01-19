using WebApplication1.Models.Domain;

namespace WebApplication1
{
	public static class StaticData
	{
		public static List<Post> Posts = new();
		public static void AddPost(Post newPost)
		{
            Posts.Add(newPost);
		}
	}
}
