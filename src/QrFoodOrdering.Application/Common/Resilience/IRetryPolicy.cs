using System;
using System.Threading;
using System.Threading.Tasks;

namespace QrFoodOrdering.Application.Common.Resilience;

public interface IRetryPolicy
{
    Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken ct);
}
