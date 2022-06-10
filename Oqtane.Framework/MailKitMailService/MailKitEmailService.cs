using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Oqtane.Repository;
using Oqtane.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Oqtane.Infrastructure
{
    public class MailKitEmailService : IEmailService
    {
        private readonly ISettingRepository _settingRepository;

        public MailKitEmailService(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public async Task<string> Send(int siteId, string to, string subject, string html, string from = null)
        {
            string log = string.Empty;
            var siteSettings = _settingRepository.GetSettings(EntityNames.Site, siteId).ToList();
            var smtpHost = siteSettings.FirstOrDefault(x => x.SettingName == "SMTPHost")?.SettingValue;
            var smtpPort = Convert.ToInt32(siteSettings.FirstOrDefault(x => x.SettingName == "SMTPPort")?.SettingValue);
            var smtpSSL = Convert.ToBoolean(siteSettings.FirstOrDefault(x => x.SettingName == "SMTPSSL")?.SettingValue);
            if (from == null)
                from = siteSettings.FirstOrDefault(x => x.SettingName == "SMTPSender")?.SettingValue;

            if (!string.IsNullOrWhiteSpace(smtpHost) && smtpPort > 0 && !string.IsNullOrWhiteSpace(from))
            {
                var smtpUser = siteSettings.FirstOrDefault(x => x.SettingName == "SMTPUsername")?.SettingValue;
                var smtpPassword = siteSettings.FirstOrDefault(x => x.SettingName == "SMTPPassword")?.SettingValue;

                // create message
                var email = new MimeMessage();
                try
                {
                    email.From.Add(MailboxAddress.Parse(from));
                }
                catch
                {
                    log += "Invalid address specified in from";
                    return log;
                }
                try
                {
                    email.To.Add(MailboxAddress.Parse(to));
                }
                catch
                {
                    log += "Invalid address specified in to";
                    return log;
                }
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = html };

                // send email
                using var smtp = new SmtpClient();
                try
                {
                    await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.Auto);
                    await smtp.AuthenticateAsync(smtpUser, smtpPassword);
                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                }
                catch(Exception ex)
                {
                    log += ex.Message + "<br />";
                }
            }
            else
                log += "SMTP Not Configured Properly In Site Settings - Host, Port, And Sender Are All Required" + "<br />";
            return log;
        }
    }
}
