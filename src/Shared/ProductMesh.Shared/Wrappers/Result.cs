namespace ProductMesh.Shared.Wrappers;

public class Result<T>
{
    public bool Succeeded { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = [];

    public static Result<T> Success(T data, string? message = null) =>
        new() { Succeeded = true, Data = data, Message = message };

    public static Result<T> Failure(string message) =>
        new() { Succeeded = false, Message = message };

    public static Result<T> Failure(List<string> errors) =>
        new() { Succeeded = false, Errors = errors };
}

public class Result
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = [];

    public static Result Success(string? message = null) =>
        new() { Succeeded = true, Message = message };

    public static Result Failure(string message) =>
        new() { Succeeded = false, Message = message };

    public static Result Failure(List<string> errors) =>
        new() { Succeeded = false, Errors = errors };
}
