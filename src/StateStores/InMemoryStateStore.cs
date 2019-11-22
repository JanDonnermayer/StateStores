using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace StateStores
{
    public class InMemoryStateStore : IStateStore
    {
        private ImmutableDictionary<Type, object> mut_stateMap =
            ImmutableDictionary<Type, object>.Empty;
        private readonly ReplaySubject<Type> keySubject =
            new ReplaySubject<Type>(TaskPoolScheduler.Default);
        private readonly SemaphoreSlim semaphore =
            new SemaphoreSlim(1, 1);

        private async Task<IDisposable> GetLockAsync()
        {
            await semaphore.WaitAsync();
            return Disposable.Create(() => semaphore.Release());
        }

        private ImmutableDictionary<string, TValue> GetTypedMap<TValue>() =>
            (ImmutableDictionary<string, TValue>)ImmutableInterlocked.GetOrAdd(
                location: ref mut_stateMap,
                key: typeof(TValue),
                valueFactory: _ => ImmutableDictionary<string, TValue>.Empty);

        public IObservable<IEnumerable<KeyValuePair<string, T>>> GetObservable<T>() =>
            keySubject
                .Where(k => k is T)
                .Select(_ =>
                    GetTypedMap<TokenStatePair<T>>()
                    .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value.State));


        public async Task<bool> TryRemoveAsync<T>(string key, string token)
        {
            using var _ = await GetLockAsync();

            var map = GetTypedMap<TokenStatePair<T>>();
            if (!map.TryGetValue(key, out var tsp) || tsp.Token != token) return false;

            ImmutableInterlocked.Update(
                location: ref mut_stateMap,
                transformer: m => m.SetItem(typeof(T), map.Remove(key)));

            keySubject.OnNext(typeof(T));

            return true;
        }

        public async Task<bool> TrySetAsync<T>(string key, string token, T state)
        {
            using var _ = await GetLockAsync();

            var map = GetTypedMap<TokenStatePair<T>>();
            if (map.TryGetValue(key, out var tsp) && tsp.Token != token) return false;

            ImmutableInterlocked.Update(
                location: ref mut_stateMap,
                transformer: m => m.SetItem(typeof(T), map.SetItem(key, new TokenStatePair<T>(token, state))));

            keySubject.OnNext(typeof(T));

            return true;
        }


        private readonly struct TokenStatePair<TState>
        {
            public TokenStatePair(string token, TState state)
            {
                Token = token;
                State = state;
            }
            public string Token { get; }
            public TState State { get; }
        }
    }
}
