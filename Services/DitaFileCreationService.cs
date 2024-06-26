﻿using System.Text.RegularExpressions;
using AppConfgDocumentation.enms;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AppConfgDocumentation.Services
{

    public delegate string HandleDitaFileCreation(string xmlContent, string outputPath, string filename);
    public interface IDitaFileCreationService
    {
        public string SaveDitaFile(string xmlContent, string outputPath, string filename, DitaFileExtensions fileExtension = DitaFileExtensions.dita, string roleName = "");
        public string ReplaceInvalidChars(string title);
        public void CreateFolderForDocument(string title);
        public string RenameFolder(string oldFolderName, string newFolderName);
        public void DeleteDocumentFolder(string folderPath);
    }

    // DitaFileCreationService.cs
    public class DitaFileCreationService : IDitaFileCreationService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IDitaValidationService _ditaValidationService;

        public DitaFileCreationService(IWebHostEnvironment hostingEnvironment, IDitaValidationService ditaValidationService)
        {
            _hostingEnvironment = hostingEnvironment;
            _ditaValidationService = ditaValidationService;
        }

        public string SaveDitaFile(string xmlContent, string outputPath, string filename, DitaFileExtensions fileExtension = DitaFileExtensions.dita, string roleName = "")
        {
            // var isValid = _ditaValidationService.ValidateDitaXml(xmlContent, out string validationErrors);
            // if (!isValid)
            // {
            //     throw new InvalidOperationException($"The XML content is not valid: {validationErrors}");
            // }
            var extension = fileExtension == DitaFileExtensions.dita ? ".dita" : ".ditamap";
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, outputPath, $"{ReplaceInvalidChars(filename)}{roleName}{extension}");
            File.WriteAllText(filePath, xmlContent);
            return filePath;
        }

        //delete forlder 
        public void DeleteDocumentFolder(string folderPath)
        {
            folderPath = Path.Combine(_hostingEnvironment.WebRootPath, ReplaceInvalidChars(folderPath));
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }
        public void CreateFolderForDocument(string title)
        {
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, title);

            if (folderPath != null && !Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }
        public string RenameFolder(string oldFolderName, string newFolderName)
        {
            var folderName = ReplaceInvalidChars(newFolderName);
            string existingFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, oldFolderName);
            string newFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, folderName);
            if (Directory.Exists(existingFolderPath))
            {
                if (!Directory.Exists(newFolderPath))
                {
                    Directory.Move(existingFolderPath, newFolderPath);
                }
            }
            return folderName;
        }
        public string ReplaceInvalidChars(string title)
        {
            // Remove any characters that are not allowed in folder names
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+|\s)", invalidChars);

            return Regex.Replace(title, invalidRegStr, "_");
        }
    }
}
