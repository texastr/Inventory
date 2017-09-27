using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TexasTRInventory
{
    public static class Constants
    {
        public static class SecretNames
        {
            public const string AdminInitializer = "AdminInitializer";
        }

        public static class ClaimTypes
        {
            public const string IsInternal = "IsInternal";
            public const string EmployerID = "EmployerID";
        }

        public static class PolicyNames
        {
            public const string IsInternal = "IsInternal";
        }
        
        public static class KeyNames
        {
            public const string ErrorDetails = "ErrorDetails";
            public const string SupplierID = "SupplierID";
        }

        public const string TexasTRCompanyName = "Texas TR";
    }
}
