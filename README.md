# StateStores

[![](https://github.com/JanDonnermayer/StateStores/workflows/UnitTests/badge.svg)](
https://github.com/JanDonnermayer/StateStores/actions)

Provides a simple interface for observing and modifying state atomically.

## Usage

```csharp
    using System.Reactive.Linq;
    using StateStores.Redis;

    enum States { S1, S2 };

    // ...

    var proxy = new RedisStateStore(SERVER).CreateProxy<States>(KEY);

    // Reactive-transit: S1 --> S2
    proxy
        .OnNext(States.S1.Equals)
        .Subscribe(state => proxy.UpdateAsync(state, States.S2));

    // Reactive-transit: S2 --> {0}
    proxy
        .OnNext(States.S2.Equals)
        .Subscribe(state => proxy.RemoveAsync(state));

    // Imperative-transit: {0} --> S1
    await proxy.AddAsync(States.S1);
```
