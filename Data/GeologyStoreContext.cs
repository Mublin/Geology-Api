using Geology_Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Geology_Api.Data;

public class GeologyStoreContext(DbContextOptions<GeologyStoreContext> options) : DbContext (options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Hash> Hashes { get; set; }

    public DbSet<Level> Levels { get; set; }
    public DbSet<LectureNote> LectureNotes { get; set; }
    public DbSet<PastQue> PastQues { get; set; }
}
