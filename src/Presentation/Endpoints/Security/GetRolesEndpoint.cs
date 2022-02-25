using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Core.Security.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MinimalApi.Endpoint;
//using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Security;

/// <summary>
/// Authenticates a user
/// </summary>
public class GetRolesResponse : IEndpoint<IResult>
{
    private RoleService _roleService;

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/roles",[Authorize(Policy = "roles.view")] async
           (RoleService roleService) =>
           {
               _roleService = roleService;
               return await HandleAsync();
           })
          .Produces<IQueryable<IdentityRole>>()
          .WithTags("RolesEndpoint");
    }

    public async Task<IResult> HandleAsync()
    {
        var roles =  _roleService.GetAll();
        return Results.Ok(roles);
    }
}
