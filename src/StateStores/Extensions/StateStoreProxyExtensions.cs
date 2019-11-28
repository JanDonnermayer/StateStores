using System;
using System.Reactive.Linq;
using System.Linq;

namespace StateStores
{
    public static class StateStoreProxyExtensions
    {        
        public static IObservable<TState> OnEntry<TState>(this IStateStoreProxy<TState> proxy)
        {
            if (proxy is null) throw new ArgumentNullException(nameof(proxy));
            return proxy.OnAdd.Merge(proxy.OnUpdate.Select(_ => _.currentState));
        }

        public static IObservable<TState> OnNext<TState>(this IStateStoreProxy<TState> proxy,
            Func<TState, bool> condition)
        {
            if (proxy is null) throw new ArgumentNullException(nameof(proxy));
            return proxy.OnEntry().Where(condition);
        }

        public static IObservable<TState> OnPrevious<TState>(this IStateStoreProxy<TState> proxy)
        {
            if (proxy is null) throw new ArgumentNullException(nameof(proxy));
            return proxy.OnRemove.Merge(proxy.OnUpdate.Select(_ => _.previousState));
        }

        public static IObservable<TState> OnPrevious<TState>(this IStateStoreProxy<TState> proxy,
            Func<TState, bool> condition)
        {
            if (proxy is null) throw new ArgumentNullException(nameof(proxy));
            return proxy.OnPrevious().Where(condition);
        }
    }
}
