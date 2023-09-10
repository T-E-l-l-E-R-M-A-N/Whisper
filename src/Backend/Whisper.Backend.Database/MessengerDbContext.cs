using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Whisper.Backend.ChatModels;

namespace Whisper.Backend.Database;

public class MessengerDbContext : IdentityDbContext<MessengerUserModel>
{
    public DbSet<MessengerRoomModel> Rooms { get; set; }
    public DbSet<MessengerMessageModel> Messages { get; set; }

    public MessengerDbContext()
    {
        //Database.EnsureCreated();
    }

    public MessengerDbContext(DbContextOptions<MessengerDbContext> options) : base(options)
    {
        //Database.EnsureCreated();
    }
}