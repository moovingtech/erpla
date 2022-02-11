using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Core.Domain;

namespace Presentation.Controllers
{
    [Route("api/Authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }
        [HttpGet]
        [Route("seed")]
        [Authorize]
        public async Task<IActionResult> Seed()
        {
            if (!userManager.Users.Any())
            {

                var newUser = new User
                {
                    Email = "test@demo.com",
                    FirstName = "Test",
                    LastName = "User",
                    UserName = "test.demo"
                };

                await userManager.CreateAsync(newUser, "P@ss.W0rd");
                await roleManager.CreateAsync(new IdentityRole
                {
                    Name = "Admin"
                });
                await roleManager.CreateAsync(new IdentityRole
                {
                    Name = "AnotherRole"
                });

                await userManager.AddToRoleAsync(newUser, "Admin");
                await userManager.AddToRoleAsync(newUser, "AnotherRole");
            }
            return Ok("Hello");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(AuthenticateRequest authenticationRequest)
        {
            // ToDo: Move this code to a service
            var user = await userManager.FindByNameAsync(authenticationRequest.UserName);

            if (user is null || !await userManager.CheckPasswordAsync(user, authenticationRequest.Password))
            {
                return Forbid();
            }

            var roles = await userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(720),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

            return Ok(new
            {
                AccessToken = jwt
            });
        }
    }
}
