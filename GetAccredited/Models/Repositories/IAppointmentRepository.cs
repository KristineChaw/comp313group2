using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.Repositories
{
    public interface IAppointmentRepository
    {
        IQueryable<Appointment> Appointments { get; }
        void DeleteAppointmentsByOrganization(Organization organization);
        void SaveAppointment(Appointment appointment);
        IQueryable<Booking> Bookings { get; }
        void SaveBooking(Booking booking);
        IQueryable<Card> Cards { get; }
        void SaveCard(Card card);
        IQueryable<Request> Requests { get; }
        void SaveRequest(Request request);
        void DeleteRequest(Request request);
    }
}