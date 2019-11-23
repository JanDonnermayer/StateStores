namespace StateStores
{
    public abstract class StateTransferResult
    {
        public sealed class OK : StateTransferResult { }

        public abstract class Error : StateTransferResult { }

        public sealed class ConnectionError : Error { }

        public sealed class TokenError : Error { }
    }

}
