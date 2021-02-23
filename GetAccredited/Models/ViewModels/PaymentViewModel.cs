using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.ViewModels
{
    public class PaymentViewModel
    {
        public Accreditation Accreditation { get; set; }
        public Appointment Appointment { get; set; }
        public Card Card { get; set; }
        public bool SaveInfo { get; set; }
    }
}