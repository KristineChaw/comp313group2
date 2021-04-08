using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    /// <summary>
    /// An accreditation is a skill evaluation certification being offered by an organization.
    /// </summary>
    public class Accreditation
    {
        public int AccreditationId { get; set; }
        public Organization Organization { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatorId { get; set; }
        public string Type { get; set; }
        public string Eligibility { get; set; }
        public string EligibilityFileURL { get; set; }

        /// <summary>
        /// Returns a string representation of this accreditation.
        /// </summary>
        /// <returns>String representation of this accreditation instance</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Returns a list of string that represent the types of accreditation being offered by the system.
        /// </summary>
        /// <returns>List of accreditation types</returns>
        public static List<string> GetTypes()
        {
            return new List<string>
            {
                "Agriculture, Food and Natural Resources",
                "Architecture and Construction",
                "Arts, Audio/Video Technology and Communications",
                "Business Management and Administration",
                "Education and Training",
                "Finance",
                "Government and Public Administration",
                "Health Science",
                "Hospitality and Tourism",
                "Human Services",
                "Information Technology",
                "Law, Public Safety, Corrections and Security",
                "Manufacturing",
                "Marketing, Sales and Service",
                "Science, Technology, Engineering and Mathematics",
                "Transportation, Distribution and Logistics"
            };
        }
    }
}