using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection.PortableExecutable;
using WebApplication1.Models;
using WebApplication1.Models.Domain;

var builder = WebApplication.CreateBuilder(args);

// ���������� ����� ��������������
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/Login"; // ���� � �������� �����
        options.AccessDeniedPath = "/Account/AccessDenied"; // ���� ��� ������ � �������
    });
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10 MB
});


// ���������� �����������
builder.Services.AddAuthorization();
string connection = "Data Source=DESKTOP-5V8CSOG\\SQLEXPRESS;Initial Catalog=Blog;Integrated Security=True;Encrypt=False";
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(connection));


// Add services to the container.
builder.Services.AddControllersWithViews();


// �������� ������ ����������� �� ����� ������������
//string connection = builder.Configuration.GetConnectionString("DefaultConnection");

// ��������� �������� DataContext � �������� ������� � ����������


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();




app.UseRouting();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Post}/{action=Index}/{id?}");

app.UseAuthentication();
app.UseAuthorization();

//app.MapGet("/posts", async (DataContext db) => await db.Posts.ToListAsync());


//app.MapGet("/api/posts/{id:int}", async (int id, DataContext db) =>
//{
//    // �������� ���� �� id
//    Post? post = await db.Posts.FirstOrDefaultAsync(u => u.Id == id);

//    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
//    if (post == null) return Results.NotFound(new { message = "���� �� �������" });

//    // ���� ������������ ������, ���������� ���
//    return Results.Json(post);
//});

//app.MapDelete("/api/users/{id:int}", async (int id, DataContext db) =>
//{
//    // �������� ���� �� id
//    Post? post = await db.Posts.FirstOrDefaultAsync(u => u.Id == id);

//    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
//    if (post == null) return Results.NotFound(new { message = "������������ �� ������" });

//    // ���� ������������ ������, ������� ���
//    db.Posts.Remove(post);
//    await db.SaveChangesAsync();
//    return Results.Json(post);
//});

//app.MapPost("/api/posts", async (Post post, DataContext db) =>
//{
//    // ��������� ���� � ������
//    await db.Posts.AddAsync(post);
//    await db.SaveChangesAsync();
//    return post;
//});

//app.MapPut("/api/posts", async (Post postData, DataContext db) =>
//{
//    // �������� ���� �� id
//    var post = await db.Posts.FirstOrDefaultAsync(u => u.Id == postData.Id);

//    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
//    if (post == null) return Results.NotFound(new { message = "���� �� ������" });

//    // ���� ������������ ������, �������� ��� ������ � ���������� ������� �������
//    foreach (var prop in typeof(Post).GetProperties())
//    {
//        var newValue = prop.GetValue(postData);
//        if (newValue != null) 
//        {
//            prop.SetValue(post, newValue);
//        }
//    }

//    await db.SaveChangesAsync();
//    return Results.Json(post);
//});
app.MapControllers();
app.Run();
