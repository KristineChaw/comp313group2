using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.Repositories
{
    public interface IAccreditationRepository
    {
        IQueryable<Accreditation> Accreditations { get; }
        Accreditation DeleteAccreditation(int accreditationId);
        void DeleteAccreditationsByOrganization(Organization organization);
        void SaveAccreditation(Accreditation accreditation);
    }
}