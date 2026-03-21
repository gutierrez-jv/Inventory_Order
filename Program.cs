using Inventory_Order.Models.Database;
using Inventory_Order.Repository.CustomerRepository;
using Inventory_Order.Repository.OrderRepository;
using Inventory_Order.Repository.ProductRepository;
using Inventory_Order.Repository.UserRepository;
using Inventory_Order.Service.Auth;
using Inventory_Order.Service.Customer;
using Inventory_Order.Service.Order;
using Inventory_Order.Service.Product;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<InventoryOrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICustomerRepo, CustomerRepo>();
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();

builder.Services.AddScoped<ICustomerServ, CustomerServ>();
builder.Services.AddScoped<IProductServ, ProductServ>();
builder.Services.AddScoped<IOrderServ, OrderServ>();
builder.Services.AddScoped<IAuthServ, AuthServ>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();