using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Text;
using System.Threading.Tasks;

namespace Core.Security.Application.Service.Mailing
{
    public interface IMailerService
    {
        MimeMessage BuildMessageFromData(EmailData emailData);

        bool Send(MimeMessage message);
    }
    public class MailerService : IMailerService
    {
        private readonly SmtpConfig _smtpConfig;
        public MailerService(SmtpConfig config)
        {
            _smtpConfig = config;
        }

        public MimeMessage BuildMessageFromData(EmailData emailData)
        {
            MimeMessage emailMessage = new MimeMessage();
            MailboxAddress emailFrom = new MailboxAddress("Test", _smtpConfig.SmtpEmailFrom);
            emailMessage.From.Add(emailFrom);
            MailboxAddress emailTo = new MailboxAddress(emailData.EmailToName, emailData.EmailTo);
            emailMessage.To.Add(emailTo);
            emailMessage.Subject = emailData.EmailSubject;
            BodyBuilder emailBodyBuilder = new BodyBuilder();
            emailBodyBuilder.TextBody = emailData.EmailBody;
            emailMessage.Body = emailBodyBuilder.ToMessageBody();

            return emailMessage;
        }

        public bool Send(MimeMessage message)
        {
            SmtpClient emailClient = new SmtpClient();
            try
            {
                emailClient.Connect(_smtpConfig.SmtpServer, int.Parse(_smtpConfig.SmtpPort), _smtpConfig.SmtpUseSSL);
                emailClient.Authenticate(_smtpConfig.SmtpUser, _smtpConfig.SmtpUserPassword);
                emailClient.Send(message);
                emailClient.Disconnect(true);
                emailClient.Dispose();
                return true;
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
        }
    }
}
