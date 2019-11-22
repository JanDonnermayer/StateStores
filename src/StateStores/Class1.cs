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
    interface IStateStore
    {
        Task<bool> TrySetAsync<T>(string key, string token, T state);

        Task<bool> TryRemoveAsync<T>(string key, string token);

        IObservable<IEnumerable<KeyValuePair<string, T>>> GetObservable<T>();
    }



    public class InMemoryStateStore : IStateStore
    {
        private ImmutableDictionary<Type, dynamic> mut_stateMap =
            ImmutableDictionary<Type, dynamic>.Empty;
        private readonly ReplaySubject<Type> keySubject =
            new ReplaySubject<Type>(TaskPoolScheduler.Default);
        private readonly SemaphoreSlim semaphore =
            new SemaphoreSlim(0, 1);


        private async Task<IDisposable> GetLockAsync()
        {
            await semaphore.WaitAsync();
            return Disposable.Create(() => semaphore.Release());
        }

        private ImmutableDictionary<string, TValue> GetTypedMap<TValue>() =>
            ImmutableInterlocked.GetOrAdd(
                ref mut_stateMap,
                typeof(TValue),
                _ => ImmutableDictionary<string, TValue>.Empty);

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


        private class TokenStatePair<TState>
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
