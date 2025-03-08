using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Skincare.Services.Implements
{
    public class FileService : IFileService
    {
        private readonly string _rootPath;
        private readonly ILogger<FileService> _logger;

        private const int MaxImageWidth = 800;
        private const int MaxImageHeight = 800;

        /// <summary>
        /// Constructor nhận vào đường dẫn gốc (rootPath) thay cho IWebHostEnvironment
        /// </summary>
        /// <param name="rootPath">Đường dẫn gốc (VD: "C:\\MyProject\\wwwroot")</param>
        /// <param name="logger"></param>
        public FileService(string rootPath, ILogger<FileService> logger)
        {
            _rootPath = rootPath;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(string base64Image)
        {
            try
            {
                if (string.IsNullOrEmpty(base64Image)) return null;

                // Loại bỏ header của base64 nếu có (vd: "data:image/png;base64,xxx")
                var base64Data = base64Image.Contains(",")
                    ? base64Image.Substring(base64Image.IndexOf(",") + 1)
                    : base64Image;

                // Convert base64 sang mảng byte
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                // Load ảnh bằng ImageSharp
                using (var image = Image.Load(imageBytes))
                {
                    // Resize nếu ảnh quá lớn
                    if (image.Width > MaxImageWidth || image.Height > MaxImageHeight)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(MaxImageWidth, MaxImageHeight),
                            Mode = ResizeMode.Max
                        }));
                    }

                    // Tạo tên file duy nhất
                    string fileName = $"{Guid.NewGuid()}.jpg";

                    // Ghép đường dẫn tới thư mục "uploads"
                    string uploadsFolder = Path.Combine(_rootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Nối full path của file
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    // Lưu ảnh dưới dạng JPEG
                    await image.SaveAsJpegAsync(filePath);

                    // Trả về đường dẫn (relative) nếu bạn muốn dùng cho web
                    // Giả sử bạn mapping "wwwroot" là "/", thì:
                    // => "/uploads/<tên file>"
                    return $"/uploads/{fileName}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image");
                throw;
            }
        }

        public void DeleteImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath)) return;

                // Nếu imagePath = "/uploads/xxx.jpg", ta bỏ ký tự '/' ở đầu
                // rồi ghép với _rootPath
                string relativePath = imagePath.TrimStart('/');
                string fullPath = Path.Combine(_rootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
                throw;
            }
        }
    }
}
