using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    // This class represents an appointment rescheduling or cancellation request made by a student.
    public class Request
    {
        public int RequestId { get; set; }

        public Booking Booking { get; set; }
        public Appointment NewAppointment { get; set; } // this is null if it's a cancellation request
        public bool IsCancel { get => NewAppointment == null; }
    }
}