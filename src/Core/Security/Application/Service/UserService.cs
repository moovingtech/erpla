using Core.Domain;
using Core.Security.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Core.Application.Service
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        //private readonly IConfiguration _configuration;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> CreateUserAsync(AddUserRequest user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var userExists = await _userManager.FindByNameAsync(user.UserName);
            if (userExists != null)
                throw new ApplicationException("User Name already exists");

            User newUser = new User()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
                Email = user.Email
            };

            var identityResult = await _userManager.CreateAsync(newUser, user.Password);

            if (identityResult.Succeeded)
            {
                foreach (var role in user.Roles)
                {
                    await _userManager.AddToRoleAsync(newUser, role);
                }
            }
            return identityResult;
        }

        public IQueryable<User> GetAll()
        {
            return _userManager.Users;
        }

        public async Task<User> FindByIdAsync(string id)
        {
            //ToDo: Solve circular reference during serialization. Use AutoMapper
            return await _userManager.FindByIdAsync(id);
        }

        public string GetRandomPassword(int length)
        {
            var opts = new PasswordOptions()
            {
                RequiredLength = length,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "@$?_-"                        // non-alphanumeric
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public async Task<IdentityResult> UserPasswordChange(UpdatePasswordRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }
            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword,request.NewPassword);
            return result;
        }
    }
}
