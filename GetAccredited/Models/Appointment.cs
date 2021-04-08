using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    /// <summary>
    /// An appointment is booked by student users and are created by representatives.
    /// </summary>
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public Organization Organization { get; set; }
        public string StudentId { get; set; }
        public DateTime Date { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        /// <summary>
        /// Determines whether this appointment is booked or not.
        /// </summary>
        public bool IsBooked { get => StudentId != null; }

        /// <summary>
        /// Determines whether this appointment is in the past or not.
        /// </summary>
        public bool IsPast { get => Date.AddHours(Start.Hour).AddMinutes(Start.Minute) < DateTime.Now; }

        /// <summary>
        /// Determines whether this appointment conflicts with another appointment.
        /// </summary>
        /// <param name="other">The appointment being compared with this instance</param>
        /// <returns>true if there are scheduling conflicts between the two appointments; otherwise, false</returns>
        public bool ConflictsWith(Appointment other)
        {
            if (Date != other.Date)
                return false;

            if (End <= other.Start)
                return false;

            if (Start >= other.End)
                return false;

            return true;
        }

        /// <summary>
        /// Returns a string representation of this appointment.
        /// </summary>
        /// <returns>A string that represents this appointment instance.</returns>
        public override string ToString()
        {
            return $"{Date:MMMM d, yyyy}, {Start:h:mm tt}-{End:h:mm tt}";
        }
    }
}