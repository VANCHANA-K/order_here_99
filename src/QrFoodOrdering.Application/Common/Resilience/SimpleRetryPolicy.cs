using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QrFoodOrdering.Application.Common.Resilience;

public sealed class SimpleRetryPolicy : IRetryPolicy
{
    private readonly ILogger<SimpleRetryPolicy> _logger;

    public SimpleRetryPolicy(ILogger<SimpleRetryPolicy> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken ct)
    {
        const int maxAttempts = 3;
        var delayMs = 100;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                await action(ct);
                return;
            }
            catch (TimeoutException ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(ex, "TransientFailureRetryable {@data}", new { Attempt = attempt });
                await Task.Delay(delayMs, ct);
                delayMs *= 2; // backoff (100ms, 200ms, 400ms)
            }
        }
    }
}

