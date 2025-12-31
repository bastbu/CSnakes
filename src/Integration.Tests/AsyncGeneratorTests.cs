using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.Tests;

public class AsyncGeneratorIteratorTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task TestSendAndYield()
    {
        var mod = Env.TestAsyncGenerators();
        await using var generator = mod.SendYield(1);

        Assert.True(await generator.SendAsync(TestContext.Current.CancellationToken));
        Assert.Equal("Item 0", generator.Current);
        Assert.True(await generator.SendAsync(10, TestContext.Current.CancellationToken));
        Assert.Equal("Received 10", generator.Current);
        Assert.False(await generator.SendAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestGeneratorExhaustion()
    {
        var mod = Env.TestAsyncGenerators();
        await using var generator = mod.YieldOnly();

        List<string> result = [];
        while (await generator.SendAsync(TestContext.Current.CancellationToken))
        {
            result.Add(generator.Current);
        }

        Assert.Equal(["one", "two"], result);
    }

    [Fact]
    public async Task TestDisposalClosesGenerator()
    {
        var mod = Env.TestAsyncGenerators();

        async Task Act()
        {
            await using var generator = mod.ErroneousCleanup();
            _ = await generator.SendAsync(TestContext.Current.CancellationToken);
        }

        var exception = await Record.ExceptionAsync(Act);

        Assert.Equal("Cleanup error", exception?.InnerException?.Message);
    }

    [Fact]
    public async Task TestDisposalIsIdempotent()
    {
        var mod = Env.TestAsyncGenerators();

        var generator = mod.ErroneousCleanup();

        await generator.DisposeAsync();
        await generator.DisposeAsync();
    }

    [Fact]
    public async Task TestCancellation()
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        await using var generator = Env.TestAsyncGenerators().YieldOnly();

        await generator.SendAsync(cts.Token);

        cts.Cancel();

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(async () => await generator.SendAsync(cts.Token));
        Assert.Equal(cts.Token, ex.CancellationToken);
    }
}

public class AsyncGeneratorIteratorEnumeratorTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SendAndYield()
    {
        var mod = Env.TestAsyncGenerators();
        await using var generator = mod.SendYield(1).GetAsyncEnumerator(TestContext.Current.CancellationToken);

        Assert.True(await generator.MoveNextAsync());
        Assert.Equal("Item 0", generator.Current);
        Assert.False(await generator.MoveNextAsync());
    }

    [Fact]
    public async Task YieldOnly()
    {
        var mod = Env.TestAsyncGenerators();
        await using var generator = mod.YieldOnly().GetAsyncEnumerator(TestContext.Current.CancellationToken);

        List<string> result = [];
        while (await generator.MoveNextAsync())
        {
            result.Add(generator.Current);
        }

        Assert.Equal(["one", "two"], result);
    }

    [Fact]
    public async Task TestDisposalClosesGenerator()
    {
        var mod = Env.TestAsyncGenerators();

        async Task Act()
        {
            await using var generator = mod.ErroneousCleanup().GetAsyncEnumerator(TestContext.Current.CancellationToken);
            _ = await generator.MoveNextAsync();
        }

        var exception = await Record.ExceptionAsync(Act);

        Assert.Equal("Cleanup error", exception?.InnerException?.Message);
    }

    [Fact]
    public async Task TestDisposalIsIdempotent()
    {
        var mod = Env.TestAsyncGenerators();

        var generator = mod.ErroneousCleanup().GetAsyncEnumerator(TestContext.Current.CancellationToken);

        await generator.DisposeAsync();
        await generator.DisposeAsync();
    }

    [Fact]
    public async Task TestEnumeratorAsEnumerable()
    {
        var mod = Env.TestAsyncGenerators();
        await using var enumerator = mod.YieldOnly().GetAsyncEnumerator(TestContext.Current.CancellationToken);

        List<string> result = [];
        await foreach (var element in enumerator.ToAsyncEnumerable())
        {
            result.Add(element);
        }

        Assert.Equal(["one", "two"], result);
    }

    [Fact]
    public async Task TestCancellation()
    {
        var mod = Env.TestAsyncGenerators();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        await using var generator = mod.YieldOnly().GetAsyncEnumerator(cts.Token);

        await generator.MoveNextAsync();

        cts.Cancel();

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(async () => await generator.MoveNextAsync());
        Assert.Equal(cts.Token, ex.CancellationToken);
    }
}
