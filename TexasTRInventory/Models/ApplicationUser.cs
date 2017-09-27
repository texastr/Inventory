using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNet.Identity;
using System.ComponentModel;

namespace TexasTRInventory.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser, IUser<String>
    {

        public bool IsDisabled { get; set; }

        //EXP 9.20.17. Going back to using claims rather than a field. Or maybe both? I don't know
        public int? EmployerID { get; set; }

        public Company Employer { get; set; }

    }
}
