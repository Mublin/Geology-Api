using Geology_Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Geology_Api.Data;

public class GeologyStoreContext : DbContext
{
    public GeologyStoreContext(DbContextOptions<GeologyStoreContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Hash> Hashes { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AdminLog> AdminLogs { get; set; }
    public DbSet<Level> Levels { get; set; }
    public DbSet<LectureNote> LectureNotes { get; set; }
    public DbSet<PastQue> PastQues { get; set; }
}
