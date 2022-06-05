using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace IdentityStructureModel.HelperMethod
{
    public static class HelperClass
    {
        public static void SendResetPasswordEmail(string email, string link)
        {
            MailMessage mail = new MailMessage();
            SmtpClient smtpClient = new SmtpClient("smtp.office365.com",587);
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential("developerkaraca@outlook.com", "DvlprKrc");
            
            mail.From = new MailAddress("developerkaraca@outlook.com");
            mail.To.Add(email);
            mail.Subject = $"Şifre sıfırlama";
            mail.Body = $"<h2>Şifrenizi sıfırlamak için lütlen <a href='{link}'>tıklayınız</a>";
            mail.IsBodyHtml = true;
            smtpClient.Send(mail);
        }
    }
}
