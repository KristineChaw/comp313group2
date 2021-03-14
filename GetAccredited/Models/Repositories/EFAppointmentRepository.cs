using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GetAccredited.Models.Repositories
{
    public class EFAppointmentRepository : IAppointmentRepository
    {
        private ApplicationDbContext context;

        public EFAppointmentRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IQueryable<Appointment> Appointments => context.Appointments.Include("Organization");

        public IQueryable<Booking> Bookings => context.Bookings.Include("Appointment").Include("Accreditation")
            .Include("Appointment.Organization");

        public IQueryable<Card> Cards => context.Cards;

        public IQueryable<Request> Requests => context.Requests.Include("Booking").Include("NewAppointment")
            .Include("Booking.Appointment").Include("Booking.Appointment.Organization");

        public void DeleteAppointmentsByOrganization(Organization organization)
        {
            // retrieve all appointments by organization
            var appointments = context.Appointments.Where(a => a.Organization == organization);

            if (!appointments.Any())
                return;

            // deleted requests associated with this organization
            var requests = context.Requests.Where(r => r.Booking.Appointment.Organization == organization);
            context.Requests.RemoveRange(requests);

            // update bookings affected
            var bookings = context.Bookings.Where(b => b.Appointment.Organization == organization);
            foreach (var booking in bookings)
                booking.Appointment = null;

            // delete selected appointments
            context.Appointments.RemoveRange(appointments);
            context.SaveChanges();
        }

        public void DeleteRequest(Request request)
        {
            if (request != null)
            {
                context.Requests.Remove(request);
                context.SaveChanges();
            }
            else
                throw new ArgumentNullException();
        }

        public void SaveAppointment(Appointment appointment)
        {
            // attempt to retrieve appointment
            Appointment appointmentEntry = context.Appointments
                .FirstOrDefault(a => a.AppointmentId == appointment.AppointmentId);

            // if null, add
            if (appointmentEntry == null)
            {
                context.Appointments.Add(appointment);
            }
            else
            {
                // update
                appointmentEntry.Date = appointment.Date;
                appointmentEntry.Start = appointment.Start;
                appointmentEntry.End = appointment.End;
                appointmentEntry.StudentId = appointment.StudentId;
            }

            context.SaveChanges();
        }

        public void SaveBooking(Booking booking)
        {
            Booking bookingEntry = context.Bookings
                .FirstOrDefault(b => b.BookingId == booking.BookingId);

            if (bookingEntry == null)
            {
                context.Bookings.Add(booking);
            }
            else
            {
                bookingEntry.Accreditation = booking.Accreditation;
                bookingEntry.Appointment = booking.Appointment;
                bookingEntry.IsCancelled = booking.IsCancelled;
            }

            context.SaveChanges();
        }

        public void SaveCard(Card card)
        {
            Card cardEntry = context.Cards
                .FirstOrDefault(c => c.CustomerId == card.CustomerId);

            if (cardEntry == null)
                context.Cards.Add(card);
            else
            {
                cardEntry.IsCredit = card.IsCredit;
                cardEntry.Holder = card.Holder;
                cardEntry.Number = card.Number;
                cardEntry.Expiry = card.Expiry;
                cardEntry.Code = card.Code;
            }

            context.SaveChanges();
        }

        public void SaveRequest(Request request)
        {
            if (request.RequestId == 0)
            {
                context.Requests.Add(request);
                context.SaveChanges();
            }
        }
    }
}