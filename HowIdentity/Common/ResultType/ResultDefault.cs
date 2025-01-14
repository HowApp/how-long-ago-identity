namespace HowIdentity.Common.ResultType;

public class ResultDefault : ResultGeneric<int>
{
    private ResultDefault(bool isSuccess) : base(isSuccess)
    {
    }

    public static ResultDefault Success() => new(true) { };

    public new static ResultDefault Fatality() => new (false)
    {
        FailureData = new Failure
        {
            Errors = [new ErrorResult("default", "Something went wrong!")]
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
    protected Failure? FailureData { get; set; }
    private Success<T>? SuccessData { get; set; }

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
    
    public static ResultGeneric<int> Fatality() => new (false)
    {
        SuccessData = new Success<int>
        {
            Value = -1
        },
        FailureData = new Failure
        {
            Errors = new [] { new ErrorResult("default", "Something went wrong!") }
        }
    };
    
    public static ResultGeneric<int> Fatality(IEnumerable<ErrorResult> errors) => new (false)
    {
        SuccessData = new Success<int>
        {
            Value = -1
        },
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

    public IEnumerable<ErrorResult> Errors()
    {
        return FailureData?.Errors ?? [];
    }
}

public class Success<T>
{
    public T Value { get; init; }
}

public class Failure
{
    public IEnumerable<ErrorResult> Errors { get; set; } = [];
}