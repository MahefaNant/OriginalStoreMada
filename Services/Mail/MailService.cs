using System.Net;
using System.Net.Mail;

namespace originalstoremada.Services.Mail;

public class MailService
{
    public static void EnvoyerEmail(SmtpConfig smtpConfig, string expediteur, string destinataire, string sujet, string corps)
    {
        using (SmtpClient smtpClient = new SmtpClient(smtpConfig.Server, smtpConfig.Port))
        {
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(smtpConfig.Username, smtpConfig.Password);
            smtpClient.EnableSsl = true;
            MailMessage message = new MailMessage
            {
                From = new MailAddress(expediteur),
                Subject = sujet,
                Body = corps,
                IsBodyHtml = true
            };
            message.To.Add(destinataire);
            smtpClient.Send(message);
        }
    }
}