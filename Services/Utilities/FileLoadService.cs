﻿using Microsoft.Extensions.FileProviders;
using PIPMUNI_ARG.Models.Domain;
using System.IO;
using System.Reflection;

namespace JaosLib.Services.Utilities
{
    public class FileLoadService : IFileLoadService
    {
        static public string PathProjectImages = "/Docs/ProjectImages"; 
        static public string PathProjectAttachments = "/Docs/Projects";
        static public string PathPaymentAttachments = "/Docs/Payments";
        static public string PathPayments = "/Docs/PaymentAttachs";
        static public string PathAdditions = "/Docs/Additions";
        static public string PathAdditionAttachments = "/Docs/Additions";
        static public string PathExtensions = "/Docs/Extensions";
        static public string PathExtensionAttachments = "/Docs/Extensions";
        static public string PathVariantes = "/Docs/Variantes";
        static public string PathVarianteAttachments = "/Docs/Variantes";
        static public string PathTempFiles = "/Docs/TempFiles";

        static public string serverRootPath = "wwwroot";

        static public int maxFileSize = 1048576; // 1024 x 1024

        public const int resultOK = 100;
        public const int errorNoInfoAvailable = 200;
        public const int errorNoFileInfo = 201;
        public const int errorEmpty = 202;
        public const int errorCatch = 220;
        public const int errorFileSize = 240;

        // 
        // fileName: 
        // fileInfo: 
        /// <summary>
        /// Upload a file from Client Side to server 
        /// </summary>
        /// <param name="fileName">The name that will be used to store the file in the server.</param>
        /// <param name="fileInfo">the information obtained from the Upload Control to create the file </param>
        /// <param name="filePath">Location in the server to store the file. Use Path static variables created within the FileLoadService Class</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> UploadFile(string fileName, IFormFile fileInfo, string filePath)
        {
            string path = "";
            try
            {
                if (fileInfo != null)
                {
                    if (fileInfo.Length > 0)
                    {
                        //if (fileInfo.Length > maxFileSize) // 1024 x 1024
                        //    return errorFileSize;
                        path = serverFullPath(filePath);
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await fileInfo.CopyToAsync(fileStream);
                        }
                        return resultOK;
                    }
                    else
                        return errorEmpty;
                }
                else
                    return errorNoFileInfo;
            }
            catch
            {
                throw;
            }
        }

        public string serverFullPath(string partialPath)
        {
            return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, serverRootPath + partialPath));
        }

        /// <summary>
        /// Returns the standard name that will be used to store the file in the server.
        /// </summary>
        /// <param name="parentId">The id of the parent that owns the attachment</param>
        /// <param name="recordId">The id of the record containing the attachment information.</param>
        /// <param name="fileInfo">The file to be stored. Used to get the file extension.</param>
        /// <returns></returns>
        public string serverFileName(string prefix, int parentId, int recordId, IFormFile fileInfo)
        {
            if (parentId > 0 && recordId > 0 && fileInfo != null)
            {
                string name = prefix + "_p" + parentId + "r" + recordId;
                string extension = fileInfo.FileName;
                if (extension.Contains("."))
                    name += extension.Substring(extension.LastIndexOf("."));
                return name;
            }
            return "";
        }


        public string errMessage(int result)
        {
            switch (result)
            {
                case resultOK:
                    return "Archivo cargado correctamente."; //"File Upload Successful";

                case -1:
                    return "Error interno cargando archivo.";  //"Invalid result file name";
                case errorNoInfoAvailable:
                    return "No hay información disponible."; //"No information available";
                case errorNoFileInfo:
                    return "No se encuentra información de archivo."; //"No file information available";
                case errorEmpty:
                    return "Archivo vacío."; //"File is empty";
                case errorCatch:
                    return "Error cargando archivo."; //"Error loading file";
                case errorFileSize:
                    return "Tamaño máximo de archivo " + maxFileSize.ToString("n0"); //"File size limit is 1024x1024";
                default:
                    return "No se pudo cargar el archivo."; //"File Upload Failed";
            }
        }





        public class UploadFileException : Exception
        {
            public UploadFileException(string? clientFileName, string? serverFileName)
                : base(FormatErrorMessage(clientFileName, serverFileName))
            {
            }

            private static string FormatErrorMessage(string? clientFileName, string? serverFileName)
            {
                return string.Format("Error cargando '{0}' en: '{1}'", clientFileName ?? "", serverFileName ?? "");
            }
        }

    }
}
