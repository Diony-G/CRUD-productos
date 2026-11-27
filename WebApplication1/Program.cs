using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(o => o.UseSqlite("Data Source=productos.db"));
var app = builder.Build();

// Crea la base de datos local si no existe
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDb>().Database.EnsureCreated();

app.UseDefaultFiles();
app.UseStaticFiles();

// Leer
app.MapGet("/api/productos", async (AppDb db) => await db.Productos.ToListAsync());

// Crear
app.MapPost("/api/productos", async (Producto p, AppDb db) =>
{
    if (!EsValido(p)) return Results.BadRequest("Datos no válidos");

    p.Id = 0;
    db.Productos.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/api/productos/{p.Id}", p);
});

// Actualizar
app.MapPut("/api/productos/{id}", async (int id, Producto datos, AppDb db) =>
{
    if (!EsValido(datos)) return Results.BadRequest("Datos no válidos");

    var p = await db.Productos.FindAsync(id);
    if (p is null) return Results.NotFound();

    p.Nombre = datos.Nombre;
    p.Precio = datos.Precio;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Eliminar
app.MapDelete("/api/productos/{id}", async (int id, AppDb db) =>
{
    var p = await db.Productos.FindAsync(id);
    if (p is null) return Results.NotFound();

    db.Productos.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

static bool EsValido(Producto p) => !string.IsNullOrWhiteSpace(p.Nombre) && p.Precio > 0;