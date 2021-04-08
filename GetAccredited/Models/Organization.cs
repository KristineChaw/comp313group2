using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    public class Organization
    {
        public String OrganizationId { get; set; }
        public String Name { get; set; }
        public String Acronym { get; set; }
        public String Description { get; set; }
        public String WebsiteUrl { get; set; }
        public String Logo { get; set; }
        public override String ToString() => $"{Name} ({Acronym})";
    }
}