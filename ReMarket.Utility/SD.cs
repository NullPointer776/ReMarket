using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMarket.Utility
{
    /// <summary>
    /// Application-wide string constants shared across projects.
    /// </summary>
    public static class SD
    {
        public const string Role_Admin = "Admin";
        public const string Role_Seller = "Seller";
        public const string Role_Buyer = "Buyer";

        /// <summary>Fallback admin email when configuration <c>Seed:AdminEmail</c> is not set.</summary>
        public const string DefaultAdminEmail = "admin@remarket.local";
    }
}
