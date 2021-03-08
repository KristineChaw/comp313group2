using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
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

        public override string ToString() => Name;

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