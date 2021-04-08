using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GetAccredited.Models.Repositories
{
    public class EFAccreditationRepository : IAccreditationRepository
    {
        private ApplicationDbContext context;
        private IWebHostEnvironment env;

        public EFAccreditationRepository(ApplicationDbContext ctx, IWebHostEnvironment _env)
        {
            context = ctx;
            env = _env;
        }

        /// <summary>
        /// Returns all Accreditations.
        /// </summary>
        public IQueryable<Accreditation> Accreditations => context.Accreditations.Include("Organization");

        /// <summary>
        /// Deletes an accreditation with the specified ID.
        /// </summary>
        /// <param name="accreditationId">The ID of the accreditation to be deleted</param>
        /// <returns>The deleted accreditation</returns>
        public Accreditation DeleteAccreditation(int accreditationId)
        {
            Accreditation accreditationEntry = context.Accreditations
                .FirstOrDefault(a => a.AccreditationId == accreditationId);

            if (accreditationEntry != null)
            {
                // delete eligibility requirements file if there's any
                if (accreditationEntry.EligibilityFileURL != null)
                    Utility.DeleteFile(env.WebRootPath + Utility.REQUIREMENTS_DIR + accreditationEntry.EligibilityFileURL);

                context.Accreditations.Remove(accreditationEntry);
                context.SaveChanges();
            }

            return accreditationEntry;
        }

        /// <summary>
        /// Deletes all accreditations being offered by the specified organization.
        /// </summary>
        /// <param name="organization">The organization that offers the accreditations being deleted</param>
        public void DeleteAccreditationsByOrganization(Organization organization)
        {
            // retrieve all accreditations by organization
            var accreditations = context.Accreditations.Where(a => a.Organization == organization);

            if (!accreditations.Any())
                return;

            // update bookings affected
            var bookings = context.Bookings.Where(b => b.Accreditation.Organization == organization);
            foreach (var booking in bookings)
                booking.Accreditation = null;

            //delete eligibility requirements file if there's any
            foreach (var acc in accreditations)
            {
                if (acc.EligibilityFileURL != null)
                    Utility.DeleteFile(env.WebRootPath + Utility.REQUIREMENTS_DIR + acc.EligibilityFileURL);
            }

            // delete selected accreditations
            context.Accreditations.RemoveRange(accreditations);
            context.SaveChanges();
        }

        /// <summary>
        /// Saves an accreditation.
        /// </summary>
        /// <param name="accreditation">The accreditation being created or updated.</param>
        public void SaveAccreditation(Accreditation accreditation)
        {
            Accreditation accreditationEntry = context.Accreditations.FirstOrDefault(a => a.AccreditationId == accreditation.AccreditationId);

            if (accreditationEntry == null)
            {
                context.Accreditations.Add(accreditation);
            }
            else
            {
                accreditationEntry.Name = accreditation.Name;
                accreditationEntry.Type = accreditation.Type;
                accreditationEntry.Eligibility = accreditation.Eligibility;
                accreditationEntry.EligibilityFileURL = accreditation.EligibilityFileURL;
                accreditationEntry.CreatorId = accreditation.CreatorId;
            }

            context.SaveChanges();
        }
    }
}