using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.ViewModels
{
    public class AccreditationViewModel
    {
        public Accreditation Accreditation { get; set; }
        public IFormFile Eligibility { get; set; }
    }
}