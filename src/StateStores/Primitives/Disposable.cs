using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace System
{
    public static class Disposable
    {
        public static IDisposable Append(this IDisposable source, IDisposable second) =>
            new CompositeDisposable(source, second);

        public static IDisposable Append(this IDisposable source, Action second) =>
            new CompositeDisposable(source, Create(second));

        public static IDisposable Create(Action disposeAction) =>
            Reactive.Disposables.Disposable.Create(disposeAction);
    }
}
