namespace CSnakes.Runtime.Python;

public interface IAsyncGeneratorIterator<out TYield, in TSend> :
    IAsyncEnumerator<TYield>, IAsyncDisposable, IAsyncGeneratorIterator
{
    ValueTask<bool> SendAsync(TSend value);
}

public interface IAsyncGeneratorIterator { }
