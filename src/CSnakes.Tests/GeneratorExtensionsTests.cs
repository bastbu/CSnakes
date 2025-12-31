using CSnakes.Runtime;

namespace CSnakes.Tests;

public sealed class GeneratorExtensionsTests
{
    [Fact]
    public async Task ToAsyncEnumerableExhaustsEnumerator()
    {
        var enumerator = ToAsyncEnumerable([1, 2, 3]).GetAsyncEnumerator(TestContext.Current.CancellationToken);

        List<int> result = [];
        await foreach (var element in enumerator.ToAsyncEnumerable())
        {
            result.Add(element);
        }

        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public async Task ToAsyncEnumerableThrowsOnNullEnumerator()
    {
        Assert.Throws<ArgumentNullException>(() => GeneratorExtensions.ToAsyncEnumerable<int>(null!));
    }

    [Fact]
    public async Task ToAsyncEnumerableIsNotIdempotent()
    {
        var enumerator = ToAsyncEnumerable([1, 2, 3]).GetAsyncEnumerator(TestContext.Current.CancellationToken);

        // Exhaus the enumerator
        await foreach (var element in enumerator.ToAsyncEnumerable()) { }

        // Second enumeration yields no elements
        List<int> result = [];
        await foreach (var element in enumerator.ToAsyncEnumerable())
        {
            result.Add(element);
        }

        Assert.Empty(result);
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }
    }
}
