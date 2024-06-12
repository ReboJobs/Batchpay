using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace XeroApp.Services
{
    public class EmailSender
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;

        public EmailSender(
            IConfiguration Configuration,
            IHttpContextAccessor HttpContextAccessor)
        {
            configuration = Configuration;
            httpContextAccessor = HttpContextAccessor;
        }
        public async Task SendEmail(
            string EmailSubject,
            string EmailAddressTo,
            string body)
        {
            try
            {
                // Email settings.
                var apiKey = configuration["SendGrid:APIKEY"];
                var senderEmail = configuration["SendGrid:SenderEmail"];
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("noreply@batchpay.com.au", "Batch Pay");
                var to = new EmailAddress(EmailAddressTo, EmailAddressTo);
                //var htmlContent = "<strong>Task ID: " + TaskId + "</strong>";
                var msg = MailHelper.CreateSingleEmail(from, to, EmailSubject, null, body);
                var response = await client.SendEmailAsync(msg);
            }
            catch
            {
                // Could not send email.
                // Perhaps SENDGRID_APIKEY not set in
                // appsettings.json
            }
        }
    }
}