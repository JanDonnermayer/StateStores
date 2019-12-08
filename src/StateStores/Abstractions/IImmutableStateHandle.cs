using System;
using System.Reactive;

namespace StateStores
{
    public interface IImmutableStateHandle<TState>
    {
        TState Value { get; }

        IObservable<Unit> RemoveAsync();
        IObservable<IImmutableStateHandle<TState>> UpdateAsync(TState nextState);
    }

}
