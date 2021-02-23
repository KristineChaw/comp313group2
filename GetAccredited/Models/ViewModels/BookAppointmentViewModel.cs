using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.ViewModels
{
    public class BookAppointmentViewModel
    {
        public Organization Organization { get; set; }
        public IEnumerable<Accreditation> Accreditations { get; set; }
        public IEnumerable<Appointment> Appointments { get; set; }
        public int AccreditationId { get; set; }
        public int AppointmentId { get; set; }
    }
}