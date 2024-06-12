using IronPdf;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using XeroApp.Models.BusinessModels;

namespace XeroApp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(Dictionary<string, SelectPdf.PdfDocument> docs, List<RemittanceAdviceModel> remittances, string organisationEmail, string organisationName);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;

        public EmailService(
            IConfiguration Configuration,
            IHttpContextAccessor HttpContextAccessor)
        {
            configuration = Configuration;
            httpContextAccessor = HttpContextAccessor;

        }
        public async Task SendEmailAsync(Dictionary<string, SelectPdf.PdfDocument> docs, List<RemittanceAdviceModel> remittances, string organisationEmail, string organisationName)
        {
                var groupEmail = docs.GroupBy(docs => docs.Key);
                EmailSender emailSender = new EmailSender(configuration, httpContextAccessor);
                var apiKey = configuration["SendGrid:APIKEY"];
                var senderEmail = configuration["SendGrid:SenderEmail"];
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(organisationEmail, organisationName);

                foreach (var item in groupEmail)
                {
                    var emailTo = remittances.Where(r => r.ContactId.ToString() == item.Key).Select(c => c.ContactEmailAddress).FirstOrDefault();
                    var to = new EmailAddress(emailTo, emailTo);
                    var contactName = remittances.Where(r => r.ContactId.ToString() == item.Key).Select(c => c.ContactName).FirstOrDefault();
                    var total = remittances.Where(r => r.ContactId.ToString() == item.Key).Select(c => c.Total).FirstOrDefault();
                    var myCompany = remittances.Where(r => r.ContactId.ToString() == item.Key).Select(c => c.MyCompanyName).FirstOrDefault();
                    var paymentDate = remittances.Where(r => r.ContactId.ToString() == item.Key).SelectMany(r => r.Invoices).Select(i => i.PaymentDate).FirstOrDefault();

              
                    //var subject = $"Payment has been made by {myCompany} for {contactName} for $AUD {total}";
                    var subject = $"Payment will be made on {paymentDate} by {myCompany} for {contactName} for $AUD {total}";
                    var body = $"Hi {contactName}, <br/><br/>" +
                            $"Here's your remittance advice for payment of  $AUD {total}. <br/><br/>" +
                            $"If you have any questions, please let us know. <br/><br/>" +
                            $"Thanks, <br/><br/>" +
                            $"{myCompany} <br/>";

                    int counter = 1;

                    foreach (var pdf in item)
                    {
                        await File.WriteAllBytesAsync($"GeneratedFiles/RemittanceAdvice/InvoiceRemittance{counter}_{item.Key}.pdf", pdf.Value.Save());

                        using (var fileStream = File.OpenRead($"GeneratedFiles/RemittanceAdvice/InvoiceRemittance{counter}_{item.Key}.pdf"))
                        {

                            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, body);
                            await msg.AddAttachmentAsync($"GeneratedFiles/RemittanceAdvice/InvoiceRemittance{counter}_{item.Key}.pdf", fileStream);
                            counter++;
                            var response = await client.SendEmailAsync(msg);
                        }
                    }

                }
        }
    }
}