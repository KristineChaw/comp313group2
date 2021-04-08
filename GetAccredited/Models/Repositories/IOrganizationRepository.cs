using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.Repositories
{
    public interface IOrganizationRepository
    {
        IQueryable<Organization> Organizations { get; }

        Organization DeleteOrganization(string organizationId);
        bool OrganizationExists(string id);
        void SaveOrganization(Organization organization);
    }
}