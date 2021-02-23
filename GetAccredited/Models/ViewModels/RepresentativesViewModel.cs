using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.ViewModels
{
    public class RepresentativesViewModel
    {
        public Organization Organization { get; set; }
        public IEnumerable<ApplicationUser> Representatives { get; set; }

        [Required]
        public string Email { get; set; }
    }
}