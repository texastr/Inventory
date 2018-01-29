using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNet.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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

        [DisplayName("Is Admin User?")]
        [NoExternalAdmins]
        public bool IsAdmin { get; set; }
    }

    public class NoExternalAdminsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool isAdmin = (bool)value;
            ApplicationUser user = (ApplicationUser)validationContext.ObjectInstance;

            //It's janky to hardcode in the one. but this is just for a corner case on a rare admin workflow. I'm going to let it slide.
            if (isAdmin && user.EmployerID != 1)
            {
                return new ValidationResult("An admin user must be employeed by Texas TR");
            }

            return ValidationResult.Success;
        }
    }
}
