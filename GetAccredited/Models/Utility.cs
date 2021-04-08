using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    /// <summary>
    /// Provides helper methods used throughout the application.
    /// </summary>
    public static class Utility
    {
        public const string ROLE_ADMIN = "administrator";
        public const string ROLE_STUDENT = "student";
        public const string ROLE_REP = "representative";
        public const string REQUIREMENTS_DIR = "/data/requirements/";
        public const string UPLOADS_DIR = "/data/uploads/";
        public const string LOGOS_DIR = "/images/logos/";

        private static RoleManager<IdentityRole> roleManager;
        private static UserManager<ApplicationUser> userManager;

        private const string ADMIN_NAME = "administrator";
        private const string DEFAULT_PASSWORD = "Secret123$";
        private const string GETACCREDITED_EMAIL = "noreply.getaccredited@gmail.com";
        private const string SENDGRID_APIKEY = "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";

        /// <summary>
        /// This method deletes an eligibility requirements file uploaded to an accreditation.
        /// </summary>
        /// <param name="file">The path of the file being deleted</param>
        /// <returns>true if the file was deleted; otherwise, false</returns>
        public static bool DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ensures that the admin account is created.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static async Task EnsureAdminCreatedAsync(IApplicationBuilder app)
        {
            if (userManager == null)
                userManager = app.ApplicationServices.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser admin = await userManager.FindByNameAsync(ADMIN_NAME);
            if (admin == null)
            {
                admin = new ApplicationUser() { UserName = ADMIN_NAME };

                // create Admin user
                if (!(await userManager.CreateAsync(admin, DEFAULT_PASSWORD)).Succeeded)
                    throw new Exception("Failed to create Admin user.");

                // assign Administrator role to the Admin user
                if (!(await userManager.AddToRoleAsync(admin, ROLE_ADMIN)).Succeeded)
                    throw new Exception("Failed to assign Administrator role to the Admin user.");
            }
        }

        /// <summary>
        /// Ensures that the default organizations are added.
        /// </summary>
        /// <param name="app"></param>
        public static void EnsureOrganizationsAdded(IApplicationBuilder app)
        {
            ApplicationDbContext context = app.ApplicationServices.GetRequiredService<ApplicationDbContext>();

            context.Database.Migrate();

            if (!context.Organizations.Any())
            {
                Organization abc = new Organization
                {
                    OrganizationId = GenerateId(),
                    Name = "A Brave Company",
                    Acronym = "ABC",
                    Description = "This is the first organization added.",
                    WebsiteUrl = "https://www.centennialcollege.ca"
                };

                Organization def = new Organization
                {
                    OrganizationId = GenerateId(),
                    Name = "Do Everything Free",
                    Acronym = "DEF",
                    Description = "This is the second organization added.",
                    WebsiteUrl = "https://www.centennialcollege.ca"
                };

                context.Organizations.Add(abc);
                context.Organizations.Add(def);
                context.SaveChanges();

                // create representatives
                CreateRepresentatives(abc, def, out ApplicationUser abcRep, out ApplicationUser defRep);

                // create accreditations
                CreateAccreditations(abc, def, abcRep, defRep, out Accreditation abcAcc, out Accreditation defAcc, context);

                // create appointments
                CreateAppointments(abc, def, out Appointment abcApp, out Appointment defApp, context);
            }
        }

        /// <summary>
        /// Ensures that the three roles (Admin, Representative, and Student) are created.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static async Task EnsureRolesCreatedAsync(IApplicationBuilder app)
        {
            if (roleManager == null)
                roleManager = app.ApplicationServices.GetRequiredService<RoleManager<IdentityRole>>();

            // create the Administrator role
            IdentityRole role = await roleManager.FindByNameAsync(ROLE_ADMIN);
            if (role == null)
            {
                role = new IdentityRole(ROLE_ADMIN);
                await roleManager.CreateAsync(role);
            }

            // create the Student role
            role = await roleManager.FindByNameAsync(ROLE_STUDENT);
            if (role == null)
            {
                role = new IdentityRole(ROLE_STUDENT);
                await roleManager.CreateAsync(role);
            }

            // create the Representative role
            role = await roleManager.FindByNameAsync(ROLE_REP);
            if (role == null)
            {
                role = new IdentityRole(ROLE_REP);
                await roleManager.CreateAsync(role);
            }
        }

        /// <summary>
        /// Returns a unique 22-character string ID that does not contain / or +.
        /// </summary>
        /// <returns>Unique string ID</returns>
        public static string GenerateId()
        {
            string id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            while (id.Contains("/") || id.Contains("+"))
                id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            return id.Substring(0, 22).ToUpper();
        }

        /// <summary>
        /// Sends an invitation email to a recipient to be a representative for an organization.
        /// </summary>
        /// <param name="recipient">The email of the recipient</param>
        /// <param name="organization">The organization the recipient will represent</param>
        /// <param name="inviteLink">A link to create a representative account</param>
        /// <returns>true if the email has been successfully sent; otherwise, false</returns>
        public static async Task<bool> SendInviteEmail(string recipient, Organization organization, string inviteLink)
        {
            var client = new SendGridClient(SENDGRID_APIKEY);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(GETACCREDITED_EMAIL, "GetAccredited, Site Administrator"),
                Subject = "(noreply) GetAccredited - Invitation to Register as Representative",
                HtmlContent = $"Dear {recipient},<br/><br/>" +
                $"You have been invited to register as a representative for your organization, {organization.Name}, at GetAccredited." +
                $"<br/></br>As a representative, you will be reponsible for managing your organization. This will include " +
                $"building schedules for student appointments, adding accreditations and corresponding requirements, " +
                $"and some more." +
                $"<br/><br/>Use code <b>{organization.OrganizationId}</b> to create your account. You can go to this link to start the registration process: <u><a href=\"{inviteLink}\">{inviteLink}</a></u>" +
                $"<br/><br/>GetAccredited, <i>Site Administrator</i>"
            };
            msg.AddTo(new EmailAddress(recipient));
            var response = await client.SendEmailAsync(msg);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Uploads a file into the system and gives it a unique name.
        /// </summary>
        /// <param name="file">The file being uploaded</param>
        /// <param name="path">The path where the file will be stored</param>
        /// <returns>The unique name of the file uploaded</returns>
        public static async Task<string> UploadFile(IFormFile file, string path) // path is the destination
        {
            // get file extension
            var ext = Path.GetExtension(file.FileName);

            // generate a unique string for the file name
            string fileName = Guid.NewGuid().ToString() + ext;

            // upload file to path
            var filePath = Path.Combine(path, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        /// <summary>
        /// Creates the first 2 representative accounts of the system.
        /// </summary>
        /// <param name="org1">The organization being represented by rep1</param>
        /// <param name="org2">The organization being represented by rep2</param>
        /// <param name="rep1">The first representative account</param>
        /// <param name="rep2">The second representative account</param>
        private static void CreateRepresentatives(Organization org1, Organization org2,
            out ApplicationUser rep1, out ApplicationUser rep2)
        {
            rep1 = new ApplicationUser
            {
                UserName = "jsmith",
                FirstName = "John",
                LastName = "Smith",
                Email = "jsmith@abc.org",
                OrganizationId = org1.OrganizationId
            };

            rep2 = new ApplicationUser
            {
                UserName = "jdoe",
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jdoe@def.org",
                OrganizationId = org2.OrganizationId
            };

            userManager.CreateAsync(rep1, DEFAULT_PASSWORD).Wait();
            userManager.CreateAsync(rep2, DEFAULT_PASSWORD).Wait();
            userManager.AddToRoleAsync(rep1, ROLE_REP).Wait();
            userManager.AddToRoleAsync(rep2, ROLE_REP).Wait();
        }

        /// <summary>
        /// Creates the first 2 accreditations of the system.
        /// </summary>
        /// <param name="org1">The organization offering acc1</param>
        /// <param name="org2">The organization offering acc2</param>
        /// <param name="rep1">The representative that added the first accreditation</param>
        /// <param name="rep2">The representative that added the second accreditation</param>
        /// <param name="acc1">The first accreditation</param>
        /// <param name="acc2">The second accreditation</param>
        /// <param name="context"></param>
        private static void CreateAccreditations(Organization org1, Organization org2,
            ApplicationUser rep1, ApplicationUser rep2, out Accreditation acc1, out Accreditation acc2,
            ApplicationDbContext context)
        {
            acc1 = new Accreditation
            {
                Organization = org1,
                Name = "ABC Nursing Accreditation",
                DateCreated = DateTime.Now,
                CreatorId = rep1.Id,
                Type = Accreditation.GetTypes().ElementAt(7),
                Eligibility = "N/A"
            };

            acc2 = new Accreditation
            {
                Organization = org2,
                Name = "DEF Business Accreditation",
                DateCreated = DateTime.Now,
                CreatorId = rep2.Id,
                Type = Accreditation.GetTypes().ElementAt(3),
                Eligibility = "N/A"
            };

            context.Accreditations.AddRange(acc1, acc2);
            context.SaveChanges();
        }

        /// <summary>
        /// Creates the first 2 appointments of the system.
        /// </summary>
        /// <param name="org1">The organization associated with app1</param>
        /// <param name="org2">The organization associated with app2</param>
        /// <param name="app1">The first appointment</param>
        /// <param name="app2">The second appointment</param>
        /// <param name="context"></param>
        private static void CreateAppointments(Organization org1, Organization org2,
            out Appointment app1, out Appointment app2, ApplicationDbContext context)
        {
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            app1 = new Appointment
            {
                Organization = org1,
                Date = now,
                Start = DateTime.Parse("18:10"),
                End = DateTime.Parse("18:30")
            };

            app2 = new Appointment
            {
                Organization = org2,
                Date = now,
                Start = DateTime.Parse("18:10"),
                End = DateTime.Parse("18:30")
            };

            context.Appointments.AddRange(app1, app2);
            context.SaveChanges();
        }
    }
}