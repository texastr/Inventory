using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TexasTRInventory.Services
{
    public interface IEmailSender
    {
        //EXP 9.2.17 Changed the interface definition from Task to Task<Response>. This was caused ultimately by secret retrieval being async
        Task<Response> SendEmailAsync(string email, string subject, string message);
    }
}
