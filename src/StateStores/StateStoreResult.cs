namespace StateStores
{
    public abstract class StateStoreResult
    {
        public static Ok Ok() => new Ok();

        public static StateError StateError() => new StateError();

        public static ConnectionError ConnectionError() => new ConnectionError();

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
