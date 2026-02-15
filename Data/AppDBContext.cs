using Microsoft.EntityFrameworkCore;
using CampusRoomBackend.Models;

namespace CampusRoomBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Mendaftarkan tabel Users
        public DbSet<User> Users { get; set; }
    }
}