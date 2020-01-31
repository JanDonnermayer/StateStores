using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static StateStores.StateStoreResult;

namespace StateStores
{
    internal static class StateStoreResultBuilder
    {
        /// <summary>
        /// Executes the provided function within a try-catch-block.
        /// If an exception of <typeparam name="TException"/> occurs,
        /// the specified <paramref name="resultMapper"/>
        /// is used to create the result.
        /// </summary>
        public static Func<Task<StateStoreResult>> Catch<TException>(this Func<Task<StateStoreResult>> source,
            Func<TException, StateStoreResult> resultMapper)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (resultMapper is null)
                throw new ArgumentNullException(nameof(resultMapper));

            async Task<StateStoreResult> GetResultAsync()
            {
                try
                {
                    return await source().ConfigureAwait(false);
                }
                catch (Exception _) when (_ is TException ex)
                {
                    return resultMapper(ex);
                }
            }

            return GetResultAsync;
        }

        /// <summary>
        /// If the operation results in <typeparamref name="TError" />:
        /// Retries the operation within specified intervals of
        /// exponentially increasing length of [currentTryCount] * 2 ^ [baseDelay]
        /// until the result is not of type <typeparamref name="TError"/>,
        /// or the <paramref name="retryCount" /> is exhausted.
        /// </summary>
        public static Func<Task<StateStoreResult>> RetryIncrementallyOn<TError>(
            this Func<Task<StateStoreResult>> source,
            int baseDelayMs = 100,
            int retryCount = 5) where TError : ErrorResult
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            async Task<StateStoreResult> GetResultAsync()
            {
                var mut_res = await source.Invoke().ConfigureAwait(false);
                if (!(mut_res is TError)) return mut_res;

                foreach (var timeOut in Enumerable
                    .Range(0, retryCount)
                    .Select(i => 2 ^ i * baseDelayMs))
                {
                    await Task.Delay(timeOut).ConfigureAwait(false);
                    mut_res = await source.Invoke().ConfigureAwait(false);
                    if (!(mut_res is TError)) return mut_res;
                }

                return mut_res;
            }

            return GetResultAsync;
        }
    }

}
