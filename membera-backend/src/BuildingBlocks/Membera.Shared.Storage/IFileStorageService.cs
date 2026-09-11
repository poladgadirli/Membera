namespace Membera.Shared.Storage;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream, string contentType);
    Task DeleteFileAsync(string bucketName, string objectName);
    string GetFileUrl(string bucketName, string objectName);
}
