using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    public static class Utility
    {
        public const string ROLE_ADMIN = "administrator";
        public const string ROLE_STUDENT = "student";
        public const string ROLE_REP = "representative";

        public const string REQUIREMENTS_DIR = "/data/requirements/";

        private static RoleManager<IdentityRole> roleManager;
        private static UserManager<ApplicationUser> userManager;

        private const string ADMIN_NAME = "administrator";
        private const string DEFAULT_PASSWORD = "Secret123$";

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

        public static string GenerateId()
        {
            string id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            while (id.Contains("/") || id.Contains("+"))
                id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            return id.Substring(0, 22).ToUpper();
        }

        public static void SendEmail(string recipient, Organization organization, string host)
        {
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);

            smtpClient.Credentials = new System.Net.NetworkCredential("noreply.getaccredited@gmail.com", "GetAccreditedcomp231");
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;

            MailMessage mail = new MailMessage();
            mail.Subject = "(noreply) GetAccredited - Invitation to Register as Representative";
            mail.IsBodyHtml = true;

            mail.Body = $"Dear {recipient},<br/><br/>";

            mail.Body += $"You have been invited to register as a representative for your organization, {organization.Name}, at GetAccredited.";

            mail.Body += $"<br/></br>As a representative, you will be reponsible for managing your organization. This will include " +
                $"building schedules for student appointments, adding accreditations and corresponding requirements, " +
                $"and some more.";

            string link = $"{host}/Account/Register?role=representative";
            mail.Body += $"<br/><br/>Use code <b>{organization.OrganizationId}</b> to create your account. You can go to this link to start the registration process: <u><a href=\"{link}\">{link}</a></u>";
            mail.Body += $"<br/><br/>GetAccredited, <i>Site Administrator</i>";

            // Setting From and To
            mail.From = new MailAddress("noreply.getaccredited@gmail.com", "getAccredited-noreply");
            mail.To.Add(new MailAddress(recipient));

            smtpClient.Send(mail);
        }

        // This method deletes an eligibility requirements file uploaded to an accreditation.
        public static bool DeleteRequirementsFile(string url, IWebHostEnvironment env)
        {
            string file = env.WebRootPath + REQUIREMENTS_DIR + url;
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
                return true;
            }
            return false;
        }
    }
}