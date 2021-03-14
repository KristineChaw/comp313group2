using GetAccredited.Models;
using GetAccredited.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace GetAccreditedTests
{
    [TestClass]
    public class EFAccreditationRepositoryTests
    {
        private ApplicationDbContext context;

        public EFAccreditationRepositoryTests()
        {
            context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "AppDatabase")
            .Options);
        }

        [TestMethod]
        public void SaveAccreditation_UpdatingAccreditation_ChangesShouldBeSaved()
        {
            // Arrange
            var org = new Organization
            {
                OrganizationId = "G3T4CCR3D173D",
                Name = "Temporary Organization",
                Acronym = "TMP",
                Description = "This is a temporary organization.",
                WebsiteUrl = "http://www.google.ca/"
            };
            context.Organizations.Add(org);
            context.SaveChanges();

            var acc = new Accreditation
            {
                AccreditationId = 1,
                Organization = org,
                Name = "Temporary Accreditation",
                DateCreated = DateTime.Now,
                CreatorId = "jsmith",
                Type = "Finance",
                Eligibility = "Nothing to see here"
            };
            context.Accreditations.Add(acc);
            context.SaveChanges();

            EFAccreditationRepository repo = new EFAccreditationRepository(context, null);
            var path = "hello/world";

            // Act
            acc.EligibilityFileURL = path;
            repo.SaveAccreditation(acc);
            var record = context.Accreditations.First(a => a.AccreditationId == 1);

            // Assert
            Assert.AreEqual(record.EligibilityFileURL, path);
        }
    }
}
