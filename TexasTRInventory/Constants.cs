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
            public static readonly string AdminInitializer = "AdminInitializer";
        }

        public static class ConfigNames
        {
            public static readonly string AdminUsername = "AdminUsername";
        }

        public static class Roles
        {
            public static readonly string Administrator = "Administrator";
            public static readonly string InternalUser = "InternalUser";
            public static readonly string Supplier = "Supplier";

            public static string[] ToArray() {
                string[] ret =  { Administrator, InternalUser, Supplier};
                return ret;
            }
        }
    }
}
