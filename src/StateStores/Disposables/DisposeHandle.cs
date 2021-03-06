using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System
{
    /// <summary>
    /// Provides helper methods for <see cref="IDisposable"/> implementations.
    /// </summary>
    public sealed class DisposeHandle<TOwner> : IDisposable
        where TOwner : class
    {
        private int _disposed;
        private readonly Stack<IDisposable> _disposeStack;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposeHandle{T}"/> class.
        /// </summary>
        public DisposeHandle(params IDisposable[] disposables)
        {
            _disposeStack = new Stack<IDisposable>(disposables ?? Array.Empty<IDisposable>());
            CancellationTokenSource CTS = new CancellationTokenSource();
            this.CancellationToken = CTS.Token;
#pragma warning disable CA2000 // Instance is disposed in routine
            _disposeStack.Push(Disposable.Create(() => CTS.Cancel()));
#pragma warning restore
        }

        /// <summary>
        /// Whether this instance is disposed.
        /// </summary>
        public bool IsDisposed =>
            _disposed == 1;

        /// <summary>
        /// Returns a token that is cancelled, as soon as this instance is disposed.
        /// </summary>
        /// <returns></returns>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Appends an instance of <see cref="IDisposable"/> to the dispose routine.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns> 
        /// The current instance for chaining.
        /// </returns>
        public DisposeHandle<TOwner> Append(IDisposable instance)
        {
            this.Test();
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _disposeStack.Push(instance);
            return this;
        }

        /// <summary>
        /// Appends an action to the dispose routine.
        /// </summary>
        /// <param name="action"></param>
        /// <returns> 
        /// The current instance for chaining.
        /// </returns>
        public DisposeHandle<TOwner> Append(Action action)
        {
            this.Test();
            if (action == null) throw new ArgumentNullException(nameof(action));
#pragma warning disable CA2000 // Instance is disposed in routine
            _disposeStack.Push(Disposable.Create(action));
#pragma warning restore
            return this;
        }

        /// <summary>
        /// If disposed, throws an <see cref="ObjectDisposedException"/>
        /// </summary>
        public void Test()
        {
            if (_disposed == 1)
                throw new ObjectDisposedException(typeof(TOwner).Name);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            if (System.Threading.Interlocked.Exchange(ref _disposed, 1) == 1) return;
            while (_disposeStack.Count > 0)
                _disposeStack.Pop().Dispose();
        }
    }
}
