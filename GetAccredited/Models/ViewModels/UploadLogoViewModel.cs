using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.ViewModels
{
    public class UploadLogoViewModel
    {
        public Organization Organization { get; set; }
        public IFormFile Logo { get; set; }
    }
}