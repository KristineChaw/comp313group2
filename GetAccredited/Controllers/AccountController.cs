using GetAccredited.Models;
using GetAccredited.Models.Repositories;
using GetAccredited.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private IAccountRepository accountRepository;
        private IOrganizationRepository organizationRepository;

        private IWebHostEnvironment env;

        public AccountController(SignInManager<ApplicationUser> signInMgr,
            UserManager<ApplicationUser> userMgr,
            IAccountRepository accountRepo,
            IOrganizationRepository organizationRepo,
            IWebHostEnvironment _env)
        {
            signInManager = signInMgr;
            userManager = userMgr;
            accountRepository = accountRepo;
            organizationRepository = organizationRepo;
            env = _env;
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

        [HttpGet]
        public ViewResult UploadStudentFile()
        {
            return View("UploadFile");
        }

        [HttpPost]
        public async Task<IActionResult> UploadStudentFile(UploadStudentFileViewModel model)
        {
            if (ModelState.IsValid)
            {
                // retrieve current user
                var user = await userManager.GetUserAsync(User);

                // update fields of the upload file
                model.Upload.StudentId = user.Id;
                model.Upload.FileURL = await Utility.UploadFile(model.UploadFile, env.WebRootPath + Utility.UPLOADS_DIR);

                // save upload file to db
                accountRepository.SaveUpload(model.Upload);

                // redirect user to documents list
                TempData["message"] = "Document successfully uploaded.";
                return await Uploads(user.Id);
            }
            else
            {
                TempData["message"] = "Something went wrong. Please try again.";
                return UploadStudentFile();
            }
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_STUDENT)]
        public async Task<IActionResult> DeleteStudentFile(int uploadId)
        {
            // retrieve the document
            var document = accountRepository.Uploads.FirstOrDefault(u => u.UploadId == uploadId);

            // verify authorization and delete the file from the system
            if (document != null)
            {
                var currentStudent = await userManager.GetUserAsync(User);
                if (currentStudent != await userManager.FindByIdAsync(document.StudentId))
                {
                    TempData["message"] = "No document deleted. You are not authorized to perform this operation.";
                    return RedirectToAction("Index", "Home");
                }

                accountRepository.DeleteUpload(uploadId); // delete from the DB
                TempData["message"] = Utility.DeleteFile(env.WebRootPath + Utility.UPLOADS_DIR + document.FileURL) ? // delete the actual file
                    $"{document.Name} was successfully deleted." : $"Failed to delete document due to an unknown error.";
            }
            else
            {
                TempData["message"] = "Failed to delete the document because it does not exist.";
            }

            return await Uploads(userManager.GetUserId(User));
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_REP + "," + Utility.ROLE_STUDENT)]
        public async Task<IActionResult> ViewStudentFile(int uploadId)
        {
            // retrieve document
            var document = accountRepository.Uploads.FirstOrDefault(u => u.UploadId == uploadId);

            if (document == null)
            {
                TempData["message"] = "This document does not exist.";
                return RedirectToAction("Index", "Home");
            }

            // retrieve uploader
            var uploader = await userManager.FindByIdAsync(document.StudentId);

            // unless the current user is the uploader, do not allow user from seeing this document
            if (User.IsStudent())
            {
                var currentStudent = await userManager.GetUserAsync(User);
                if (currentStudent != uploader)
                {
                    TempData["message"] = "You are not authorized to view this page.";
                    return RedirectToAction("Index", "Home");
                }
            }

            return View("ViewUpload", new ViewUploadViewModel
            {
                Document = document,
                Student = uploader
            });
        }

        [HttpGet]
        [Authorize(Roles = Utility.ROLE_REP + "," + Utility.ROLE_STUDENT)]
        public async Task<IActionResult> Uploads(string user)
        {
            var student = await userManager.FindByIdAsync(user);
            if (student == null)
            {
                TempData["message"] = "This user does not exist.";
                return RedirectToAction("Index", "Home");
            }

            // do not allow a student from seeing other students' document
            if (User.IsStudent())
            {
                var currentStudent = await userManager.GetUserAsync(User);
                if (currentStudent != student)
                {
                    TempData["message"] = "You are not authorized to view this page.";
                    return RedirectToAction("Index", "Home");
                }
            }

            return View("StudentUploads", new StudentUploadsViewModel
            {
                Uploads = accountRepository.Uploads
                            .Where(u => u.StudentId == user)
                            .OrderBy(u => u.Name),
                Student = student
            });
        }
    }
}