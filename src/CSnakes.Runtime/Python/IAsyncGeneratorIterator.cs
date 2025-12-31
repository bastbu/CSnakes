namespace CSnakes.Runtime.Python;

public interface IAsyncGeneratorIterator<out TYield, in TSend> : IAsyncGeneratorIterator, IAsyncDisposable
{
    ValueTask<bool> SendAsync(CancellationToken cancellationToken);
    ValueTask<bool> SendAsync(TSend value, CancellationToken cancellationToken);
    IAsyncEnumerator<TYield> GetAsyncEnumerator(CancellationToken cancellationToken = default);
    TYield Current { get; }
}

public interface IAsyncGeneratorIterator { }
