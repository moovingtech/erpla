
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
using Core.Application.Service;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Controllers.Security
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Sección para Administradores autenticados")]
    public class UsersController : ControllerBase
    {
        //private readonly UserManager<User> userManager;
        //private readonly RoleManager<IdentityRole> roleManager;
        //private readonly IConfiguration _configuration;
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("")]
        [SwaggerOperation(Summary = "Lista de usuarios", 
                          Description = "Retorna la lista de usuarios completa.")]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(new Response { Success = true, Data = users });
        }

        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation(Summary = "Obtener usuario", 
                          Description ="Obtiene un usuario atravez del Id.")]
        public async Task<IActionResult> GetById(string Id)
        {
            var user = await _userService.FindByIdAsync(Id);
            return Ok(new Response { Success = true, Data = user });
        }

        [HttpPost]
        [Route("")]
        [SwaggerOperation(Summary = "Agregar usuario",
                          Description = "Registra un nuevo usuario en el sistema.")]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest user)
        {
            // ToDo: Handle ApplicationException (user already exists). Return 400 instead of 500
            var result = await _userService.CreateUserAsync(user);
            return Ok(new Response { Success = true, Message = "User Created " });
        }

    }
}
