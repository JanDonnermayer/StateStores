# StateStores

[![](https://github.com/JanDonnermayer/StateStores/workflows/UnitTests/badge.svg)](
https://github.com/JanDonnermayer/StateStores/actions)

[![](https://img.shields.io/badge/nuget-v0.0.4-blue.svg)](
https://www.nuget.org/packages/StateStores.InMemory/)

Provides a simple interface for observing and modifying state atomically.

## Usage

```csharp
using System.Reactive.Linq;
using StateStores.InMemory;

enum States { S1, S2 };

// ...

var channel = new InMemoryStateStore()
    .CreateChannel<States>("state1");

// Reactive-transit: S1 --> S2
channel
    .OnNextWithHandle(States.S1)
    .Update(States.S2)
    .Subscribe();

// Reactive-transit: S2 --> {0}
channel
    .OnNextWithHandle(States.S2)
    .Remove()
    .Subscribe();

// Imperative-transit: {0} --> S1
await channel.AddAsync(States.S1);
```

## Dotnet CLI

```powershell
dotnet add package StateStores.InMemory
```
