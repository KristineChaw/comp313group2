using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    /// <summary>
    /// A booking is a record of an appointment booking by a student.
    /// </summary>
    public class Booking
    {
        public int BookingId { get; set; }
        public string StudentId { get; set; }
        public Appointment Appointment { get; set; }
        public Accreditation Accreditation { get; set; }
        public DateTime DateBooked { get; set; }

        /// <summary>
        /// Determines whether this booking has been cancelled.
        /// </summary>
        public bool IsCancelled { get; set; }
    }
}