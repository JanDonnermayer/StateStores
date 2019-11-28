# StateStores

Provides a simple interface for observing and modifying state.

## Usage

```csharp

    enum SampleStates { state1, state2 };

    // ...
     
    var proxy = new RedisStateStore(SERVER).CreateProxy<SampleStates>(KEY);

    await proxy.AddAsync(sampleStates.state1);

    await proxy.UpdateAsync(sampleStates.state1, sampleStates.state2);

    await proxy.RemoveAsync(sampleStates.state2);
```