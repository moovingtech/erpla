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
using Core.Security.Application.Service;

namespace Presentation.Controllers
{
    [Route("api/roles")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RolesController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetAll()
        {
            var roles = _roleService.GetAll();
            return Ok(roles);
        }

        [HttpPost]
        [Route("{roleId}/claims")]
        [Authorize(Policy = "claims.create")]
        public async Task<IActionResult> addClaims([FromRouteAttribute] string roleId, [FromBody] IList<string> claims)
        {
            await _roleService.AddClaim(roleId, claims);
            return Ok();
        }

    }
}
