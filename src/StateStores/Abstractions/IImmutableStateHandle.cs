using System;
using System.Reactive;

namespace StateStores
{
    public interface IImmutableStateHandle<TState>
    {
        TState State { get; }

        IObservable<Unit> Remove();

        IObservable<IImmutableStateHandle<TState>> Update(TState nextState);

        IObservable<IImmutableStateHandle<TState>> Update(Func<TState, TState> nextState);
    }

}
