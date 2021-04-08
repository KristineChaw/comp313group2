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

        /// <summary>
        /// Returns all Appointments.
        /// </summary>
        public IQueryable<Appointment> Appointments => context.Appointments.Include("Organization");

        /// <summary>
        /// Returns all Bookings.
        /// </summary>
        public IQueryable<Booking> Bookings => context.Bookings.Include("Appointment").Include("Accreditation")
            .Include("Appointment.Organization");

        /// <summary>
        /// Returns all Cards (debit/credit card information).
        /// </summary>
        public IQueryable<Card> Cards => context.Cards;

        /// <summary>
        /// Returns all Requests (appointment rescheduling or cancellation request).
        /// </summary>
        public IQueryable<Request> Requests => context.Requests.Include("Booking").Include("NewAppointment")
            .Include("Booking.Appointment").Include("Booking.Appointment.Organization");

        /// <summary>
        /// Deletes all appointments of the specified organization.
        /// </summary>
        /// <param name="organization">The organization whose appointments are being deleted</param>
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

        /// <summary>
        /// Deletes all bookings by a student account.
        /// </summary>
        /// <param name="studentId">The ID of the ApplicationUser whose bookings are to be deleted</param>
        public void DeleteBookingsByStudent(string studentId)
        {
            var bookings = context.Bookings.Where(b => b.StudentId == studentId);
            if (bookings.Any())
            {
                // delete requests associated with these bookings
                foreach (var booking in bookings)
                {
                    var requests = context.Requests.Where(r => r.Booking == booking);
                    if (requests.Any())
                        context.Requests.RemoveRange(requests);
                }

                // delete the bookings and save changes
                context.Bookings.RemoveRange(bookings);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes all cards (debit/credit card information) by a student account.
        /// </summary>
        /// <param name="studentId">The ID of the ApplicationUser whose cards are to be deleted</param>
        public void DeleteCardsByStudent(string studentId)
        {
            var cards = context.Cards.Where(c => c.CustomerId == studentId);
            context.Cards.RemoveRange(cards);
            context.SaveChanges();
        }

        /// <summary>
        /// Deletes a request (appointment rescheduling or cancellation request).
        /// </summary>
        /// <param name="request">The request to be deleted</param>
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

        /// <summary>
        /// Saves an appointment.
        /// </summary>
        /// <param name="appointment">The appointment being created or updated</param>
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

        /// <summary>
        /// Saves a list of appointments into the system.
        /// </summary>
        /// <param name="slots">The appointment slots to be saved</param>
        public void SaveAppointments(List<Appointment> slots)
        {
            context.Appointments.AddRange(slots);
            context.SaveChanges();
        }

        /// <summary>
        /// Saves a booking.
        /// </summary>
        /// <param name="booking">The booking being created or updated</param>
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

        /// <summary>
        /// Saves a card (debit/credit card information).
        /// </summary>
        /// <param name="card">The card being created or updated</param>
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

        /// <summary>
        /// Saves a request (appointment rescheduling or cancellation request).
        /// </summary>
        /// <param name="request">The request being created or updated</param>
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