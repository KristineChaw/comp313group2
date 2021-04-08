using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.Repositories
{
    public class EFAccountRepository : IAccountRepository
    {
        private ApplicationDbContext context;
        private IWebHostEnvironment env;

        public EFAccountRepository(ApplicationDbContext ctx, IWebHostEnvironment _env)
        {
            context = ctx;
            env = _env;
        }

        public IQueryable<Upload> Uploads => context.Uploads;

        public void SaveUpload(Upload upload)
        {
            Upload uploadEntry = context.Uploads.FirstOrDefault(u => u.UploadId == upload.UploadId);

            if (uploadEntry == null)
            {
                context.Uploads.Add(upload);
            }
            else
            {
                uploadEntry.FileURL = upload.FileURL;
                uploadEntry.Name = upload.Name;
            }

            context.SaveChanges();
        }

        public Upload DeleteUpload(int uploadId)
        {
            Upload upload = context.Uploads.FirstOrDefault(u => u.UploadId == uploadId);

            if (upload == null)
            {
                return null;
            }
            else
            {
                context.Uploads.Remove(upload);
                context.SaveChanges();
            }

            return upload;
        }

        public void DeleteUploadsByUser(string userId)
        {
            // retrieve all uploads by this user
            var uploads = context.Uploads.Where(u => u.StudentId == userId);

            if (uploads.Any())
            {
                // delete all files uploaded by this user
                foreach (var upload in uploads.ToList())
                {
                    Utility.DeleteFile(env.WebRootPath + Utility.UPLOADS_DIR + upload.FileURL);
                }

                // remove from the Uploads table all entries by this user
                context.Uploads.RemoveRange(uploads);
                context.SaveChanges();
            }
        }
    }
}