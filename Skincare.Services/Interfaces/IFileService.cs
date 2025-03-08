public interface IFileService
{
    Task<string> SaveImageAsync(string base64Image);
    void DeleteImage(string imagePath);
} 