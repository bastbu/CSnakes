namespace CSnakes.Runtime;

public static class GeneratorExtensions
{
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IAsyncEnumerator<T> generator)
    {
        ArgumentNullException.ThrowIfNull(generator);

        return Core(generator);

        static async IAsyncEnumerable<T> Core(IAsyncEnumerator<T> generator)
        {
            await using (generator.ConfigureAwait(false))
            {
                while (await generator.MoveNextAsync().ConfigureAwait(false))
                {
                    yield return generator.Current;
                }
            }
        }
    }
}
