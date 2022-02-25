using Core.Security.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.Security.Application.Service.Authentication
{
    public class IdentityDataSeeder
    {


        public static void SeedData(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            SeedRolesAndClaims(roleManager);
            SeedUsers(userManager);
        }

        private static void SeedClaims(RoleManager<IdentityRole> roleManager, IdentityRole role)
        {
            var result =  roleManager.AddClaimAsync(role, new Claim(RoleService.PERMISSION_CLAIM_TYPE, "roles.view"));
        }

        public static User SeedUsers(UserManager<User> userManager)
        {
            User user = null; ;
            if (userManager.FindByNameAsync("Admin").Result == null)
            {
                user = new User()
                {
                    UserName = "Admin",
                    Email = "admin@gmail.com",
                    LockoutEnabled = false,
                    PhoneNumber = "",
                    FirstName = "",
                    LastName = ""
                };
                IdentityResult result = userManager.CreateAsync(user, "1Q2w3E4r#").Result;
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                }
            }
            return user;
        }

        public static  void SeedRolesAndClaims(RoleManager<IdentityRole> roleManager)
        {
            string[] adminClaims = new string[] { "user.view", "user.edit" };
            IdentityRole role = roleManager.FindByNameAsync("Admin").Result;
            IList<Claim> roleClaims = new List<Claim>();
            if (role == null)
            {
                role = new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" };
                IdentityResult roleResult = roleManager.CreateAsync(role).Result;
            } else
            {
                roleClaims = roleManager.GetClaimsAsync(role).Result;
            }
            foreach (var claim in adminClaims)
            {
                if(!RoleHasClaim(roleClaims, claim)) { 
                    IdentityResult claimResult = AddClaim(roleManager, role, claim);
                    Console.WriteLine(claimResult);
                }
            }
        }

        private static bool RoleHasClaim(IList<Claim> roleClaims, string claim)
        {
            return roleClaims.Where(x=>x.Value.Equals(claim)).Any();
        }

        private static IdentityResult AddClaim(RoleManager<IdentityRole> roleManager, IdentityRole role, string claim)
        {
            IdentityResult result = roleManager.AddClaimAsync(role, new Claim("Auth", claim)).Result;
            return result;
        }

    }
}
