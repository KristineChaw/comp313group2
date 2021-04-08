using GetAccredited.Models;
using GetAccredited.Models.Repositories;
using GetAccredited.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GetAccredited.Controllers
{
    [Authorize]
    public class OrganizationController : Controller
    {
        private readonly IOrganizationRepository organizationRepository;
        private IAccreditationRepository accreditationRepository;
        private IAppointmentRepository appointmentRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private IWebHostEnvironment env;

        public OrganizationController(IOrganizationRepository organizationRepo,
            IAccreditationRepository accreditationRepo,
            IAppointmentRepository appointmentRepo,
            UserManager<ApplicationUser> userMgr,
             IWebHostEnvironment _env)
        {
            organizationRepository = organizationRepo;
            accreditationRepository = accreditationRepo;
            appointmentRepository = appointmentRepo;
            userManager = userMgr;
            env = _env;
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_ADMIN)]
        public ViewResult Create()
        {
            return View("CreateOrganization", new Organization());
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_ADMIN)]
        public async Task<IActionResult> Delete(string organizationId)
        {
            // check if organization exists
            var organization = organizationRepository.Organizations.FirstOrDefault(o => o.OrganizationId == organizationId);
            if (organization == null)
            {
                TempData["message"] = "No organization deleted. This organization does not exist.";
                var res = List();
                res.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return res;
            }

            // retrieve all appointments
            var appointments = appointmentRepository.Appointments.Where(a => a.Organization == organization);

            // check if there are scheduled appointments, don't allow deletion if there's any
            if (appointments.ToList().Any(a => !a.IsPast && a.IsBooked))
            {
                TempData["message"] = $"{organization.Name} was not deleted. The organization currently has at least one scheduled appointment at the moment.";
                return List();
            }

            // delete appointments
            appointmentRepository.DeleteAppointmentsByOrganization(organization);

            // delete accreditations
            accreditationRepository.DeleteAccreditationsByOrganization(organization);

            // delete representatives of this organization
            var reps = await userManager.GetRepresentativesByOrganization(organizationId);
            foreach (var r in reps)
            {
                var result = await userManager.DeleteAsync(r);
                if (!result.Succeeded)
                {
                    TempData["message"] = "Cannot delete a representative. Organization was not successfully deleted.";
                    return List();
                }
            }

            // delete the organization itself
            organizationRepository.DeleteOrganization(organization.OrganizationId);

            TempData["message"] = $"{organization.Name} has been successfully deleted.";
            return List();
        }

        public ViewResult Display(string organizationId)
        {
            ViewBag.Accreditations = accreditationRepository.Accreditations
                .Where(acc => acc.Organization.OrganizationId == organizationId);
            return View("OrganizationDetails", organizationRepository.Organizations
                .FirstOrDefault(o => o.OrganizationId == organizationId));
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<IActionResult> Edit(string organizationId)
        {
            var rep = await userManager.GetUserAsync(User);
            if (rep.OrganizationId != organizationId)
            {
                TempData["message"] = "You do not have permission to edit this organization.";
                return RedirectToAction("Index", "Home");
            }
            return View("CreateOrganization", organizationRepository
                .Organizations.First(o => o.OrganizationId == organizationId));
        }

        [HttpPost]
        [Authorize(Roles = Utility.ROLE_ADMIN)]
        public async Task<IActionResult> Invite(RepresentativesViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Organization = organizationRepository.Organizations.
                    First(o => o.OrganizationId == model.Organization.OrganizationId);
                var inviteLink = $"{HttpContext.Request.Host}/Account/Register?role=representative";
                if (await Utility.SendInviteEmail(model.Email, model.Organization, inviteLink))
                {
                    TempData["message"] = $"Email sent to {model.Email}.";
                }
                else
                {
                    TempData["message"] = $"Something went wrong. No invitation email has been sent.";
                }
                return RedirectToAction("Representatives", new
                {
                    organizationId = model.Organization.OrganizationId
                });
            }
            return View("RepresentativeList", model);
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_ADMIN + "," + Utility.ROLE_STUDENT)]
        public ViewResult List()
        {
            return View("OrganizationList", organizationRepository.Organizations.OrderBy(o => o.Name));
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_ADMIN)]
        public async Task<IActionResult> RemoveRepresentative(String email)
        {
            // retrieve rep via email
            var rep = await userManager.FindByEmailAsync(email);

            // if email is invalid
            if (rep == null)
            {
                // set appropriate message
                TempData["message"] = $"No representative removed. There does not exist a representative with the email {email}.";
                return UnprocessableEntity();
            }

            // retrieve organization of the rep
            var org = organizationRepository.Organizations.First(o => o.OrganizationId == rep.OrganizationId);

            // attempt to delete rep
            var result = await userManager.DeleteAsync(rep);
            if (result.Succeeded)
            {
                // update accreditations created by this rep
                var accreditations = accreditationRepository.Accreditations.Where(a => a.CreatorId == rep.Id);
                foreach (var acc in accreditations.ToList())
                {
                    acc.CreatorId = null;
                    accreditationRepository.SaveAccreditation(acc);
                }

                // set appropriate message
                TempData["message"] = $"{rep.FirstName} {rep.LastName} is no longer a representative for {org.Name}.";
            }
            else
            {
                // set appropriate message
                TempData["message"] = $"Failed to remove {rep.FirstName} {rep.LastName} as a representative of {org.Name}.";
            }

            // return to manage representative page
            return View("RepresentativeList", new RepresentativesViewModel
            {
                Organization = org,
                Representatives = await userManager.GetRepresentativesByOrganization(org.OrganizationId)
            });
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_ADMIN)]
        public async Task<ViewResult> Representatives(string organizationId)
        {
            Organization organization = organizationRepository.Organizations
                .First(o => o.OrganizationId == organizationId);

            return View("RepresentativeList", new RepresentativesViewModel
            {
                Organization = organization,
                Representatives = await userManager.GetRepresentativesByOrganization(organizationId)
            });
        }

        [HttpPost]
        [Authorize(Roles = Utility.ROLE_ADMIN + "," + Utility.ROLE_REP)]
        public async Task<IActionResult> Save(Organization model)
        {
            if (ModelState.IsValid)
            {
                // organization is being created
                if (model.OrganizationId == null)
                {
                    model.OrganizationId = Utility.GenerateId();
                    TempData["message"] = $"{model.Name} successfully created.";
                }

                // organization is being updated
                else
                {
                    string orgId = (await userManager.GetUserAsync(User)).OrganizationId;
                    TempData["message"] = $"{model.Name} successfully updated.";
                    organizationRepository.SaveOrganization(model);
                    return RedirectToAction("Display", new { organizationId = orgId });
                }

                organizationRepository.SaveOrganization(model);
                return RedirectToAction("Representatives", new { organizationId = model.OrganizationId });
            }
            return View("CreateOrganization", model);
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<IActionResult> UploadLogo(string organizationId)
        {
            // check if organization exists
            var organization = organizationRepository.Organizations
                .FirstOrDefault(o => o.OrganizationId == organizationId);

            if (organization == null)
            {
                TempData["message"] = "There does not exist an organization with this ID.";
                return RedirectToAction("Index", "Home");
            }

            // check if current user is part of this organization
            var rep = await userManager.GetUserAsync(User);
            if (rep.OrganizationId != organizationId)
            {
                TempData["message"] = "You do not have permission to upload a logo for this organization.";
                return RedirectToAction("Index", "Home");
            }

            return View(new UploadLogoViewModel
            {
                Organization = organization
            });
        }

        [HttpPost]
        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<IActionResult> UploadLogo(UploadLogoViewModel model)
        {
            if (ModelState.IsValid)
            {
                // retrieve organization
                model.Organization = organizationRepository.Organizations
                    .First(o => o.OrganizationId == model.Organization.OrganizationId);

                // delete previous logo if Exists
                if (model.Organization.Logo != null)
                {
                    Utility.DeleteFile(env.WebRootPath + Utility.LOGOS_DIR + model.Organization.Logo);
                    model.Organization.Logo = null;
                }

                // upload file and get set file name of the organization's logo
                model.Organization.Logo = await Utility.UploadFile(model.Logo, env.WebRootPath + Utility.LOGOS_DIR);

                // save changes
                organizationRepository.SaveOrganization(model.Organization);
                TempData["message"] = "Organization logo has been successfully added or changed.";

                // redirect to organization page
                return RedirectToAction("Display", new { organizationId = model.Organization.OrganizationId });
            }

            TempData["message"] = "Something went wrong. Try again.";
            return await UploadLogo(model.Organization.OrganizationId);
        }
    }
}