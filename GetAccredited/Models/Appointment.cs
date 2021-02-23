using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public Organization Organization { get; set; }
        public string StudentId { get; set; }
        public DateTime Date { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public bool IsBooked { get => StudentId != null; }
        public bool IsPast { get => Date.AddHours(Start.Hour).AddMinutes(Start.Minute) < DateTime.Now; }
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

        public override string ToString()
        {
            return $"{Date:MMMM d, yyyy}, {Start:h:mm tt}-{End:h:mm tt}";
        }
    }
}