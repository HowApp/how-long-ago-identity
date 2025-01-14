namespace HowIdentity.Common.ResultType;

public class ErrorResult
{
    public string Key {get;set;}
    public string Message {get;set;}

    public ErrorResult()
    {
    }
    public ErrorResult(string key, string message)
    {
        Key = key;
        Message = message;
    }
}