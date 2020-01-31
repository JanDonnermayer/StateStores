namespace StateStores
{
    /// <summary>
    /// Represents the result of an operation.
    /// </summary>
    public abstract class StateStoreResult
    {
        internal static Ok Ok() => new Ok();

        internal static StateError StateError() => new StateError();

        internal static ConnectionError ConnectionError() => new ConnectionError();

        internal StateStoreResult() { }
    }

    /// <summary>
    /// Represents the result of a failed operation.
    /// </summary>
    public abstract class ErrorResult : StateStoreResult
    {
        internal ErrorResult() { }
    }

    /// <summary>
    /// Represents the result of a successfull operation.
    /// </summary>
    public sealed class Ok : StateStoreResult { }

    /// <summary>
    /// Represents the result of a failed operation,
    /// due to incorrect state.
    /// </summary>
    public sealed class StateError : ErrorResult { }

    /// <summary>
    /// Represents the result of a failed operation,
    /// due to connection issues.
    /// </summary>
    public sealed class ConnectionError : ErrorResult { }
}
