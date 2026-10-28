var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.Name = ".Forkful.Cart";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>();
builder.Services.AddSingleton<DishCustomizationService>();
builder.Services.AddHttpClient("PublicInfo", client =>
{
    client.Timeout = TimeSpan.FromSeconds(12);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Forkful-Classroom-Demo/1.0");
});

// SQLite makes the demo runnable immediately. Set Database__Provider=MySql and
// ConnectionStrings__MySql to use the very same EF Core models against MySQL.
var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (provider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
    {
        var connection = builder.Configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("Set ConnectionStrings__MySql first.");
        options.UseMySql(connection, ServerVersion.AutoDetect(connection));
    }
    else
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")
            ?? "Data Source=forkful.db");
    }
});

var app = builder.Build();

// Code-first schema creation and repeatable seed data for the classroom demo.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Initialize(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseResponseCaching();
app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
