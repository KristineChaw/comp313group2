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
    public class AccountController : Controller
    {
        private SignInManager<ApplicationUser> signInManager;
        private UserManager<ApplicationUser> userManager;
        private IOrganizationRepository organizationRepository;

        public AccountController(SignInManager<ApplicationUser> signInMgr,
            UserManager<ApplicationUser> userMgr,
            IOrganizationRepository organizationRepo)
        {
            signInManager = signInMgr;
            userManager = userMgr;
            organizationRepository = organizationRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                TempData["message"] = "You're already logged in.";
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    if ((await signInManager.PasswordSignInAsync(user, model.Password, false, false)).Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    ModelState.AddModelError("", "Incorrect password");
                }
                else
                {
                    ModelState.AddModelError("", "User name does not exist");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string role = Utility.ROLE_STUDENT)
        {
            if (User.Identity.IsAuthenticated)
            {
                TempData["message"] = "You are currently logged in.";
                return RedirectToAction("Index", "Home");
            }

            return View("Register", new RegisterViewModel
            {
                User = new ApplicationUser(),
                Role = role
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // check if email is unique
                if (userManager.IsEmailTaken(model.User.Email))
                {
                    ModelState.AddModelError("", $"Email '{model.User.Email}' is already taken");
                    return View(model);
                }

                // validate representative registration
                if (model.Role == Utility.ROLE_REP)
                {
                    // check if Organization ID exists
                    if (!organizationRepository.OrganizationExists(model.User.OrganizationId))
                    {
                        ModelState.AddModelError("", "Organization does not exist.");
                        return View(model);
                    }
                }

                // attempt to create account and assign appropriate role
                var result = await userManager.CreateAsync(model.User, model.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(model.User, model.Role);
                    await signInManager.SignInAsync(model.User, false);
                    TempData["message"] = "Account successfully created.";
                    return RedirectToAction("Index", "Home");
                }

                // at this point, account creation has failed; retrieve all error messages
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        public async Task<ViewResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpGet]
        public async Task<ViewResult> Edit()
        {
            var user = await userManager.GetUserAsync(User);
            return View("EditProfile", user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ApplicationUser model)
        {
            var user = await userManager.GetUserAsync(User);

            // check if email is valid
            if (user.Email != model.Email && userManager.IsEmailTaken(model.Email))
            {
                ModelState.AddModelError("", $"Email '{model.Email}' is already taken");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;

                IdentityResult result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["message"] = "Changes successfully saved.";
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }
    }
}