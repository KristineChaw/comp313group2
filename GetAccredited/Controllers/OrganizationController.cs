using GetAccredited.Models;
using GetAccredited.Models.Repositories;
using GetAccredited.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
        private readonly UserManager<ApplicationUser> userManager;

        public OrganizationController(IOrganizationRepository organizationRepo, IAccreditationRepository accreditationRepo, UserManager<ApplicationUser> userMgr)
        {
            organizationRepository = organizationRepo;
            accreditationRepository = accreditationRepo;
            userManager = userMgr;
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_ADMIN)]
        public ViewResult Create()
        {
            return View("CreateOrganization", new Organization());
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
        public IActionResult Invite(RepresentativesViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Organization = organizationRepository.Organizations.
                    First(o => o.OrganizationId == model.Organization.OrganizationId);
                Utility.SendEmail(model.Email, model.Organization, HttpContext.Request.Host.ToString());
                TempData["message"] = $"Email sent to {model.Email}.";
                return RedirectToAction("Representatives", new
                {
                    organizationId = model.Organization.OrganizationId
                });
            }
            return View("RepresentativeList", model);
        }

        [HttpGet]
        public ViewResult List()
        {
            return View("OrganizationList", organizationRepository.Organizations.OrderBy(o => o.Name));
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
    }
}