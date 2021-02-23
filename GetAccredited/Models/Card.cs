using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    public class Card
    {
        [Key]
        public string CustomerId { get; set; }
        public bool IsCredit { get; set; }
        public string Holder { get; set; }
        public string Number { get; set; }
        public DateTime Expiry { get; set; }
        public string Code { get; set; }

        public bool IsExpired { get => Expiry < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); }
    }
}