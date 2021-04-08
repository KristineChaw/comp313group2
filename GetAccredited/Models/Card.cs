using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    /// <summary>
    /// The entity that holds debit/credit card information of student accounts.
    /// </summary>
    public class Card
    {
        [Key]
        public string CustomerId { get; set; }
        public bool IsCredit { get; set; }
        public string Holder { get; set; }
        public string Number { get; set; }
        public DateTime Expiry { get; set; }
        public string Code { get; set; }

        /// <summary>
        /// Determines whether the card has expired or not.
        /// </summary>
        public bool IsExpired { get => Expiry < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); }
    }
}