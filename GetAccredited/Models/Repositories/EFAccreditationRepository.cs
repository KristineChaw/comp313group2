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
            }

            context.SaveChanges();
        }
    }
}