namespace AnimalRescueSystem.Dtos;

public class ResponseDataDto<T>
{
    public bool Success { get; set; }
    public int Code { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string[]? Warnings { get; set; }
    public string[]? ValidationErrors { get; set; }

    public ResponseDataDto()
    {
    }

    public ResponseDataDto(bool success, T data)
    {
        Success = success;
        Data = data;
    }

    public ResponseDataDto(bool success, T data, string[] errors)
    {
        Success = success;
        Data = data;
        ValidationErrors = errors;
    }

    public ResponseDataDto(bool success, string[] errors)
    {
        Success = success;
        ValidationErrors = errors;
    }

    public ResponseDataDto(bool success, string error)
    {
        Success = success;
        ValidationErrors = new string[] { error };
    }
}

public class ResponseDataDto
{
    public bool Success { get; set; }
    public int Code { get; set; }
    public string? Message { get; set; }
    public string[]? Warnings { get; set; }
    public string[]? ValidationErrors { get; set; }

    public ResponseDataDto()
    {
    }

    public ResponseDataDto(bool success)
    {
        Success = success;
    }

    public ResponseDataDto(bool success, string error)
    {
        Success = success;
        ValidationErrors = new string[] { error };
    }
}
