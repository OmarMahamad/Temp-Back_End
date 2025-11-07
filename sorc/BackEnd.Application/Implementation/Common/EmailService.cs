using BackEnd.Application.Common;
using BackEnd.Application.DTOs.Common;
using Microsoft.Extensions.Configuration;
using MailKit.Security;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace BackEnd.Application.Implementation.Common
{
    public class EmailService : IEmailService
    {
        private readonly string smtpServer;
        private readonly int smtpPort;
        private readonly string smtpUser;
        private readonly string smtpPass;

        public EmailService(IConfiguration configuration)
        {
            smtpServer = configuration["EmailSettings:SmtpServer"];
            smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
            smtpUser = configuration["EmailSettings:SmtpUser"];
            smtpPass = configuration["EmailSettings:SmtpPassword"];
        }

        public async Task<Response> SendEmailAsync(SandEmailDTO sandEmail)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("ERP System", smtpUser));
                message.To.Add(new MailboxAddress("", sandEmail.EmailTo));
                message.Subject = sandEmail.Subject;
                var bodyBuilder = new BodyBuilder { HtmlBody = sandEmail.Body };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUser, smtpPass);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return Response.Success();
            }
            catch (Exception ex)
            {
                return Response.Failure($"Error in Sand Email: {ex.Message}");
            }
        }
    }
}
