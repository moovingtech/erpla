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
            //Generate password for user
            user.Password = _userService.GetRandomPassword();

            // ToDo: Handle ApplicationException (user already exists). Return 400 instead of 500
            var result = await _userService.CreateUserAsync(user);

            if (result.Succeeded)
            {
                await _mailerService.SendRegistrationMail(user);
            }
            return Ok(new Response() { Success = true, Message = "User Created" });
        }

        [HttpPut]
        [Route("update-password")]
        [SwaggerOperation(Summary = "Cambio de password para un usuario",
                          Description = "cambia la password de un usuario validando la nueva password mediante las políticas aplicadas")]
        public async Task<IActionResult> PasswordChange([FromBody] UpdatePasswordRequest req)
        {
            var user = HttpContext.User;
            if (user.Identity.Name == null)
            {
                return Unauthorized();
            }
            var result = await _userService.PasswordChange(req, user.Identity.Name.ToString());
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return Ok(new Response() { Success = true, Data = result });
        }


    }
}
