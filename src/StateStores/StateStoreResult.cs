namespace StateStores
{
    public abstract class StateStoreResult
    {
        public sealed class Ok : StateStoreResult { }

        public abstract class Error : StateStoreResult { }

        public sealed class ConnectionError : Error { }

        public sealed class TokenError : Error { }

        public sealed class StateError : Error { }
        
    }

}
