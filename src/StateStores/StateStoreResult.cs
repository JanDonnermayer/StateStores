namespace StateStores
{
    public abstract class StateStoreResult
    {
        public static Ok Ok { get; }

        public static StateError StateError { get; }

        public static ConnectionError ConnectionError { get; }

        internal StateStoreResult() { }
    }

    public abstract class ErrorResult : StateStoreResult
    {
        internal ErrorResult() { }
    }

    public sealed class Ok : StateStoreResult { }

    public sealed class StateError : ErrorResult { }

    public sealed class ConnectionError : ErrorResult { }
}
