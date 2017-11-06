using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNet.Identity;
using System.ComponentModel;

namespace TexasTRInventory.Models
{
    public class ApplicationUser : IdentityUser, IUser<String>
    {

        [DisplayName("Is User Disabled?")]
        public bool IsDisabled { get; set; }

        [DisplayName("Employer")]
        public int? EmployerID { get; set; }

        public Company Employer { get; set; }

        [DisplayName("Is Email Confirmed?")]
        public override bool EmailConfirmed { get => base.EmailConfirmed; set => base.EmailConfirmed = value; }
    }
}
