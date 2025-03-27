using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folderName);
        bool DeleteFile(string filePath);
    }
}
