using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public ApplicationUser User { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
    }
}