namespace Forkful.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(x => x.PublicId).IsUnique();
        modelBuilder.Entity<Product>().Property(x => x.Price).HasPrecision(10, 2);
        modelBuilder.Entity<Product>().Property(x => x.Rating).HasPrecision(3, 1);
        modelBuilder.Entity<Order>().Property(x => x.Subtotal).HasPrecision(10, 2);
        modelBuilder.Entity<Order>().Property(x => x.DeliveryFee).HasPrecision(10, 2);
        modelBuilder.Entity<Order>().Property(x => x.Total).HasPrecision(10, 2);
        modelBuilder.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(10, 2);
        modelBuilder.Entity<Product>().HasOne(x => x.Category).WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Product>().HasOne(x => x.Supplier).WithMany(x => x.Products)
            .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<OrderItem>().HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);
    }
}
