using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.Repositories
{
    public interface IOrganizationRepository
    {
        IQueryable<Organization> Organizations { get; }

        void SaveOrganization(Organization organization);

        Organization DeleteOrganization(string organizationId);

        bool OrganizationExists(string id);
    }
}