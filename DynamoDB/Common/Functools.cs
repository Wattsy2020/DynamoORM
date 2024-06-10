namespace Common;

public static class Functools
{
    /// <summary>
    /// Wait for a predicate to be true
    /// </summary>
    public static async Task WaitForPredicateAsync(Func<Task<bool>> predicateAsync, TimeSpan? checkInterval = null)
    {
        checkInterval ??= TimeSpan.FromSeconds(1);
        
        do
        {
            await Task.Delay(checkInterval.Value);
        }
        while (!await predicateAsync());
    }
}