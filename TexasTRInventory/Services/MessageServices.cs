using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace TexasTRInventory.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link https://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender //EXP 9.2.17. This no longer implements the interface ISmsSender.
    {
        //EXP 8.2.17. I give up. I'll just paste in the extra field and the other code that they had me put in EmailSender.cs
        /*public AuthMessageSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }*/
        //EXP 9.11.17. I have no recollection what went into the comment from 8.2.17
        
        //9.11.7 fuck the secret store
        //public AuthMessageSenderOptions Options { get; } //comment from the site: set only via Secret Manager.


        public async Task<Response> SendEmailAsync(string email, string subject, string message)
        {
            // Plug in your email service here to send an email.
            //return Task.FromResult(0);
            //EXP 9.2.17. Changing a bit from the provided code. using azure key vault instead of user secret store
            string apiKey = await GlobalCache.GetSendGridKey();
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = GlobalCache.GetSystemEmailAddress(),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));
            return await client.SendEmailAsync(msg);
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
