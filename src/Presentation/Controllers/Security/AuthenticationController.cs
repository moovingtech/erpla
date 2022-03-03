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
using Core.Security.Domain.Entities;
using Core.Application.Service;

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
        [SwaggerOperation(Summary = "Usuario Seed",
                          Description = "Si no existe usuarios resgistrados en el sistema crea un usuario Administrador.")]
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
            return Ok("Seeded");
        }

        [HttpPost]
        [Route("login")]
        [SwaggerOperation(Summary = "Usuario Inicio Sesion",
                          Description = "Crea el token de usuario, valida si el usuario existe y si la password es correcta")]
        [SwaggerResponse(200, "Datos correctos, token genrado")]
        [SwaggerResponse(400, "El usuario no se encuentro")]
        [SwaggerResponse(401, "Password incorrecta")]
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

            foreach (var roleName in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
                var role = await roleManager.FindByNameAsync(roleName);
                var roleClaims = await roleManager.GetClaimsAsync(role);
                foreach (var claim in roleClaims)
                {
                    claims.Add(new Claim(claim.Type, claim.Value));
                }
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
            var refreshToken = JwtUtils.GenerateRefreshToken(ipAddress());
            setTokenCookie(refreshToken.Token);

            return Ok(new Response() { Success = true, Data = jwt });
        }
        /*
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _userService.RefreshToken(refreshToken, ipAddress());
            setTokenCookie(response.RefreshToken);
            return Ok(response);
        }
        */

        private void setTokenCookie(string token)
        {
            // append cookie with refresh token to the http response
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
        private string ipAddress()
        {
            // get source ip address for the current request
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
