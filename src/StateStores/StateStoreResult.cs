namespace StateStores
{
    public abstract class StateStoreResult
    {
        public sealed class Ok : StateStoreResult { }

        public sealed class Error : StateStoreResult { }
        
    }

}
