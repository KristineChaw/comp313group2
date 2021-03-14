using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.Repositories
{
    public class EFOrganizationRepository : IOrganizationRepository
    {
        private ApplicationDbContext context;

        public EFOrganizationRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IQueryable<Organization> Organizations => context.Organizations;

        public Organization DeleteOrganization(string organizationId)
        {
            Organization organization = context.Organizations
                .FirstOrDefault(o => o.OrganizationId == organizationId);

            if (organization == null)
            {
                return null;
            }
            else
            {
                context.Organizations.Remove(organization);
                context.SaveChanges();
            }

            return organization;
        }

        public bool OrganizationExists(string id)
        {
            return Organizations.Any(o => o.OrganizationId == id);
        }

        public void SaveOrganization(Organization organization)
        {
            Organization organizationEntry = context.Organizations
                .FirstOrDefault(o => o.OrganizationId == organization.OrganizationId);

            if (organizationEntry == null)
            {
                context.Organizations.Add(organization);
            }
            else
            {
                organizationEntry.Name = organization.Name;
                organizationEntry.Acronym = organization.Acronym;
                organizationEntry.WebsiteUrl = organization.WebsiteUrl;
                organizationEntry.Description = organization.Description;
            }

            context.SaveChanges();
        }
    }
}