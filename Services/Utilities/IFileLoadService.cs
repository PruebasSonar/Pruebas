﻿using Microsoft.Extensions.FileProviders;
using PIPMUNI_ARG.Models.Domain;

namespace JaosLib.Services.Utilities
{
    public interface IFileLoadService
    {
        Task<int> UploadFile(string fileName, IFormFile fileInfo, string filePath);
        string errMessage(int result);
        /// <summary>
        /// Returns the standard name that will be used to store the file in the server.
        /// </summary>
        /// <param name="parentId">The id of the parent that owns the attachment</param>
        /// <param name="recordId">The id of the record containing the attachment information.</param>
        /// <param name="fileInfo">The file to be stored. Used to get the file extension.</param>
        /// <returns></returns>
        string serverFileName(string prefix, int parentId, int recordId, IFormFile fileInfo);
        string serverFullPath(string partialPath);

    }
}
