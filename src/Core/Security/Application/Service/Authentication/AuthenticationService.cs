using Core.Domain;
using Core.Security.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;

namespace Core.Application.Service
{
    public class AuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticationService(UserManager<User> userManage, RoleManager<IdentityRole> roleManager,IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManage;
            _configuration = configuration;
        }

    }
}
