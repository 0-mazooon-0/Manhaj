using Manhaj.Models;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Services
{
    public static class DataSeeder
    {
        public static void Seed(ManhajDbContext context)
        {
            context.Database.EnsureCreated();

            // Seed Admin
            if (!context.Admins.Any(a => a.Email == "superadmin@manhaj.com"))
            {
                var admin = new Admin
                {
                    FirstName = "System",
                    LastName = "Admin",
                    Email = "superadmin@manhaj.com",
                    Password = PasswordHasher.HashPassword("SuperAdmin@2026!"),
                    Phone = "01000000000"
                };
                context.Admins.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
