using GetAccredited.Models;
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
    public class HomeController : Controller
    {
        private UserManager<ApplicationUser> userManager;

        public HomeController(UserManager<ApplicationUser> userMgr)
        {
            userManager = userMgr;
        }

        public async Task<ViewResult> Index() => View(User.IsStudent() ? "Student" :
            User.IsRepresentative() ? "Representative" : "Admin", await userManager.GetUserAsync(User));
    }
}