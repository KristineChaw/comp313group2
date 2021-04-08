using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    /// <summary>
    /// A user of the GetAccredited application.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OrganizationId { get; set; }

        /// <summary>
        /// Returns a string representation of this ApplicationUser.
        /// </summary>
        /// <returns>The full name of this ApplicationUser instance</returns>
        public override string ToString() => $"{FirstName} {LastName}";
    }
}