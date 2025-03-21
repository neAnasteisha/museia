﻿using museia.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using museia.Models;
using museia.Services;
using museia.Repositories;
using museia.IService;
using museia.IRepository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PostRepository>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ComplaintRepository>();
builder.Services.AddScoped<ComplaintService>();
builder.Services.AddScoped<ReactionService>();
builder.Services.AddScoped<IReactionService, ReactionService>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Post/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var users = userManager.Users.ToList();

    Console.WriteLine("🔥 Registered Users in Database:");
    foreach (var user in users)
    {
        Console.WriteLine($"📌 Email: {user.Email}, Username: {user.UserName}, Role: {user.UserType}, user id = {user.Id}");
    }

    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    //if (!context.Posts.Any()) // Щоб уникнути дублювання
    //{

    //    context.Posts.AddRange(
    //        new Post { PostID = 1, CreatedAt = DateTime.Now.AddDays(-10), PostTag = PostTag.Поезія, PostText = "Бог ся рождає, хто ж то може знати. Ісус мо ім'я Марія мо муати.....", UserID = "98610c32-1d86-4555-b029-8f5a7a18c1dd" },
    //        new Post { PostID = 2, CreatedAt = DateTime.Now.AddDays(-3), PostTag = PostTag.Поезія, PostText = "👨‍💻 Програміст влаштовується на роботу.\r\nHR запитує:\r\n— Яка у вас найбільша слабкість?\r\n\r\nПрограміст:\r\n— Регулярні вирази.\r\n\r\nHR:\r\n— А сила?\r\n\r\nПрограміст:\r\n— ([A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+.[A-Za-z]{2,4})", UserID = "e5347ee1-d361-46eb-bc25-b55b0be48d95" }
    //    );
    //    context.SaveChanges();
    //}
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Post}/{action=Index}/{id?}");

app.Run();
