using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GetAccredited.Models.Repositories
{
    public class EFAccreditationRepository : IAccreditationRepository
    {
        private ApplicationDbContext context;

        public EFAccreditationRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IQueryable<Accreditation> Accreditations => context.Accreditations.Include("Organization");

        public Accreditation DeleteAccreditation(int accreditationId)
        {
            Accreditation accreditationEntry = context.Accreditations
                .FirstOrDefault(a => a.AccreditationId == accreditationId);

            if (accreditationEntry != null)
            {
                context.Accreditations.Remove(accreditationEntry);
                context.SaveChanges();
            }

            return accreditationEntry;
        }

        public void DeleteAccreditationsByOrganization(Organization organization)
        {
            // retrieve all accreditations by organization
            var accreditations = context.Accreditations.Where(a => a.Organization == organization);

            if (!accreditations.Any())
                return;

            // update bookings affected
            foreach (var acc in accreditations)
            {
                var bookings = context.Bookings.Where(b => b.Accreditation == acc);

                foreach (var b in bookings)
                    b.Accreditation = null;
            }

            // delete selected accreditations
            context.Accreditations.RemoveRange(accreditations);
            context.SaveChanges();
        }

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
                accreditationEntry.CreatorId = accreditation.CreatorId;
            }

            context.SaveChanges();
        }
    }
}