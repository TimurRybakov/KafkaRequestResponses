using System.Collections.Concurrent;

namespace KafkaRequestResponse.Gateway.Services.KafkaResponseRouter;

internal sealed class KafkaResponseRouter : IKafkaResponseRouter
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<string>> _waiters = new();

    public Task<string> RegisterWaiter(Guid id, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_waiters.TryAdd(id, tcs))
            throw new InvalidOperationException($"Duplicate correlation {id}");

        // авто-таймаут
        _ = Task.Delay(timeout).ContinueWith(_ => {
            if (_waiters.TryRemove(id, out var old))
                old.TrySetException(new TimeoutException());
        });

        return tcs.Task;
    }

    public void Complete(Guid id, string response)
    {
        if (_waiters.TryRemove(id, out var tcs))
            tcs.TrySetResult(response);
    }
}

