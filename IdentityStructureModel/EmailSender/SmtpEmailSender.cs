using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace IdentityStructureModel.EmailSender
{
    public class SmtpEmailSender : IEmailSender
    {
        private string host;
        private int port;
        private bool enableSSL;
        private string userName;
        private string password;
        public SmtpEmailSender(string host, int port, bool enableSsl, string userName, string password)
        {
            this.host = host;
            this.port = port;
            enableSSL = enableSsl;
            this.userName = userName;
            this.password = password;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = enableSSL,
                Timeout = int.MaxValue
            };
            await client.SendMailAsync(new MailMessage(userName, email, subject, htmlMessage)
            {
                IsBodyHtml = true,
            });

        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage, Attachment attachment)
        {
            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = enableSSL,
                Timeout = int.MaxValue
            };
            var mail = new MailMessage(userName, email, subject, htmlMessage);
            mail.Attachments.Add(attachment);
            await client.SendMailAsync(mail);
        }

        public async Task SendResetPasswordEmail(string email, string link)
        {
            var body = $"<h2>Şifrenizi sıfırlamak için lütlen <a href='{link}'>tıklayınız</a>";
            await SendEmailAsync(email, "Password Reset", body);
        }
    }
}
