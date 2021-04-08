using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    /// <summary>
    /// An appointment rescheduling or cancellation request made by a student account.
    /// </summary>
    public class Request
    {
        public int RequestId { get; set; }
        public Booking Booking { get; set; }
        public Appointment NewAppointment { get; set; }

        /// <summary>
        /// Determines whether this is a cancellation request or not.
        /// </summary>
        public bool IsCancel { get => NewAppointment == null; }
    }
}