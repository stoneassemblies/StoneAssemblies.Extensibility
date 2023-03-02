namespace StoneAssemblies.Extensibility.DemoPlugin
{
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        {
        }
    }
}