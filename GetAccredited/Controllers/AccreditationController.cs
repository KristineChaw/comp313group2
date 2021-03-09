﻿using GetAccredited.Models;
using GetAccredited.Models.Repositories;
using GetAccredited.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Controllers
{
    [Authorize]
    public class AccreditationController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        private IAppointmentRepository appointmentRepository;
        private IOrganizationRepository organizationRepository;
        private IAccreditationRepository accreditationRepository;
        private IWebHostEnvironment env;

        public AccreditationController(UserManager<ApplicationUser> userMgr,
            IOrganizationRepository organizationRepo, IAccreditationRepository accreditationRepo,
            IAppointmentRepository appointmentRepo, IWebHostEnvironment _env)
        {
            userManager = userMgr;
            organizationRepository = organizationRepo;
            accreditationRepository = accreditationRepo;
            appointmentRepository = appointmentRepo;
            env = _env;
        }

        [Authorize(Roles = Utility.ROLE_STUDENT)]
        public ViewResult BrowseAccreditations(string SearchKey, string SearchBy, string AccreditationType)
        {
            var accreditations = accreditationRepository.Accreditations;

            if (SearchKey != null)
            {
                if (SearchBy == "Organization")
                {
                    accreditations = accreditations.Where(a => a.Organization.Name.ToLower().Contains(SearchKey.Trim().ToLower()));
                }
                else
                {
                    accreditations = accreditations.Where(a => a.Name.ToLower().Contains(SearchKey.Trim().ToLower()));
                }
            }

            if (AccreditationType != null)
            {
                accreditations = accreditations.Where(a => a.Type == AccreditationType);
            }

            return View("Browse", new BrowseAccreditationsViewModel
            {
                Accreditations = accreditations.OrderBy(a => a.Organization.Name)
                .ThenBy(a => a.Name),
                SearchKey = SearchKey,
                SearchBy = SearchBy,
                AccreditationType = AccreditationType
            });
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_REP)]
        public ViewResult Create()
        {
            return View("CreateAccreditation", new AccreditationViewModel()
            {
                Accreditation = new Accreditation()
            });
        }

        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<ViewResult> Delete(int accreditationId)
        {
            var representative = await userManager.GetUserAsync(User);
            ViewResult result = View("AccreditationList", accreditationRepository.Accreditations
                    .Where(a => a.Organization.OrganizationId == representative.OrganizationId));

            // check if Accreditation exists
            var accreditation = accreditationRepository.Accreditations.FirstOrDefault(a => a.AccreditationId == accreditationId);
            if (accreditation == null)
            {
                TempData["message"] = "Failed to delete accreditation because it does not exist.";
                return result;
            }

            // check if current user is authorized to delete Accreditation
            if (representative.OrganizationId != accreditation.Organization.OrganizationId)
            {
                TempData["message"] = "You do not have enough rights to delete this accreditation as it does not belong to your organization.";
                return result;
            }

            // check if Accreditation is booked
            // retrieve all appointments
            var bookings = appointmentRepository.Bookings.Where(b => b.Accreditation == accreditation);

            // check if there are scheduled appointments with this accreditation, don't allow deletion if there's any
            if (bookings.ToList().Any(b => !b.Appointment.IsPast && b.Appointment.IsBooked))
            {
                TempData["message"] = $"{accreditation.Name} was not deleted. An appointment is currently booked with this accreditation.";
                return result;
            }

            // delete Accreditation
            var deletedAccreditation = accreditationRepository.DeleteAccreditation(accreditationId);
            if (deletedAccreditation != null)
                TempData["message"] = "Accreditation successfully deleted.";
            else
                TempData["message"] = "Failed to delete accreditation due to some unknown error.";

            // return to Accreditation List
            return result;
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_REP)]
        public ViewResult Edit(int accreditationId)
        {
            return View("CreateAccreditation", new AccreditationViewModel()
            {
                Accreditation = accreditationRepository.Accreditations
                .FirstOrDefault(a => a.AccreditationId == accreditationId)
            });
        }

        [Authorize(Roles = Utility.ROLE_STUDENT)]
        public ViewResult Eligibility(int accreditationId)
        {
            return View("Eligibility", accreditationRepository.Accreditations
                .Where(a => a.AccreditationId == accreditationId)
                .First());
        }

        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<ViewResult> List()
        {
            var representative = await userManager.GetUserAsync(User);

            return View("AccreditationList", accreditationRepository.Accreditations
                .Where(a => a.Organization.OrganizationId == representative.OrganizationId));
        }

        [HttpPost]
        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<IActionResult> Save(AccreditationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // accreditation is being created
                if (model.Accreditation.AccreditationId == 0)
                {
                    var creator = await userManager.GetUserAsync(User);
                    model.Accreditation.CreatorId = creator.Id;
                    model.Accreditation.Organization = organizationRepository.Organizations
                        .FirstOrDefault(o => o.OrganizationId == creator.OrganizationId);
                    model.Accreditation.DateCreated = DateTime.Now;
                    TempData["message"] = "Accreditation successfully created.";
                }

                // accreditation is being updated
                else
                {
                    TempData["message"] = "Accreditation successfully updated.";
                }

                // check if a file is uploaded
                if (model.Eligibility != null)
                {
                    // if there is an existing file, delete it
                    Utility.DeleteRequirementsFile(model.Accreditation.EligibilityFileURL, env);

                    // upload new file
                    model.Accreditation.EligibilityFileURL = await UploadFile(model.Eligibility, env.WebRootPath + Utility.REQUIREMENTS_DIR);
                    model.Accreditation.Eligibility = "N/A";
                }

                accreditationRepository.SaveAccreditation(model.Accreditation);
                return RedirectToAction("List");
            }

            return View("CreateAccreditation", model);
        }

        [Authorize(Roles = Utility.ROLE_REP)]
        public IActionResult DeleteFile(int accreditationId)
        {
            var accreditation = accreditationRepository.Accreditations
                .FirstOrDefault(a => a.AccreditationId == accreditationId);

            if (accreditation == null) // if accreditation does not exist, return to home
            {
                TempData["message"] = "This accreditation does not exist.";
                return RedirectToAction("Index", "Home");
            }

            // delete file if there is an existing one
            if (Utility.DeleteRequirementsFile(accreditation.EligibilityFileURL, env))
            {
                // clear EligibilityFileURL property
                accreditation.EligibilityFileURL = null;

                // save changes
                accreditationRepository.SaveAccreditation(accreditation);

                TempData["message"] = "Requirements document has been successfully deleted.";
            }
            else // there is no file
            {
                TempData["message"] = "There is no requirements document associated with this accreditation.";
            }

            // return to edit view
            return View("CreateAccreditation", new AccreditationViewModel()
            {
                Accreditation = accreditation
            });
        }

        private static async Task<string> UploadFile(IFormFile file, string path)
        {
            // generate a unique string for the file name
            string fileName = Guid.NewGuid().ToString() + ".pdf";

            // upload file to data/requirements
            var filePath = Path.Combine(path, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}