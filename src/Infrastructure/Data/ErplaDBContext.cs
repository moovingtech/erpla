using Core.Security.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ErplaDBContext : IdentityDbContext<User>  
    {  
        public ErplaDBContext(DbContextOptions<ErplaDBContext> options) : base(options)
            {

            }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //this.SeedUsers(builder);
            //this.SeedRoles(builder);
            //this.SeedUserRoles(builder);
            //this.SeedRolesClaims(builder);
        }

        private void SeedUsers(ModelBuilder builder)
        {
            User user = new User()
            {
                Id = "b74ddd14-6340-4840-95c2-db12554843e5",
                UserName = "Admin",
                Email = "admin@gmail.com",
                LockoutEnabled = false,
                PhoneNumber = "",
                FirstName = "",
                LastName = ""
            };

            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, "Admin*123");

            builder.Entity<User>().HasData(user);
        }

        private void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Id = "fab4fac1-c546-41de-aebc-a14da6895711", Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" }
                //new IdentityRole() { Id = "c7b013f0-5201-4317-abd8-c211f91b7330", Name = "Operator", ConcurrencyStamp = "2", NormalizedName = "Human Resource" }
                );
        }

        private void SeedUserRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>() { UserId = "b74ddd14-6340-4840-95c2-db12554843e5", RoleId = "fab4fac1-c546-41de-aebc-a14da6895711" }
                );
        }

        private void SeedRolesClaims(ModelBuilder builder)
        {
            builder.Entity<IdentityRoleClaim<string>>().HasData(
                new IdentityRoleClaim<string>() { Id = 1, RoleId = "fab4fac1-c546-41de-aebc-a14da6895711", ClaimType = "Permission", ClaimValue = "claims.create" }
                );
        }
    }  
}
