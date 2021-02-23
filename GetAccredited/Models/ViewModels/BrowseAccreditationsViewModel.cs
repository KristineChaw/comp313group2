using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.ViewModels
{
    public class BrowseAccreditationsViewModel
    {
        public IQueryable<Accreditation> Accreditations { get; set; }
        public string SearchKey { get; set; }
        public string SearchBy { get; set; }
        public string AccreditationType { get; set; }
    }
}