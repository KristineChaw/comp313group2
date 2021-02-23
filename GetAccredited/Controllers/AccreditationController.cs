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
    public class AccreditationController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        private IOrganizationRepository organizationRepository;
        private IAccreditationRepository accreditationRepository;

        public AccreditationController(UserManager<ApplicationUser> userMgr,
            IOrganizationRepository organizationRepo, IAccreditationRepository accreditationRepo)
        {
            userManager = userMgr;
            organizationRepository = organizationRepo;
            accreditationRepository = accreditationRepo;
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
            return View("CreateAccreditation", new Accreditation());
        }

        [Authorize(Roles = Utility.ROLE_REP)]
        public async Task<ViewResult> Delete(int accreditationId)
        {
            var representative = await userManager.GetUserAsync(User);
            var accreditation = accreditationRepository.DeleteAccreditation(accreditationId);
            if (accreditation != null)
                TempData["message"] = "Accreditation successfully deleted.";
            else
                TempData["message"] = "Failed to delete accreditation because it does not exist.";
            return View("AccreditationList", accreditationRepository.Accreditations
                .Where(a => a.Organization.OrganizationId == representative.OrganizationId));
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_REP)]
        public ViewResult Edit(int accreditationId)
        {
            return View("CreateAccreditation", accreditationRepository.Accreditations
                .FirstOrDefault(a => a.AccreditationId == accreditationId));
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
        public async Task<IActionResult> Save(Accreditation model)
        {
            if (ModelState.IsValid)
            {
                // accreditation is being created
                if (model.AccreditationId == 0)
                {
                    var creator = await userManager.GetUserAsync(User);
                    model.CreatorId = creator.Id;
                    model.Organization = organizationRepository.Organizations
                        .FirstOrDefault(o => o.OrganizationId == creator.OrganizationId);
                    model.DateCreated = DateTime.Now;
                    TempData["message"] = "Accreditation successfully created.";
                }

                // accreditation is being updated
                else
                {
                    TempData["message"] = "Accreditation successfully updated.";
                }

                accreditationRepository.SaveAccreditation(model);
                return RedirectToAction("List");
            }

            return View("CreateAccreditation", model);
        }
    }
}