namespace pyreApi.DTOs.Common
{
    public class BaseResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class BaseResponseDto<T> : BaseResponseDto
    {
        public T? Data { get; set; }
    }
}
