using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Sim_Card_Managment.Repos.MailConfigurationRepos;
using Sim_Card_Managment.Settings;

namespace Sim_Card_Managment.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _fallbackSettings;
        private readonly IMailConfigurationRepo _mailConfigurationRepo;

        public SmtpEmailService(IOptions<EmailSettings> fallbackSettings, IMailConfigurationRepo mailConfigurationRepo)
        {
            _fallbackSettings = fallbackSettings.Value;
            _mailConfigurationRepo = mailConfigurationRepo;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Prefer whatever's marked active in the MailConfiguration table; only
            // fall back to appsettings/user-secrets if nothing's been configured yet.
            var active = await _mailConfigurationRepo.GetActiveAsync();

            string smtpHost, senderEmail, senderPassword, senderDisplayName;
            int smtpPort;
            bool enableSsl;

            if (active != null)
            {
                smtpHost = active.SmtpHost;
                smtpPort = active.SmtpPort;
                senderEmail = active.SenderEmail;
                senderPassword = active.SenderPassword;
                senderDisplayName = active.SenderDisplayName;
                enableSsl = active.EnableSsl;
            }
            else
            {
                smtpHost = _fallbackSettings.SmtpHost;
                smtpPort = _fallbackSettings.SmtpPort;
                senderEmail = _fallbackSettings.SenderEmail;
                senderPassword = _fallbackSettings.SenderPassword;
                senderDisplayName = _fallbackSettings.SenderDisplayName;
                enableSsl = true;
            }

            using var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderDisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}