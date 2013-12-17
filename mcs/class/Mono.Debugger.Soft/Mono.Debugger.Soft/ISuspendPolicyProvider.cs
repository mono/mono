namespace Mono.Debugger.Soft
{
	public interface ISuspendPolicyProvider
	{
		SuspendPolicy ProvideFor (EventType e);
	}	
}
