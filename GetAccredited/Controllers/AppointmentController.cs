using GetAccredited.Models;
using GetAccredited.Models.Repositories;
using GetAccredited.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        private IAccreditationRepository accreditationRepository;
        private IAppointmentRepository appointmentRepository;
        private IOrganizationRepository organizationRepository;

        public AppointmentController(UserManager<ApplicationUser> userMgr,
            IAppointmentRepository repo, IOrganizationRepository organizationRepo, IAccreditationRepository accreditationRepo)
        {
            userManager = userMgr;
            accreditationRepository = accreditationRepo;
            appointmentRepository = repo;
            organizationRepository = organizationRepo;
        }

        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var request = appointmentRepository.Requests.FirstOrDefault(r => r.RequestId == requestId);
            if (request == null)
            {
                TempData["message"] = "Request not found.";
                return await Requests();
            }

            // if reschedule, set Booking.Appointment.StudentId to null and Booking.Appointment to NewAppointment
            if (request.IsCancel)
            {
                appointmentRepository.DeleteRequest(request);
                await CancelAppointment(request.Booking.Appointment.AppointmentId);
            }
            else
            {
                var oldAppointment = request.Booking.Appointment;
                if (request.NewAppointment.IsBooked)
                {
                    TempData["message"] = "The requested time slot is already booked by another student.";
                    appointmentRepository.DeleteRequest(request);
                    return await Requests();
                }
                request.NewAppointment.StudentId = oldAppointment.StudentId;
                request.Booking.Appointment = request.NewAppointment;
                oldAppointment.StudentId = null;

                appointmentRepository.SaveAppointment(oldAppointment);
                appointmentRepository.SaveAppointment(request.NewAppointment);
                appointmentRepository.SaveBooking(request.Booking);
                appointmentRepository.DeleteRequest(request);
            }

            TempData["message"] = "Request approved.";
            return await Requests();
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_STUDENT)]
        public IActionResult BookAppointment(string organizationId, int accreditationId = 0)
        {
            var accreditation = accreditationId > 0 ? accreditationRepository.Accreditations.First(a => a.AccreditationId == accreditationId) :
                null;
            var organization = accreditation == null ? organizationRepository.Organizations.First(o => o.OrganizationId == organizationId) :
                accreditation.Organization;
            var accreditations = accreditationRepository.Accreditations.Where(a => a.Organization == organization);
            var appointments = appointmentRepository.Appointments.ToList()
                .Where(a => a.Organization == organization)
                .Where(a => !a.IsBooked && !a.IsPast)
                .OrderBy(a => a.Date).ThenBy(a => a.Start);
            if (!appointments.Any())
            {
                TempData["message"] = "This organization currently has no time slots available.";
                return RedirectToAction("BrowseAccreditations", "Accreditation");
            }
            return View(new BookAppointmentViewModel
            {
                Organization = organization,
                Accreditations = accreditations,
                Appointments = appointments,
                AccreditationId = accreditationId
            });
        }

        [HttpPost]
        [Authorize(Roles = Utility.ROLE_STUDENT)]
        public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var student = await userManager.GetUserAsync(User);
                var accreditation = accreditationRepository.Accreditations.First(a => a.AccreditationId == model.AccreditationId);
                var appointment = appointmentRepository.Appointments.First(a => a.AppointmentId == model.AppointmentId);
                var card = appointmentRepository.Cards.FirstOrDefault(c => c.CustomerId == student.Id);
                return View("Payment", new PaymentViewModel
                {
                    Accreditation = accreditation,
                    Appointment = appointment,
                    Card = card
                });
            }

            return View(model);
        }

        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var appointment = appointmentRepository.Appointments.FirstOrDefault(a => a.AppointmentId == appointmentId);
            if (appointment == null)
            {
                TempData["message"] = "Failed to cancel appointment because it does not exist.";
                return await List();
            }

            var booking = appointmentRepository.Bookings
                .FirstOrDefault(b => b.Appointment.AppointmentId == appointmentId &&
                b.StudentId == appointment.StudentId);

            if (booking == null)
            {
                TempData["message"] = "This appointment is not booked.";
                return await List();
            }

            // update appointment and booking entries
            appointment.StudentId = null;
            booking.IsCancelled = true;
            appointmentRepository.SaveBooking(booking);
            appointmentRepository.SaveAppointment(appointment);
            TempData["message"] = "The appointment has been successfully cancelled.";
            return await List();
        }

        [HttpPost]
        [Authorize(Roles = Utility.ROLE_STUDENT)]
        public async Task<IActionResult> CompleteBooking(PaymentViewModel model)
        {
            var accreditation = accreditationRepository.Accreditations.First(a => a.AccreditationId == model.Accreditation.AccreditationId);
            var appointment = appointmentRepository.Appointments.First(a => a.AppointmentId == model.Appointment.AppointmentId);

            // check if appointment has already been booked
            if (appointment.IsBooked)
            {
                ModelState.AddModelError("", "Failed to book appointment. The appointment has already been booked.");
                return RedirectToAction("Index", "Home");
            }

            // check if card is expired
            if (model.Card.IsExpired)
                ModelState.AddModelError("", "Payment rejected. The card has expired.");

            if (ModelState.IsValid)
            {
                var student = await userManager.GetUserAsync(User);
                appointment.StudentId = student.Id;

                // save appointment and record booking
                appointmentRepository.SaveAppointment(appointment);
                appointmentRepository.SaveBooking(new Booking
                {
                    StudentId = student.Id,
                    Appointment = appointment,
                    Accreditation = accreditation,
                    DateBooked = DateTime.Now
                });

                // if user wants to save their card info, save to db
                if (model.SaveInfo)
                {
                    model.Card.CustomerId = student.Id;
                    appointmentRepository.SaveCard(model.Card);
                }

                TempData["message"] =
                    $"Payment accepted. Thank you. " +
                    $"You have successfully booked an appointment with {accreditation.Organization.Name} " +
                    $"on {appointment.Date:MMMM d, yyyy}, from {appointment.Start:h:mm tt} to {appointment.End:h:mm tt}.";
                return RedirectToAction("Index", "Home");
            }

            return View("Payment", new PaymentViewModel
            {
                Accreditation = accreditation,
                Appointment = appointment,
                Card = model.Card,
                SaveInfo = model.SaveInfo
            });
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_REP)]
        public ViewResult Create()
        {
            return View("CreateAppointment", new Appointment() { Date = DateTime.Now });
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_REP)]
        public ViewResult Edit(int appointmentId)
        {
            var appointment = appointmentRepository.Appointments.Where(a => a.AppointmentId == appointmentId).First();
            return View("CreateAppointment", appointment);
        }

        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<ViewResult> List()
        {
            var representative = await userManager.GetUserAsync(User);

            return View("AppointmentList", appointmentRepository.Appointments
                .Where(a => a.Organization.OrganizationId == representative.OrganizationId)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Start)
                .ToList().Where(a => !a.IsPast));
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_STUDENT)]
        public async Task<IActionResult> MyAppointments()
        {
            var user = await userManager.GetUserAsync(User);
            var bookings = appointmentRepository.Bookings
                .Where(b => b.StudentId == user.Id && b.Appointment != null)
                .OrderBy(b => b.Appointment.Date)
                .ThenBy(b => b.Appointment.Start);
            var appointments = bookings.ToList()
                .Where(app => !app.Appointment.IsPast);
            return View("UserAppointments", appointments);
        }

        [Authorize(Roles = Utility.ROLE_STUDENT)]
        public async Task<IActionResult> RequestCancellation(int bookingId)
        {
            var booking = appointmentRepository.Bookings.First(b => b.BookingId == bookingId);

            // check if there's already a request associated with this booking
            if (appointmentRepository.Requests.Any(r => r.Booking == booking))
            {
                TempData["message"] = "You already sent a request to reschedule or cancel this appointment.";
                return await MyAppointments();
            }

            appointmentRepository.SaveRequest(new Request
            {
                Booking = booking
            });
            TempData["message"] = "Request to cancel appointment successfully sent.";
            return await MyAppointments();
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_STUDENT)]
        public async Task<IActionResult> RequestReschedule(int bookingId)
        {
            var booking = appointmentRepository.Bookings.FirstOrDefault(b => b.BookingId == bookingId);

            // check if booking exists
            if (booking == null)
            {
                TempData["message"] = "Booking does not exist.";
                return await MyAppointments();
            }

            // check if booking is cancelled
            if (booking.IsCancelled)
            {
                TempData["message"] = "This appointment has been cancelled";
                return await MyAppointments();
            }

            // check if appointment is finished or is not booked
            if (booking.Appointment.IsPast || !booking.Appointment.IsBooked)
            {
                TempData["message"] = "This appointment is in the past or is not booked.";
                return await MyAppointments();
            }

            // check if there's already a request associated with this booking
            if (appointmentRepository.Requests.Any(r => r.Booking == booking))
            {
                TempData["message"] = "You already sent a request to reschedule or cancel this appointment.";
                return await MyAppointments();
            }

            // get all open appointments excluding this one
            ViewBag.Appointments = appointmentRepository.Appointments
                .Where(a => a.Organization == booking.Appointment.Organization)
                .Where(a => a.AppointmentId != booking.Appointment.AppointmentId)
                .ToList().Where(a => !a.IsBooked && !a.IsPast);
            return View("Reschedule", new Request
            {
                Booking = booking
            });
        }

        [HttpPost]
        [Authorize(Roles = Utility.ROLE_STUDENT)]
        public async Task<IActionResult> RequestReschedule(Request model)
        {
            if (ModelState.IsValid)
            {
                model.NewAppointment = appointmentRepository.Appointments
                    .First(a => a.AppointmentId == model.NewAppointment.AppointmentId);

                model.Booking = appointmentRepository.Bookings
                    .First(b => b.BookingId == model.Booking.BookingId);

                appointmentRepository.SaveRequest(model);
                TempData["message"] = "Reschedule request successfully sent.";
                return await MyAppointments();
            }

            return await RequestReschedule(model.Booking.BookingId);
        }

        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            var request = appointmentRepository.Requests.FirstOrDefault(r => r.RequestId == requestId);
            appointmentRepository.DeleteRequest(request);
            TempData["message"] = "Request rejected.";
            return await Requests();
        }

        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<IActionResult> Requests()
        {
            var rep = await userManager.GetUserAsync(User);
            var organization = organizationRepository.Organizations.First(o => o.OrganizationId == rep.OrganizationId);
            var requests = appointmentRepository.Requests.ToList().Where(r => r.Booking.Appointment.Organization == organization);

            // clean requests (delete requests associated to expired appointments;
            // delete rescheduling requests to unavailable time slot)
            requests = requests.Where(r => !r.Booking.Appointment.IsPast && !r.NewAppointment.IsPast);

            if (requests == null)
            {
                TempData["message"] = "There are currently no rescheduling and cancellation requests.";
                return RedirectToAction("Index", "Home");
            }
            return View("Requests", requests);
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_ADMIN + "," + Utility.ROLE_REP)]
        public ViewResult VerifyAppointment()
        {
            return View("VerifyAppointment");
        }

        [HttpPost]
        [Authorize(Roles = Utility.ROLE_ADMIN + "," + Utility.ROLE_REP)]
        public async Task<IActionResult> VerifyAppointment(int trackingNumber)
        {
            // check if appointment exists
            var appointment = appointmentRepository.Appointments
                .Where(a => a.AppointmentId == trackingNumber)
                .FirstOrDefault();

            if (appointment == null)
            {
                TempData["message"] = $"There is no appointment with tracking number {trackingNumber}.";
                return RedirectToAction("VerifyAppointment");
            }

            // check if user is a representative
            // a representative should only be able to verify appointments booked with their own organization
            if (User.IsRepresentative())
            {
                var rep = await userManager.GetUserAsync(User);
                if (appointment.Organization.OrganizationId != rep.OrganizationId) // appointment belongs to another organization
                {
                    TempData["message"] = "You do not have enough rights to view the details of this appointment.";
                    return RedirectToAction("VerifyAppointment");
                }
            }

            // check if it's not booked
            if (!appointment.IsBooked)
            {
                TempData["message"] = "This appointment is not booked.";
                return RedirectToAction("VerifyAppointment");
            }

            // load appointment with booking information
            return View("AppointmentDetails", appointmentRepository.Bookings
                .Where(b => b.Appointment == appointment)
                .First());
        }

        [HttpPost]
        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<IActionResult> Save(Appointment model)
        {
            if (ModelState.IsValid)
            {
                // check if time is valid
                if (model.End <= model.Start)
                {
                    ModelState.AddModelError("", "Please specify a valid time period.");
                    return View("CreateAppointment", model);
                }

                // verify that the date is in the future
                if (model.IsPast)
                {
                    ModelState.AddModelError("", "Please select a date that is in the future.");
                    return View("CreateAppointment", model);
                }

                // check for scheduling conflicts
                var representative = await userManager.GetUserAsync(User);
                var appointments = appointmentRepository.Appointments
                    .Where(a => a.Organization.OrganizationId == representative.OrganizationId
                    && a.AppointmentId != model.AppointmentId);
                foreach (var appointment in appointments)
                {
                    if (appointment.ConflictsWith(model))
                    {
                        ModelState.AddModelError("", "This time period conflicts with at least one other appointment slot. Please specify another.");
                        return View("CreateAppointment", model);
                    }
                }

                // check if appointment is being updated
                if (model.AppointmentId != 0)
                {
                    TempData["message"] = "Changes saved. Appointment has been successfully updated.";
                    appointmentRepository.SaveAppointment(model);
                    return RedirectToAction("List");
                }

                // set organization and save
                model.Organization = organizationRepository.Organizations
                    .First(o => o.OrganizationId == representative.OrganizationId);
                appointmentRepository.SaveAppointment(model);
                TempData["message"] = "Appointment successfully created.";
                return RedirectToAction("List");
            }

            return View("CreateAppointment", model);
        }
    }
}