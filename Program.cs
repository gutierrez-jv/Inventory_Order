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
// Creates the web application builder and loads configuration/services

builder.Services.AddControllersWithViews();
// Registers MVC services with support for controllers and Razor views

builder.Services.AddDbContext<InventoryOrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Registers the database context and connects it to SQL Server using the DefaultConnection string

builder.Services.AddScoped<ICustomerRepo, CustomerRepo>();
// Registers the customer repository for dependency injection

builder.Services.AddScoped<IProductRepo, ProductRepo>();
// Registers the product repository for dependency injection

builder.Services.AddScoped<IOrderRepo, OrderRepo>();
// Registers the order repository for dependency injection

builder.Services.AddScoped<IUserRepo, UserRepo>();
// Registers the user repository for dependency injection

builder.Services.AddScoped<ICustomerServ, CustomerServ>();
// Registers the customer service for dependency injection

builder.Services.AddScoped<IProductServ, ProductServ>();
// Registers the product service for dependency injection

builder.Services.AddScoped<IOrderServ, OrderServ>();
// Registers the order service for dependency injection

builder.Services.AddScoped<IAuthServ, AuthServ>();
// Registers the authentication service for dependency injection

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        // Redirects unauthenticated users to the login page

        options.AccessDeniedPath = "/Account/AccessDenied";
        // Redirects users here when they do not have permission to access a page

        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        // Sets the cookie session to expire after 30 minutes

        options.SlidingExpiration = true;
        // Refreshes the cookie expiration time while the user remains active
    });

builder.Services.AddAuthorization();
// Enables role-based and policy-based authorization

var app = builder.Build();
// Builds the application pipeline

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Uses the custom error handling page outside development mode

    app.UseHsts();
    // Adds HTTP Strict Transport Security for better security in production
}

app.UseHttpsRedirection();
// Redirects HTTP requests to HTTPS

app.UseRouting();
// Enables route matching for incoming requests

app.UseAuthentication();
// Identifies the current user before accessing secured pages

app.UseAuthorization();
// Checks whether the current user is allowed to access requested resources

app.MapStaticAssets();
// Maps static asset files such as CSS, JavaScript, and images

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();
// Sets the default route so the app opens at the Account controller's Login action

app.Run();
// Starts the application