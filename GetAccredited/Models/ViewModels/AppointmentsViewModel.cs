using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.ViewModels
{
    public class AppointmentsViewModel
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public DateTime Start { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public int TimeInBetween { get; set; }
    }
}