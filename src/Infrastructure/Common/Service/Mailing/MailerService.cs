using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Text;
using System.Threading.Tasks;
using Core.Domain;

namespace Infrastructure.Common.Service.Mailing
{
    public interface IMailerService
    {
        Task<bool> SendRegistrationMail(AddUserRequest user);
    }
    public class MailerService : IMailerService
    {
        private readonly SmtpConfig _smtpConfig;
        public MailerService(SmtpConfig config)
        {
            _smtpConfig = config;
        }

        private MimeMessage BuildMessageFromData(EmailData emailData)
        {
            MimeMessage emailMessage = new MimeMessage();
            MailboxAddress emailFrom = new MailboxAddress(_smtpConfig.SmtpEmailFrom, _smtpConfig.SmtpEmailFrom);
            emailMessage.From.Add(emailFrom);
            MailboxAddress emailTo = new MailboxAddress(emailData.EmailToName, emailData.EmailTo);
            emailMessage.To.Add(emailTo);
            emailMessage.Subject = emailData.EmailSubject;
            emailMessage.Body = new TextPart("html")
            {
                Text = emailData.EmailBody
            };
            return emailMessage;
        }

        private async Task<bool> Send(EmailData emailData)
        {
            var message = BuildMessageFromData(emailData);
            SmtpClient emailClient = new SmtpClient();
            try
            {
                emailClient.Connect(_smtpConfig.SmtpServer, int.Parse(_smtpConfig.SmtpPort), _smtpConfig.SmtpUseSSL);
                emailClient.Authenticate(_smtpConfig.SmtpUser, _smtpConfig.SmtpUserPassword);
                await emailClient.SendAsync(message);
            }
            catch
            {
                return false;
            }
            finally
            {
                emailClient.Disconnect(true);
                emailClient.Dispose();
            }
            return true;
        }
        public async Task<bool> SendRegistrationMail(AddUserRequest user)
        {
            var emailData = new EmailData();
            emailData.EmailSubject = "Alta Usuario";
            emailData.EmailToName = $"{user.FirstName} {user.LastName}";
            emailData.EmailTo = user.Email;
            emailData.EmailBody = BuildRegistrationMailBody(user);
          return await Send(emailData);
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
    }
}
