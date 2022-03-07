using AutoMapper;
using Core.Domain;
using Core.Security.Domain.Dto;
using Core.Security.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.Security.Application.Service
{
    public class RoleService
    {
        public static string PERMISSION_CLAIM_TYPE = "Auth";
        private readonly RoleManager<IdentityRole> _roleManager;
        //private readonly IConfiguration _configuration;
        private IMapper _mapper;

        public RoleService(RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public IQueryable<RoleDto> GetAll()
        {
            var rolesDto = _mapper.Map<List<RoleDto>>(_roleManager.Roles);
            return rolesDto.AsQueryable();
        }

        public async Task<IList<IdentityResult>> AddClaim(string roleId, IList<string> claims)
        {
            // ToDo guard input parameters
            // Test: What happens if the role does not exist. If the claim already exist
            List<IdentityResult> results = new List<IdentityResult>();
            var role = await _roleManager.FindByIdAsync(roleId);
            foreach (var claim in claims)
            {
                var result = await _roleManager.AddClaimAsync(role, new Claim(PERMISSION_CLAIM_TYPE, claim));
                results.Add(result);
            }
            return results;
        }
    }
}
