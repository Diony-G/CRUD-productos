using Microsoft.EntityFrameworkCore;

public static class ProductoEndpoints
{
    public static void MapProductos(this WebApplication app)
    {
        var api = app.MapGroup("/api/productos");

        // Leer
        api.MapGet("/", async (AppDb db, string? buscar, string? categoria,
            decimal? min, decimal? max, bool? stockBajo, string? orden) =>
        {
            var q = db.Productos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();
                q = q.Where(p => p.Codigo.ToLower().Contains(texto) || p.Nombre.ToLower().Contains(texto));
            }
            if (!string.IsNullOrEmpty(categoria)) q = q.Where(p => p.Categoria == categoria);
            if (min is not null) q = q.Where(p => p.Precio >= min);
            if (max is not null) q = q.Where(p => p.Precio <= max);
            if (stockBajo == true) q = q.Where(p => p.Stock <= 5);

            q = orden switch
            {
                "nombre" => q.OrderBy(p => p.Nombre),
                "precio" => q.OrderBy(p => p.Precio),
                "precioDesc" => q.OrderByDescending(p => p.Precio),
                "stock" => q.OrderBy(p => p.Stock),
                _ => q.OrderBy(p => p.Codigo)
            };

            return await q.ToListAsync();
        });

        // Crear
        api.MapPost("/", async (Producto p, AppDb db) =>
        {
            if (Validar(p) is string error) return Results.BadRequest(error);

            if (await db.Productos.AnyAsync(x => x.Codigo == p.Codigo))
                return Results.Conflict("Ya existe un producto con ese código");

            p.Id = 0;
            db.Productos.Add(p);
            await db.SaveChangesAsync();
            return Results.Created($"/api/productos/{p.Id}", p);
        });

        // Actualizar
        api.MapPut("/{id}", async (int id, Producto datos, AppDb db) =>
        {
            if (Validar(datos) is string error) return Results.BadRequest(error);

            var p = await db.Productos.FindAsync(id);
            if (p is null) return Results.NotFound("El producto no existe");

            if (await db.Productos.AnyAsync(x => x.Codigo == datos.Codigo && x.Id != id))
                return Results.Conflict("Ya existe un producto con ese código");

            datos.Id = id;
            db.Entry(p).CurrentValues.SetValues(datos);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Eliminar
        api.MapDelete("/{id}", async (int id, AppDb db) =>
        {
            var p = await db.Productos.FindAsync(id);
            if (p is null) return Results.NotFound("El producto no existe");

            db.Productos.Remove(p);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    // Limpia los datos y devuelve el primer error encontrado
    static string? Validar(Producto p)
    {
        p.Codigo = (p.Codigo ?? "").Trim().ToUpper();
        p.Nombre = (p.Nombre ?? "").Trim();

        if (p.Codigo == "" || p.Nombre == "") return "El código y el nombre son obligatorios";
        if (string.IsNullOrWhiteSpace(p.Categoria)) return "La categoría es obligatoria";
        if (p.Precio <= 0) return "El precio debe ser mayor que cero";
        if (p.Stock < 0) return "El stock no puede ser negativo";
        return null;
    }
}