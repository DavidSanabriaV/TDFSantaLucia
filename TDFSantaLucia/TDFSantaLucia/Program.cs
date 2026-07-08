using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Constants;
using TDFSantaLucia.Data;
using TDFSantaLucia.Binders;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;
using TDFSantaLucia.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new UsuarioModelBinderProvider());
});

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


// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Registrar Repositories
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IInventarioRepository, InventarioRepository>();
builder.Services.AddScoped<IExpedienteRepository, ExpedienteRepository>();
builder.Services.AddScoped<ICitaRepository, CitaRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<IMovimientoPuntosRepository, MovimientoPuntosRepository>();
builder.Services.AddScoped<IArticuloRepository, ArticuloRepository>();
builder.Services.AddScoped<ICuponRepository, CuponRepository>();
builder.Services.AddScoped<IChatbotRepository, ChatbotRepository>();
builder.Services.AddScoped<IRecetaRepository, RecetaRepository>();
builder.Services.AddScoped<ITratamientoRepository, TratamientoRepository>();


// Registrar Services
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IExpedienteService, ExpedienteService>();
builder.Services.AddScoped<ICitaService, CitaService>();
builder.Services.AddScoped<ICuentaService, CuentaService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();
builder.Services.AddScoped<IPuntosService, PuntosService>();
builder.Services.AddScoped<IArticuloService, ArticuloService>();
builder.Services.AddScoped<ICuponService, CuponService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddScoped<IRecetaService, RecetaService>();
builder.Services.AddScoped<ITratamientoService, TratamientoService>();


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

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(

    name: "default",

    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();