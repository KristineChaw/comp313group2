using Microsoft.VisualStudio.TestTools.UnitTesting;
using GetAccredited.Models;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace GetAccreditedTests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void DeleteFile_ValidFilePath_SuccessfulDeletionAndReturnsTrue()
        {
            // Arrange
            var fileName = "tmp.txt";
            new StreamWriter(fileName).Close();
            var path = (Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly()
                .GetName().CodeBase) + $"\\{fileName}").Substring(6);

            // Act
            var result = Utility.DeleteFile(path);

            // Assert
            Assert.IsTrue(!File.Exists(path) && result);
        }

        [TestMethod]
        public void DeleteFile_InvalidFilePath_NoDeletionAndReturnsFalse()
        {
            // Arrange
            var path = "hello\\world.txt";

            // Act
            var result = Utility.DeleteFile(path);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task UploadFile_UploadTempFile_SuccessfulUpload()
        {
            // Arrange
            var fileName = "deleteme.txt";
            var sw = new StreamWriter(fileName);
            sw.Write("delete me");
            sw.Close();
            var path = (Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly()
                .GetName().CodeBase)).Substring(6);

            using (var stream = new MemoryStream(File.ReadAllBytes(path + $"\\{fileName}")))
            {
                var formFile = new FormFile(stream, 0, stream.Length, "streamFile", fileName);

                // Act
                var newFile = await Utility.UploadFile(formFile, path);

                // Assert
                Assert.IsTrue(File.Exists(path + $"\\{newFile}"));                
            }
        }

        [TestMethod]
        public async Task SendInviteEmail_InvalidRecipientEmail_ReturnsFalse()
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

            // Act
            var res1 = await Utility.SendInviteEmail(null, org, org.WebsiteUrl);
            var res2 = await Utility.SendInviteEmail("", org, org.WebsiteUrl);
            var res3 = await Utility.SendInviteEmail("hello world", org, org.WebsiteUrl);

            // Assert
            Assert.IsFalse(res1 || res2 || res3);
        }

        [TestMethod]
        public async Task SendInviteEmail_ValidRecipientEmail_ReturnsTrue()
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

            // Act
            var res = await Utility.SendInviteEmail("javenido@my.centennialcollege.ca", org, org.WebsiteUrl);

            // Assert
            Assert.IsTrue(res);
        }
    }
}