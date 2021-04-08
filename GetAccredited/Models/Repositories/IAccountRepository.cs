using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetAccredited.Models.Repositories
{
    public interface IAccountRepository
    {
        IQueryable<Upload> Uploads { get; }
        void SaveUpload(Upload upload);

        Upload DeleteUpload(int uploadId);
        void DeleteUploadsByUser(string userId);
    }
}