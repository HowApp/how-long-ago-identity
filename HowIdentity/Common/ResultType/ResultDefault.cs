namespace HowIdentity.Common.ResultType;

public class ResultDefault : ResultGeneric<int>
{
    private ResultDefault(bool isSuccess) : base(isSuccess)
    {
    }

    public static ResultDefault Success() => new(true) { };

    public new static ResultDefault Fatality(string key = "default", string message = "Something went wrong!") => new (false)
    {
        FailureData = new Failure
        {
            Errors = [new ErrorResult(key, message)]
        }
    };
    
    public new static ResultDefault Fatality(IEnumerable<ErrorResult> errors) => new (false)
    {
        FailureData = new Failure
        {
            Errors = errors
        }
    };
}

public class ResultGeneric<T>
{
    public bool IsSuccess { get; }
    protected Failure FailureData { get; set; }
    private Success<T> SuccessData { get; set; }

    protected ResultGeneric(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    public static ResultGeneric<T> Success(T value) => new (true)
    {
        SuccessData = new Success<T>
        {
            Value = value
        },
        FailureData = null
    };
    
    public static ResultGeneric<T> Fatality(string key = "default", string message = "Something went wrong!") => new (false)
    {
        FailureData = new Failure
        {
            Errors = new [] { new ErrorResult(key, message) }
        }
    };
    
    public static ResultGeneric<T> Fatality(IEnumerable<ErrorResult> errors) => new (false)
    {
        FailureData = new Failure
        {
            Errors = errors
        }
    };
    
    public T Value()
    {
        if (SuccessData == null)
        {
            return default!;
        }
        
        return SuccessData.Value;
    }

    public IEnumerable<ErrorResult> Errors => FailureData?.Errors ?? [];
}

public class Success<T>
{
    public T Value { get; init; }
}

public class Failure
{
    public IEnumerable<ErrorResult> Errors { get; set; } = [];
}