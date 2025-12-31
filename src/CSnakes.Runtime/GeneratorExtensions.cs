namespace CSnakes.Runtime;

public static class GeneratorExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IAsyncEnumerator<T> generator)
    {
        ArgumentNullException.ThrowIfNull(generator);

        await using (generator.ConfigureAwait(false))
        {
            while (await generator.MoveNextAsync().ConfigureAwait(false))
            {
                yield return generator.Current;
            }
        }
    }
}
