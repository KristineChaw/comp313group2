using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    public class Upload
    {
        public int UploadId { get; set; }
        public string StudentId { get; set; }
        public string FileURL { get; set; }
        public string Name { get; set; }
    }
}