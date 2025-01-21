namespace HowIdentity.Common.Extensions;

using ResultType;

public static class CollectionExtensions
{
    public static void AddError(this List<ErrorResult> list, (string KeyError, string MessageError) error)
    {
        list.Add(new ErrorResult{Key = error.KeyError, Message = error.MessageError});
    }
    
    /// <summary>
    /// Compares two collections by a specified key and returns the items to remove and add.
    /// </summary>
    /// <typeparam name="T">Type of items in the collections.</typeparam>
    /// <typeparam name="TKey">Type of the key used for comparison.</typeparam>
    /// <param name="existingItems">The current collection of exists items</param>
    /// <param name="newItems">The new collection.</param>
    /// <param name="keySelector">A function to select the key for comparison.</param>
    /// <returns>A tuple containing items to remove and items to add.</returns>
    public static (List<T> toRemove, List<T> toAdd) CompareByKey<T, TKey>(
        this IEnumerable<T> existingItems, 
        IEnumerable<T> newItems, 
        Func<T, TKey> keySelector)
    {
        if (existingItems == null)
        {
            existingItems = Enumerable.Empty<T>();
        }
            
        if (newItems == null)
        {
            newItems = Enumerable.Empty<T>();
        }
        
        var existingKeys = existingItems.Select(keySelector).ToHashSet();
        var newKeys = newItems.Select(keySelector).ToHashSet();

        // Find items to remove (exist in existing collection but not in new collection)
        var toRemove = existingItems
            .Where(item => !newKeys.Contains(keySelector(item)))
            .ToList();

        // Find items to add (exist in new collection but not in existing collection)
        var toAdd = newItems
            .Where(item => !existingKeys.Contains(keySelector(item)))
            .ToList();

        return (toRemove, toAdd);
    }
}