using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using SendGrid.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booky.Utility
{
    public class EmailSender : IEmailSender
    {
        public string SendGridSecret { get; set; }
        public EmailSender(IConfiguration _iconfig)
        {
            SendGridSecret = _iconfig.GetSection("SendGrid:SecretKey").Get<string>();
        }


        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            
            
            var client = new SendGridClient(SendGridSecret);
            
            var from = new EmailAddress("dotnethamza@outlook.com", "Booky");
            var to = new EmailAddress(email);
            var message = MailHelper.CreateSingleEmail(from, to, subject,"",htmlMessage);

            var result = client.SendEmailAsync(message);
            return result;
        }
    }
}
