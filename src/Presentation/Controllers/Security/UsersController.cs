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
            user.Password = GetRandomPassword(8);

            // ToDo: Handle ApplicationException (user already exists). Return 400 instead of 500
            var result = await _userService.CreateUserAsync(user);

            //call to mailer service here to notify to user.
            if (result.Succeeded)
            {
                SendRegistrationMail(user);
            }

            return Ok("User Created ");
        }

        private void SendRegistrationMail(AddUserRequest user)
        {
            var emailData = new EmailData();
            emailData.EmailSubject = "Alta Usuario";
            emailData.EmailToName = $"{user.FirstName} {user.LastName}";
            emailData.EmailTo = user.Email;
            emailData.EmailBody = BuildRegistrationMailBody(user);
            _mailerService.Send(emailData);
        }

        private string BuildRegistrationMailBody(AddUserRequest user)
        {
            string body;
            using (StreamReader reader = new StreamReader(Directory.GetCurrentDirectory() + "/templates/Email/UserRegistred.html"))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{NombreyApellido}", $"{user.FirstName} {user.LastName}"); //replacing the required things  
            body = body.Replace("{Nombre_de_usuario}", user.UserName);
            body = body.Replace("{Contrasena}", user.Password);
            return body;
        }
        private static string GetRandomPassword(int length)
        {
            var opts = new PasswordOptions()
            {
                RequiredLength = length,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "@$?_-"                        // non-alphanumeric
        };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}
