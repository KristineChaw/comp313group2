using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public string StudentId { get; set; }
        public Appointment Appointment { get; set; }
        public Accreditation Accreditation { get; set; }
        public DateTime DateBooked { get; set; }
        public bool IsCancelled { get; set; }
    }
}