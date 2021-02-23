using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Organization> Organizations { get; set; }

        public DbSet<Accreditation> Accreditations { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Card> Cards { get; set; }

        public DbSet<Request> Requests { get; set; }
    }
}