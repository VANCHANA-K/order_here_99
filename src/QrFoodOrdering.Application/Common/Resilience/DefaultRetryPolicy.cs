using Microsoft.Extensions.Logging;

namespace QrFoodOrdering.Application.Common.Resilience;

public sealed class DefaultRetryPolicy : IRetryPolicy
{
    private readonly ILogger<DefaultRetryPolicy> _logger;
    private readonly RetryOptions _options;

    public DefaultRetryPolicy(ILogger<DefaultRetryPolicy> logger)
        : this(logger, new RetryOptions()) { }

    public DefaultRetryPolicy(ILogger<DefaultRetryPolicy> logger, RetryOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken ct)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        for (var attempt = 1; attempt <= _options.MaxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                await action(ct);
                return;
            }
            catch (Exception ex) when (attempt < _options.MaxAttempts && ShouldRetry(ex, ct))
            {
                var delay = ComputeDelay(attempt);
                _logger.LogWarning(
                    ex,
                    "Retrying operation attempt {Attempt}/{Max} after {Delay} due to transient error.",
                    attempt,
                    _options.MaxAttempts,
                    delay
                );
                await Task.Delay(delay, ct);
            }
        }

        // Final attempt (no catch filter): let exception bubble
        await action(ct);
    }

    private bool ShouldRetry(Exception ex, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return false;
        if (_options.ShouldRetry is not null)
            return _options.ShouldRetry(ex);

        // Default: retry on timeouts and task cancellations (not due to our ct)
        return ex is TimeoutException || ex is TaskCanceledException;
    }

    private TimeSpan ComputeDelay(int attempt)
    {
        var backoff =
            _options.BaseDelay.TotalMilliseconds * Math.Pow(_options.BackoffFactor, attempt - 1);
        var jitter = Random.Shared.NextDouble() * _options.JitterRatio;
        var ms = backoff * (1.0 + jitter);
        var delay = TimeSpan.FromMilliseconds(ms);
        return delay <= _options.MaxDelay ? delay : _options.MaxDelay;
    }
}

public sealed class RetryOptions
{
    public int MaxAttempts { get; init; } = 3;
    public TimeSpan BaseDelay { get; init; } = TimeSpan.FromMilliseconds(200);
    public double BackoffFactor { get; init; } = 2.0;
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(2);
    public double JitterRatio { get; init; } = 0.2; // up to +20% jitter
    public Func<Exception, bool>? ShouldRetry { get; init; }
}
