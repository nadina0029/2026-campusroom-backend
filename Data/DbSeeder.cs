using CampusRoomBackend.Models;

namespace CampusRoomBackend.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (context.Users.Any())
            {
                return;
            }

            var users = new User[]
            {
                new User
                {
                    Username = "admin",
                    Password = "password123",
                    Role = "Admin",
                    FullName = "Administrator Kampus"
                },
                new User
                {
                    Username = "mahasiswa",
                    Password = "password123",
                    Role = "Mahasiswa",
                    FullName = "Budi Santoso"
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}