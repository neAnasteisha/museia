using museia.Data;
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
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<IReactionService, ReactionService>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Index/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // ❗️Перевіримо, чи база існує
        if (context.Database.CanConnect())
        {
            context.Database.Migrate();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var users = userManager.Users.ToList();

            Console.WriteLine("🔥 Registered Users in Database:");
            foreach (var user in users)
            {
                Console.WriteLine($"📌 Email: {user.Email}, Username: {user.UserName}, Role: {user.UserType}, user id = {user.Id}");
            }

            var reactions = context.Reactions.ToList();
            Console.WriteLine("🔥 Reactions in Database:");
            foreach (var reaction in reactions)
            {
                Console.WriteLine($"Reaction: {reaction.ReactionType}, Post: {reaction.PostID}, User: {reaction.UserID}");
            }
        }
        else
        {
            Console.WriteLine("⚠️ База даних недоступна. Пропускаємо завантаження даних.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Помилка підключення до бази або міграції: " + ex.Message);
    }
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
