namespace CSnakes.Runtime.Python;

public interface IAsyncGenerator<out TYield, in TSend> : IAsyncGenerator, IAsyncDisposable
{
    ValueTask<bool> SendAsync(CancellationToken cancellationToken);
    ValueTask<bool> SendAsync(TSend value, CancellationToken cancellationToken);
    IAsyncEnumerator<TYield> GetAsyncEnumerator(CancellationToken cancellationToken = default);
    TYield Current { get; }
}

public interface IAsyncGenerator { }
