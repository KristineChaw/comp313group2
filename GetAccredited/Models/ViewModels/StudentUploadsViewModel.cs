using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.ViewModels
{
    public class StudentUploadsViewModel
    {
        public IQueryable<Upload> Uploads { get; set; }
        public ApplicationUser Student { get; set; }
    }
}