namespace Datadog.Trace
{
    internal class AsyncLocalScopeManager : ScopeManagerBase
    {
        private readonly AsyncLocalCompat<IScope> _activeScope = new AsyncLocalCompat<IScope>();

        public override IScope Active
        {
            get
            {
                return _activeScope.Get();
            }

            protected set
            {
                _activeScope.Set(value);
            }
        }
    }
}
