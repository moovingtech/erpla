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
using Swashbuckle.AspNetCore.Annotations;

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
        [SwaggerOperation(Summary ="Usuario Seed",
                          Description ="Si no existe usuarios resgistrados en el sistema crea un usuario Administrador.")]
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
        [SwaggerOperation(Summary = "Usuario Inicio Sesion",
                          Description = "Crea el token de usuario, valida si el usuario existe y si la password es correcta")]
        [SwaggerResponse(200,"Datos correctos, token genrado")]
        [SwaggerResponse(400,"El usuario no se encuentro")]
        [SwaggerResponse(401,"Password incorrecta")]
        public async Task<IActionResult> Login(AuthenticateRequest authenticationRequest)
        {
            // ToDo: Move this code to a service
            var user = await userManager.FindByNameAsync(authenticationRequest.UserName);

            if (user is null)
            {
                return BadRequest(new Response() { Success = false, Message = "Usuario inválido" });
            }

            if (!await userManager.CheckPasswordAsync(user, authenticationRequest.Password))
            {
                return Unauthorized(new Response() { Success = false, Message = "Contraseña incorrecta" });
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

            return Ok(new Response() { Success = true, Data = jwt });
        }
    }
}
