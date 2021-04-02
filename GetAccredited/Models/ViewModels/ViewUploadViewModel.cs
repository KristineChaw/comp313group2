using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.ViewModels
{
    public class ViewUploadViewModel
    {
        public Upload Document { get; set; } // the PDF document being displayed
        public ApplicationUser Student { get; set; } // the uploader of the document
    }
}