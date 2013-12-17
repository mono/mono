namespace Mono.Debugger.Soft
{
	class DefaultSuspendPolicyProvider : ISuspendPolicyProvider
	{
		public SuspendPolicy ProvideFor (EventType e)
		{
			return SuspendPolicy.All;
		}
	}
}
