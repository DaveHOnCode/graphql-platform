using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.AspNetCore.Mvc;

namespace HotChocolate.AspNetCore.Tests.Utilities;

[ExtendObjectType("Query")]
public class QueryExtension
{
    private readonly DateTime _time = DateTime.UtcNow;

    public long Time() => _time.Ticks;

    public bool Evict([FromServices] IRequestExecutorResolver executorResolver, ISchema schema)
    {
        executorResolver.EvictRequestExecutor(schema.Name);
        return true;
    }

    public async Task<bool> Wait(int m, CancellationToken ct)
    {
        await Task.Delay(m, ct);
        return true;
    }
}
