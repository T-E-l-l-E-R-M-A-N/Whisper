using Microsoft.EntityFrameworkCore;

namespace Whisper.Frontend.Avalonia.Core.Database;

public class FrontendDbContext : DbContext
{
    public DbSet<FrontendDbModel> Items { get; set; }

    public FrontendDbContext()
    {
        
    }

    public FrontendDbContext(DbContextOptions<FrontendDbContext> options) : base(options)
    {
        
    }
}