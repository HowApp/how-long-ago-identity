namespace HowIdentity.Common.Extensions;

using Models;

public static class ListExtensions
{
    public static void AddError(this List<PageErrorModel> list, (string KeyError, string MessageError) error)
    {
        list.Add(new PageErrorModel{KeyError = error.KeyError, MessageError = error.MessageError});
    }
}