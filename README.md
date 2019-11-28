# StateStores

Provides a simple interface for observing and modifying state.

[![Actions Status](https://github.com/JanDonnermayer/StateStores/workflows/UnitTest/badge.svg)](https://github.com/JanDonnermayer/StateStores/actions)

## Usage

```csharp

    enum SampleStates { state1, state2 };

    // ...
     
    var proxy = new RedisStateStore(SERVER).CreateProxy<SampleStates>(KEY);

    await proxy.AddAsync(SampleStates.state1);

    await proxy.UpdateAsync(SampleStates.state1, SampleStates.state2);

    await proxy.RemoveAsync(SampleStates.state2);
```


