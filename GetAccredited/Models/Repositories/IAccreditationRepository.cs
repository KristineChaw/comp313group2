using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.Repositories
{
    public interface IAccreditationRepository
    {
        IQueryable<Accreditation> Accreditations { get; }

        void SaveAccreditation(Accreditation accreditation);

        Accreditation DeleteAccreditation(int accreditationId);
    }
}