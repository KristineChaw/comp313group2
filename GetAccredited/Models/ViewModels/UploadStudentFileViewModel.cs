using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.ViewModels
{
    public class UploadStudentFileViewModel
    {
        [Required]
        public Upload Upload { get; set; }
        [Required]
        public IFormFile UploadFile { get; set; }
    }
}