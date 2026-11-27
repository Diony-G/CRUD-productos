using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(o => o.UseSqlite("Data Source=inventario.db"));
var app = builder.Build();

// Crea la base de datos local si no existe
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDb>().Database.EnsureCreated();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapProductos();

app.Run();