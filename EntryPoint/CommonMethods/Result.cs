namespace EntryPoint.CommonMethods;

public class Result<T> : Result
{
    public T Value { get; }

    private Result(bool isSuccess, T value, string error, ReturnStates returnStates) : base(isSuccess, error, returnStates)
    {
        Value = value;
    }

    public static Result<T> Success(T value)
        => new Result<T>(true, value, string.Empty, ReturnStates.Ok);

    public static Result<T> Failure(string error, ReturnStates returnState)
        => new Result<T>(false, default!, error, returnState);
}

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public ReturnStates ReturnStates { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string error, ReturnStates returnStates)
    {
        IsSuccess = isSuccess;
        Error = error;
        ReturnStates = returnStates;
    }

    public static Result Success()
        => new Result(true, string.Empty, ReturnStates.Ok);

    public static Result Failure(string error, ReturnStates returnState)
        => new Result(false, error, returnState);
}

public enum ReturnStates
{
    Ok,
    NotFound,
    Conflict,
    BadRequest
}