using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.Repositories
{
    public interface IAppointmentRepository
    {
        IQueryable<Appointment> Appointments { get; }
        IQueryable<Booking> Bookings { get; }
        IQueryable<Card> Cards { get; }
        IQueryable<Request> Requests { get; }

        void DeleteAppointmentsByOrganization(Organization organization);
        void DeleteBookingsByStudent(string studentId);
        void DeleteCardsByStudent(string studentId);
        void DeleteRequest(Request request);
        void SaveAppointment(Appointment appointment);
        void SaveAppointments(List<Appointment> slots);
        void SaveBooking(Booking booking);
        void SaveCard(Card card);
        void SaveRequest(Request request);
    }
}