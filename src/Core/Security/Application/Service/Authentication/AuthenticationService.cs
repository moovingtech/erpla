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
using System.Security.Cryptography;

namespace Core.Application.Service
{
    public class AuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private const int SessionTime = 15;

        public AuthenticationService(UserManager<User> userManage, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManage;
            _configuration = configuration;
        }
        public async Task<AuthenticateResponse?> Login(AuthenticateRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);

            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}")
            };

            foreach (var roleName in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
                var role = await _roleManager.FindByNameAsync(roleName);
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in roleClaims)
                {
                    claims.Add(new Claim(claim.Type, claim.Value));
                }
            }
            _ = int.TryParse(_configuration["Jwt:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
            var token = CreateToken(claims);
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

            await _userManager.UpdateAsync(user);
            return new AuthenticateResponse()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = user.RefreshToken,
                ValidTo = token.ValidTo
            };
        }

        public async Task<bool> Seed()
        {
            if (!_userManager.Users.Any())
            {

                var newUser = new User
                {
                    Email = "test@demo.com",
                    FirstName = "Test",
                    LastName = "User",
                    UserName = "test.demo"
                };

                await _userManager.CreateAsync(newUser, "P@ss.W0rd");
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = "Admin"
                });
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = "AnotherRole"
                });

                await _userManager.AddToRoleAsync(newUser, "Admin");
                await _userManager.AddToRoleAsync(newUser, "AnotherRole");
            }
            return true;
        }

        public async Task<TokenModel?> RefreshTokens(TokenModel tokenModel)
        {
            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;

            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return null;
            }

            string username = principal.Identity.Name;

            var user = await _userManager.FindByNameAsync(username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return null;
            }

            var newAccessToken = CreateToken(principal.Claims.ToList());
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);
            return new TokenModel()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                RefreshToken = newRefreshToken
            };
        }

        public bool ValidateRefreshToken(string refreshToken)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:Key"])),
                ValidIssuer = _configuration["jwt:Issuer"],
                ValidAudience = _configuration["jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            };

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            try
            {
                jwtSecurityTokenHandler.ValidateToken(refreshToken, validationParameters,
                    out SecurityToken token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["jwt:Issuer"],
                audience: _configuration["jwt:Audience"],
                expires: DateTime.Now.AddMinutes(SessionTime),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:Key"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;

        }
    }
}
