using MediCita.Web.Entidades;
using MediCita.Web.Servicios;
using MediCita.Web.Servicios.Contrato;
using MediCita.Web.Servicios.Implementacion;

using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// CONFIGURACIÓN DE SERVICIOS
// =============================================
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IMedicamentoService, MedicamentoService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IEspecialidadService, EspecialidadService>();
builder.Services.AddScoped<IHorarioService, HorarioService>();
builder.Services.AddScoped<IAdminEstadisticas, AdminService>();
builder.Services.AddScoped<IReporte, ReporteService>();


builder.Services.AddHttpContextAccessor(); 

builder.Services.AddScoped<IAdminUsuariosService, AdminUsuariosService>();
builder.Services.AddScoped<ICitaService, CitaService>();

// NUEVO: Servicio de seeding
builder.Services.AddScoped<ISeedService, SeedService>();

// Autenticación por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Acceso/Index";
        options.LogoutPath = "/Acceso/Salir";
        options.AccessDeniedPath = "/Home/Denegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });


// Sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// =============================================
// SEEDING: CREAR USUARIOS INICIALES (solo una vez)
// =============================================
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<ISeedService>();

    try
    {
        await seedService.CrearUsuariosInicialesAsync();
        Console.WriteLine("Usuarios iniciales creados/verificados correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al crear usuarios iniciales: {ex.Message}");
    }
}


// MIDDLEWARES
// =============================================
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();