using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly string _folderId;
        private readonly ILogger<GoogleDriveService> _logger;
        private const int ChunkSize = 64 * 1024 * 1024; // 64MB chunks

        public GoogleDriveService(
            IConfiguration configuration,
            ILogger<GoogleDriveService> logger)
        {
            _logger = logger;
            _folderId = configuration["GoogleDrive:FolderId"];

            var credentialsPath = configuration["GoogleDrive:CredentialsPath"];
            var credential = GoogleCredential
                .FromFile(credentialsPath)
                .CreateScoped(DriveService.ScopeConstants.DriveFile);

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = configuration["GoogleDrive:ApplicationName"]
            });
        }

        public async Task<(string fileId, string fileName, string fileType, string fileUrl)> 
            UploadFile(IFormFile file)
        {
            try
            {
                // Map MIME types for images
                var contentType = file.ContentType;
                if (string.IsNullOrEmpty(contentType))
                {
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    contentType = extension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        _ => file.ContentType
                    };
                }

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = file.FileName,
                    Parents = new List<string> { _folderId },
                    MimeType = contentType
                };

                using var stream = file.OpenReadStream();
                var request = _driveService.Files.Create(fileMetadata, stream, contentType);
                
                request.ChunkSize = ChunkSize;
                request.Fields = "id, name, mimeType, webViewLink";

                var results = await request.UploadAsync();
                
                if (results.Status != UploadStatus.Completed)
                {
                    _logger.LogError($"Upload failed: {results.Status}");
                    throw new Exception($"Upload failed with status: {results.Status}");
                }

                // Set file permissions to anyone with the link can view
                var permission = new Google.Apis.Drive.v3.Data.Permission
                {
                    Type = "anyone",
                    Role = "reader"
                };
                await _driveService.Permissions.Create(permission, request.ResponseBody.Id).ExecuteAsync();

                // Get direct thumbnail URL for images
                var fileUrl = $"https://drive.google.com/thumbnail?id={request.ResponseBody.Id}&sz=w1000";

                return (
                    request.ResponseBody.Id,
                    request.ResponseBody.Name,
                    request.ResponseBody.MimeType,
                    fileUrl
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteFile(string fileId)
        {
            if (string.IsNullOrEmpty(fileId)) return;

            try
            {
                try
                {
                    var file = await _driveService.Files.Get(fileId).ExecuteAsync();
                    if (file != null)
                    {
                        await _driveService.Files.Delete(fileId).ExecuteAsync();
                        _logger.LogInformation($"Successfully deleted file with ID: {fileId}");
                    }
                }
                catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation($"File with ID {fileId} not found, skipping delete");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error checking file existence: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting file: {ex.Message}");
                throw;
            }
        }
    }
} 