using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.Domain;
using Core.Application.Service;
using Swashbuckle.AspNetCore.Annotations;
using Infrastructure.Common.Service.Mailing;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Identity;

namespace Presentation.Controllers.Security
{
    [Route("api/users")]
    [ApiController]

    [SwaggerTag("Sección para Administradores autenticados")]
    public class UsersController : ControllerBase
    {
        //private readonly UserManager<User> userManager;
        //private readonly RoleManager<IdentityRole> roleManager;
        //private readonly IConfiguration _configuration;
        private readonly UserService _userService;
        private readonly IMailerService _mailerService;

        public UsersController(UserService userService, IMailerService mailerService)
        {
            _userService = userService;
            _mailerService = mailerService;
        }

        [HttpGet]
        [Route("")]
        [SwaggerOperation(Summary = "Lista de usuarios",
                          Description = "Retorna la lista de usuarios completa.")]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation(Summary = "Obtener usuario",
                          Description = "Obtiene un usuario atravez del Id.")]
        public async Task<IActionResult> GetById(string Id)
        {
            var user = await _userService.FindByIdAsync(Id);
            return Ok(user);
        }

        [HttpPost]
        [Route("")]
        [SwaggerOperation(Summary = "Agregar usuario",
                          Description = "Registra un nuevo usuario en el sistema.")]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest user)
        {
            // ToDo: Handle ApplicationException (user already exists). Return 400 instead of 500
            var result = await _userService.CreateUserAsync(user);

            //call to mailer service here to notify to user.
            if (result.Succeeded)
            {
                _mailerService.SendRegistrationMail(user);
            }

            return Ok("User Created ");
        }
    }
}
