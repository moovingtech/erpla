using Core.Domain;
using Core.Security.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
