
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

namespace Presentation.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        //private readonly UserManager<User> userManager;
        //private readonly RoleManager<IdentityRole> roleManager;
        //private readonly IConfiguration _configuration;
        private readonly UserService _userService;  

        public UsersController(UserService userService)
        {
            this._userService = userService;
        }
        //public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        //{
        //    this.userManager = userManager;
        //    this.roleManager = roleManager;
        //    _configuration = configuration;
        //}

        [HttpGet]
        [Route("")]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok( users);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(string Id)
        {
            var user = _userService.FindByIdAsync(Id);
            return Ok(user);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest user)
        {
            // ToDo: Handle ApplicationException (user already exists). Return 400 instead of 500
            var result = await _userService.CreateUserAsync(user);
            return Ok(new Response { Status = "Success", Message = "User Created " });
        }

    }
}
