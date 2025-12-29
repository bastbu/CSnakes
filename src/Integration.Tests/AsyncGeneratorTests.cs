using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integration.Tests;
public class AsyncGeneratorTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task TestGenerator()
    {
        var mod = Env.TestAsyncGenerators();
        await using var generator = mod.SendYield(1, TestContext.Current.CancellationToken);

        Assert.True(await generator.MoveNextAsync());
        Assert.Equal("Item 0", generator.Current);
        Assert.True(await generator.SendAsync(10));
        Assert.Equal("Received 10", generator.Current);
    }

    [Fact]
    public async Task TestGeneratorExhaustion()
    {
        var mod = Env.TestAsyncGenerators();
        await using var generator = mod.YieldOnly(TestContext.Current.CancellationToken);

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
            await using (var generator = mod.ErroneousCleanup(TestContext.Current.CancellationToken))
            {
                _ = await generator.MoveNextAsync();
            }
        }

        var exception = await Record.ExceptionAsync(Act);

        Assert.Equal("Cleanup error", exception?.InnerException?.Message);
    }

    [Fact]
    public async Task TestDisposalIsIdempotent()
    {
        var mod = Env.TestAsyncGenerators();

        var generator = mod.ErroneousCleanup(TestContext.Current.CancellationToken);

        await generator.DisposeAsync();
        await generator.DisposeAsync();
    }
}
