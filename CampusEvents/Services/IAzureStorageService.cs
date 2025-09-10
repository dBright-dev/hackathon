namespace CampusEvents.Services
{
    public interface IAzureStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName);
        Task<bool> DeleteFileAsync(string fileName, string containerName);
    }
}
