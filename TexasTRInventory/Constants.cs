using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TexasTRInventory.Constants
{
    public static class SecretNames
    {
        public const string AdminInitializer = "AdminInitializer";
    }

    public static class ClaimNames
    {
        public const string IsInternal = "IsInternal";
        public const string EmployerID = "EmployerID";
        public const string IsAdmin = "IsAdmin";
    }

    public static class PolicyNames
    {
        public const string IsInternal = "IsInternal";
        public const string OnlyAdminsEditAdmins = "OnlyAdminsEditAdmins";
        public const string IsAdmin = "IsAdmin";
        public const string CanEditLockedProduct = "CanEditLockedProduct";
    }
        
    public static class KeyNames
    {
        public const string ErrorDetails = "ErrorDetails";
        public const string SupplierID = "SupplierID";
    }

    public static class GenConfig
    {
        public const string TexasTRCompanyName = "Texas TR";
    }
    

}
