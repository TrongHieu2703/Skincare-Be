using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Skincare.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class FileService : IFileService
    {
        private readonly string _uploadPath;
        private readonly string _avatarFolder;
        private readonly string _productFolder;
        private readonly ILogger<FileService> _logger;

        public FileService(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, ILogger<FileService> logger)
        {
            _uploadPath = configuration["FileSettings:UploadPath"] ?? "wwwroot";
            _avatarFolder = configuration["FileSettings:AvatarFolder"] ?? "avatar-images";
            _productFolder = configuration["FileSettings:ProductFolder"] ?? "product-images";
            _logger = logger;

            _uploadPath = Path.Combine(webHostEnvironment.ContentRootPath, _uploadPath);

            _logger.LogInformation($"FileService initialized with UploadPath: {_uploadPath}");
            _logger.LogInformation($"Avatar Folder: {_avatarFolder}, Product Folder: {_productFolder}");

            EnsureDirectoryExists(_uploadPath);
            EnsureDirectoryExists(Path.Combine(_uploadPath, _avatarFolder));
            EnsureDirectoryExists(Path.Combine(_uploadPath, _productFolder));
        }

        private void EnsureDirectoryExists(string folderPath)
        {
            if (Directory.Exists(folderPath)) return;

            try
            {
                Directory.CreateDirectory(folderPath);
                _logger.LogInformation($"Created directory: {folderPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create directory: {folderPath}");
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            try
            {
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadDir = Path.Combine(_uploadPath, folderName);

                EnsureDirectoryExists(uploadDir);

                var filePath = Path.Combine(uploadDir, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var returnPath = $"/{folderName}/{fileName}";
                _logger.LogInformation($"File saved to: {filePath}");
                _logger.LogInformation($"Returning path: {returnPath}");
                return returnPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving file to {folderName}. Original filename: {file.FileName}");
                return null;
            }
        }

        public bool DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            try
            {
                _logger.LogInformation($"Original file path for deletion: {filePath}");
                
                // Normalize file path
                var normalizedPath = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(_uploadPath, normalizedPath);

                _logger.LogInformation($"Attempting to delete file at: {fullPath}");

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Successfully deleted file: {fullPath}");
                    return true;
                }

                _logger.LogWarning($"File not found for deletion: {fullPath}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}. Error: {ex.Message}");
                return false;
            }
        }
    }
}
