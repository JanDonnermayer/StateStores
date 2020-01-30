namespace StateStores
{
    public abstract class StateStoreResult
    {
        public sealed class Ok : StateStoreResult { }

        public class Error : StateStoreResult { }

        public class StateError : Error { }

        public class ConnectionError : Error { }
    }

}
