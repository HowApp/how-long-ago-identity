namespace HowIdentity.Common.Extensions;

using ResultType;

public static class ListExtensions
{
    public static void AddError(this List<ErrorResult> list, (string KeyError, string MessageError) error)
    {
        list.Add(new ErrorResult{Key = error.KeyError, Message = error.MessageError});
    }
}