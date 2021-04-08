using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    /// <summary>
    /// An organization offers accreditations to students and other job-seekers.
    /// </summary>
    public class Organization
    {
        public String OrganizationId { get; set; }
        public String Name { get; set; }
        public String Acronym { get; set; }
        public String Description { get; set; }
        public String Logo { get; set; }
        public String WebsiteUrl { get; set; }

        /// <summary>
        /// Returns a string representation of this organization.
        /// </summary>
        /// <returns>A string that represents this organization instance</returns>
        public override String ToString() => $"{Name} ({Acronym})";
    }
}