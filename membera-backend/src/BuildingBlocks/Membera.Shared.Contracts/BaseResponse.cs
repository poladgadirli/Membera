namespace Membera.Shared.Contracts;

public class BaseResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static BaseResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new BaseResponse<T> { Success = true, Data = data, Message = message };
    }

    public static BaseResponse<T> FailureResponse(string message, List<string>? errors = null)
    {
        return new BaseResponse<T> { Success = false, Message = message, Errors = errors };
    }
}