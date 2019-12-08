using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
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

        private IDisposable RegisterWoehrmann()
        {
            var proxy = stateStore
                .CreateProxy<string>("Woehrmann");

            Action<string> Respond(string response) =>
                message => _ = proxy.UpdateAsync(message, response);

            IEnumerable<IDisposable> _Register()
            {
                yield return proxy
                    .OnNext("geht".Equals)
                    .Delay(TimeSpan.FromSeconds(1))
                    .Do(Respond("mois?"))
                    .Subscribe();

                yield return proxy
                    .OnNext("was".Equals)
                    .Delay(TimeSpan.FromSeconds(1))
                    .Do(Respond("geht"))
                    .Subscribe();

                yield return proxy
                    .OnNext("Hey".Equals)
                    .Delay(TimeSpan.FromSeconds(1))
                    .Do(Respond("was"))
                    .Subscribe();
            }

            return new CompositeDisposable(_Register());
        }

        private IDisposable RegisterPulse(string stateName)
        {
            var proxy = stateStore
                .CreateProxy<string>(stateName);

            Action<string> Respond() =>
                message => _ = proxy.UpdateAsync(message, Guid.NewGuid().ToString());

            return proxy
                .OnNext()
                .Delay(TimeSpan.FromSeconds(1))
                .Do(Respond())
                .Subscribe();
        }


        #region  Implementation of IHostedService

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            mut_disposable = RegisterWoehrmann()
                .Append(RegisterPulse("Jan"))
                .Append(RegisterPulse("Elisa"));
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