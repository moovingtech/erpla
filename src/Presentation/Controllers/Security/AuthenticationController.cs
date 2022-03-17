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

        private readonly AuthenticationService _authenticationService;

        public AuthenticationController(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpGet]
        [Route("seed")]
        [Authorize]
        [SwaggerOperation(Summary = "Usuario Seed",
                          Description = "Si no existe usuarios resgistrados en el sistema crea un usuario Administrador.")]
        public async Task<IActionResult> Seed()
        {
            _ = await _authenticationService.Seed();
            return Ok("Seeded");
        }

        [HttpPost]
        [Route("login")]
        [SwaggerOperation(Summary = "Usuario Inicio Sesion",
                          Description = "Crea el token de usuario, valida si el usuario existe y si la password es correcta")]
        [SwaggerResponse(200, "Datos correctos, tokens generados")]
        [SwaggerResponse(400, "Usuario/Password incorrecto")]
        public async Task<IActionResult> Login(AuthenticateRequest authenticationRequest)
        {
            // ToDo: Move this code to a service
            var response = await _authenticationService.Login(authenticationRequest);
            if (response == null)
            {
                return BadRequest("Usuario/Password incorrecto.");
            }
            //setTokenCookie(response.Value.RefreshToken);

            return Ok(response.Value);
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel tokenModel)
        {
            var response = await _authenticationService.RefreshTokens(tokenModel);
            if (response == null)
                return BadRequest("Invalid access token or refresh token");

            return Ok(response);
        }


        /*        private void setTokenCookie(string token)
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
                }*/
    }
}
