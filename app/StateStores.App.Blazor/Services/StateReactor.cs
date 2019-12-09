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

        public StateReactor(IStateStore stateStore)
        {
            this.stateStore = stateStore ?? throw new System.ArgumentNullException(nameof(stateStore));
        }

        private IDisposable RegisterChatBehaviour(string stateName, string trigger, TimeSpan frequency) =>
            stateStore
                .CreateChannel<string>(stateName)
                .WithHandle(trigger)
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

        private IDisposable RegisterPulseBehaviour(string stateName, TimeSpan frequency) =>
            stateStore
                .CreateChannel<string>(stateName)
                .WithHandle()
                .Delay(frequency)
                .Update(_ => Guid.NewGuid().ToString())
                .Subscribe();


        #region  Implementation of IHostedService

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            mut_disposable = RegisterChatBehaviour("Tobi", "Hey", TimeSpan.FromSeconds(1))
                .Append(RegisterPulseBehaviour("Jan", TimeSpan.FromSeconds(1)))
                .Append(RegisterPulseBehaviour("Elisa", TimeSpan.FromSeconds(1)));
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