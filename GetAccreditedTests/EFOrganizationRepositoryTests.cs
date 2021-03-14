using Microsoft.VisualStudio.TestTools.UnitTesting;
using GetAccredited.Models;
using GetAccredited.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GetAccreditedTests
{
    [TestClass]
    public class EFOrganizationRepositoryTests
    {
        private ApplicationDbContext context;

        public EFOrganizationRepositoryTests()
        {
            context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "AppDatabase")
            .Options);
        }

        [TestMethod]
        public void DeleteOrganization_WhenOrganizationExists_ShouldDeleteOrganization()
        {
            // Arrange
            var organizationId = "G3T4CCR3D173D";
            var org = new Organization
            {
                OrganizationId = organizationId,
                Name = "Temporary Organization",
                Acronym = "TMP",
                Description = "This is a temporary organization.",
                WebsiteUrl = "http://www.google.ca/"
            };
            context.Organizations.Add(org);
            context.SaveChanges();

            EFOrganizationRepository repo = new EFOrganizationRepository(context);

            // Act
            repo.DeleteOrganization(organizationId);

            // Assert
            Assert.IsFalse(context.Organizations.Any(o => o.OrganizationId == organizationId));
        }

        [TestMethod]
        public void DeleteOrganization_WhenOrganizationDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            EFOrganizationRepository repo = new EFOrganizationRepository(context);

            // Act
            var org = repo.DeleteOrganization("hello");

            // Assert
            Assert.IsNull(org);
        }
    }
}
