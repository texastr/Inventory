//EXP 8.2.17 copied from https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?tabs=aspnet20%2Csqlserver

using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace TexasTRInventory.Services
{
    /*Commenting out this entire class while moving from user secret store to azure key vault. It seems that we use the MessageServices class instead
     * public class EmailSender : IEmailSender
    {
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options { get; } //comment from the site: set only via Secret Manager. Didn't I set it in the constructor above?

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(Options.SendGridKey, subject, message, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("alexander@texastr.com", "The name is hardcoded!"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));
            return client.SendEmailAsync(msg);
        }
    }*/
}
