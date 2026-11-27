using Microsoft.EntityFrameworkCore;

public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        // Código único
        modelo.Entity<Producto>().HasIndex(p => p.Codigo).IsUnique();
        // SQLite necesita double para comparar y ordenar precios
        modelo.Entity<Producto>().Property(p => p.Precio).HasConversion<double>();
    }
}