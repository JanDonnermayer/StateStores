# StateStores

[![](https://github.com/JanDonnermayer/StateStores/workflows/Unit%20Tests/badge.svg)](
https://github.com/JanDonnermayer/StateStores/actions)

StateStores.Redis:

[![](https://img.shields.io/badge/nuget-v0.0.4-blue.svg)](
https://www.nuget.org/packages/StateStores.Redis/)

StateStores.InMemory:

[![](https://img.shields.io/badge/nuget-v0.0.4-blue.svg)](
https://www.nuget.org/packages/StateStores.InMemory/)

## Description

Provides a simple interface for observing and modifying state atomically.

## Usage

```csharp
using System.Reactive.Linq;
using StateStores.InMemory;

enum States { S1, S2 };

// ...

var channel = new InMemoryStateStore()
    .ToChannel<States>("state1");

// Reactive-transit: S1 --> S2
channel
    .OnNextToHandle(States.S1)
    .Update(States.S2)
    .Subscribe();

// Reactive-transit: S2 --> {0}
channel
    .OnNextToHandle(States.S2)
    .Remove()
    .Subscribe();

// Imperative-transit: {0} --> S1
await channel.AddAsync(States.S1);
```

## Dotnet CLI

```powershell
dotnet add package StateStores.InMemory
```
