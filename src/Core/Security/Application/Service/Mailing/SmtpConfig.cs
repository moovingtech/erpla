using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Security.Application.Service.Mailing
{
    /// <summary>
    /// ToDo:Change file location
    /// </summary>
    public class SmtpConfig
    {
        public string SmtpServer { get; set; }

        public string SmtpPort { get; set; }

        public string SmtpUser { get; set; }

        public string SmtpUserPassword { get; set; }

        public string SmtpEmailFrom { get; set; }

        public bool SmtpUseSSL { get; set; }
    }
}
