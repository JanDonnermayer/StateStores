using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace StateStores.App.Blazor.Services
{
    class StateReactor : IHostedService
    {
        private readonly IStateStore stateStore;

        private IDisposable? mut_disposable;

        public StateReactor(IStateStore stateStore) =>
            this.stateStore = stateStore ?? throw new System.ArgumentNullException(nameof(stateStore));

        private IDisposable RegisterChatReactor(string channel, string trigger, TimeSpan frequency) =>
            stateStore
                .ToChannel<string>(channel)
                .OnNextWithHandle(trigger)
                .Delay(frequency)
                .Update("Jo,")
                .Delay(frequency)
                .Update("was")
                .Delay(frequency)
                .Update("geht,")
                .Delay(frequency)
                .Update("Bro?")
                .Delay(frequency)
                .Remove()
                .Subscribe();

        private IDisposable RegisterPulseBehavior(string channel, TimeSpan frequency) =>
            stateStore
                .ToChannel<string>(channel)
                .OnNextWithHandle()
                .Delay(frequency)
                .Update(_ => Guid.NewGuid().ToString())
                .Subscribe();


        private IDisposable RegisterQuackReactor(string channel, string trigger, TimeSpan frequency) =>
            stateStore
                .ToChannel<string>(channel)
                .OnNextWithHandle(trigger)
                .Update("quack!")
                .Delay(frequency)
                .Update("bye...")
                .Delay(frequency)
                .Remove()
                .Subscribe();


        #region  Implementation of IHostedService

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            mut_disposable = RegisterChatReactor(
                    channel: "Tobi",
                    trigger: "Hey",
                    frequency: TimeSpan.FromSeconds(1))
                .Append(RegisterPulseBehavior(
                    channel: "Jan",
                    frequency: TimeSpan.FromSeconds(1)))
                .Append(RegisterPulseBehavior(
                    channel: "Elisa",
                    frequency: TimeSpan.FromSeconds(1)))
                .Append(RegisterQuackReactor(
                    channel: "Duck",
                    trigger: "say",
                    frequency: TimeSpan.FromSeconds(1)));

            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            mut_disposable?.Dispose();

            return Task.CompletedTask;
        }

        #endregion
    }

}