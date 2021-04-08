using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.Repositories
{
    public class EFOrganizationRepository : IOrganizationRepository
    {
        private ApplicationDbContext context;
        private IWebHostEnvironment env;

        public EFOrganizationRepository(ApplicationDbContext ctx, IWebHostEnvironment _env)
        {
            context = ctx;
            env = _env;
        }

        /// <summary>
        /// Returns all Organizations.
        /// </summary>
        public IQueryable<Organization> Organizations => context.Organizations;

        /// <summary>
        /// Deletes an organization specified by the ID provided.
        /// </summary>
        /// <param name="organizationId">The ID of the organization to be deleted</param>
        /// <returns>The deleted organization</returns>
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
                // delete logo image if exists
                if (organization.Logo != null)
                {
                    Utility.DeleteFile(env.WebRootPath + Utility.LOGOS_DIR + organization.Logo);
                    organization.Logo = null;
                }

                context.Organizations.Remove(organization);
                context.SaveChanges();
            }

            return organization;
        }

        /// <summary>
        /// Determines whether an organization with the specified ID exists or not.
        /// </summary>
        /// <param name="id">The ID of the organization being checked.</param>
        /// <returns>true if the organization exists; otherwise, false</returns>
        public bool OrganizationExists(string id)
        {
            return Organizations.Any(o => o.OrganizationId == id);
        }

        /// <summary>
        /// Saves an organization.
        /// </summary>
        /// <param name="organization">The organization being created or updated.</param>
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
                organizationEntry.Logo = organization.Logo;
            }

            context.SaveChanges();
        }
    }
}