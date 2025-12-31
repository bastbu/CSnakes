namespace CSnakes.Runtime.Python;

public sealed class AsyncGeneratorIterator<TYield, TSend>(PyObject coroutine) :
    AsyncGeneratorIterator<TYield, TSend, PyObjectImporters.Runtime<TYield>>(coroutine);

public class AsyncGeneratorIterator<TYield, TSend, TYieldImporter>(PyObject asyncGenerator) :
    IAsyncGeneratorIterator<TYield, TSend>
    where TYieldImporter : IPyObjectImporter<TYield>
{
    private bool disposed = false;
    private readonly PyObject asyncGenerator = asyncGenerator;
    private readonly PyObject aclosePyFunction = asyncGenerator.GetAttr("aclose");
    private readonly PyObject asendPyFunction = asyncGenerator.GetAttr("asend");

    private TYield current = default!;

    public TYield Current => current;

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (disposed)
            return;

        asyncGenerator.Dispose();
        await new Coroutine<PyObject, PyObjectImporters.None>(aclosePyFunction.Call()).AsTask().ConfigureAwait(false);
        aclosePyFunction.Dispose();
        asendPyFunction.Dispose();

        disposed = true;
    }

    public IAsyncEnumerator<TYield> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new Enumerator(this, cancellationToken);

    public async ValueTask<bool> SendAsync(TSend value, CancellationToken cancellationToken)
    {
        using var valuePyObject = PyObject.From(value);
        return await SendAsync(valuePyObject, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<bool> SendAsync(CancellationToken cancellationToken) =>
        SendAsync(PyObject.None, cancellationToken);

    private async ValueTask<bool> SendAsync(PyObject value, CancellationToken cancellationToken)
    {
        try
        {
            var coro = new Coroutine<TYield, TYieldImporter>(asendPyFunction.Call(value));
            current = await coro.AsTask(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (PythonInvocationException ex) when (ex.PythonExceptionType is "StopAsyncIteration")
        {
            return false;
        }
    }

    private sealed class Enumerator(AsyncGeneratorIterator<TYield, TSend, TYieldImporter> asyncGenerator,
                                    CancellationToken cancellationToken) : IAsyncEnumerator<TYield>
    {
        public TYield Current => asyncGenerator.Current;
        public ValueTask DisposeAsync() => asyncGenerator.DisposeAsync();
        public ValueTask<bool> MoveNextAsync() => asyncGenerator.SendAsync(cancellationToken);
    }
}
