using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using TDFSantaLucia.Constants;

using TDFSantaLucia.Data;

using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;
using TDFSantaLucia.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure database context

builder.Services.AddDbContext<AppDbContext>(options =>

    options.UseMySql(

        builder.Configuration.GetConnectionString("DefaultConnection"),

        ServerVersion.AutoDetect(

            builder.Configuration.GetConnectionString("DefaultConnection")

        )

    )

);

// Configurar Identity

builder.Services.AddIdentity<Usuario, IdentityRole>(options =>

{

    options.Password.RequireDigit = true;

    options.Password.RequiredLength = 8;

    options.Password.RequireNonAlphanumeric = false;

    options.Password.RequireUppercase = true;

    options.Password.RequireLowercase = true;

})

.AddEntityFrameworkStores<AppDbContext>()

.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>

{

    options.LoginPath = "/Account/Login";

    options.AccessDeniedPath = "/Account/AccesoDenegado";

});

// Registrar Repositories
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

// Registrar Services
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();

// Add services to the container

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed de roles y usuario administrador

using (var scope = app.Services.CreateScope())

{

    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager = services.GetRequiredService<UserManager<Usuario>>();

    string[] roles = { Roles.Admin, Roles.Empleado, Roles.Cliente };

    foreach (var role in roles)

    {

        if (!await roleManager.RoleExistsAsync(role))

        {

            await roleManager.CreateAsync(new IdentityRole { Name = role });

        }

    }

    var adminEmail = "admin@tdfsantalucia.com";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)

    {

        adminUser = new Usuario

        {

            UserName = adminEmail,

            Email = adminEmail,

            Nombre = "Administrador",

            Primer_Apellido = "Sistema",

            Segundo_Apellido = "TDF",

            Estado = true,

            EmailConfirmed = true

        };

        await userManager.CreateAsync(adminUser, "Admin123!");

        await userManager.AddToRoleAsync(adminUser, Roles.Admin);

    }

}

if (!app.Environment.IsDevelopment())

{

    app.UseExceptionHandler("/Home/Error");

}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(

    name: "default",

    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
