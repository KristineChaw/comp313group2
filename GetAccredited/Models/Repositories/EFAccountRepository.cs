﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.Repositories
{
    public class EFAccountRepository : IAccountRepository
    {
        private ApplicationDbContext context;

        public EFAccountRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IQueryable<Upload> Uploads => context.Uploads;

        public void SaveUpload(Upload upload)
        {
            Upload uploadEntry = context.Uploads.FirstOrDefault(u => u.UploadId == upload.UploadId);

            if (uploadEntry == null)
            {
                context.Uploads.Add(upload);
            } else
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
            } else
            {
                context.Uploads.Remove(upload);
                context.SaveChanges();
            }

            return upload;
        }
    }
}