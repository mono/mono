
namespace Mono.Debugger.Soft
{
	// Keep it in sync with debugger-agent.h
	public enum EventType {
		VMStart = 0,
		VMDeath = 1,
		ThreadStart = 2,
		ThreadDeath = 3,
		AppDomainCreate = 4,
		AppDomainUnload = 5,
		MethodEntry = 6,
		MethodExit = 7,
		AssemblyLoad = 8,
		AssemblyUnload = 9,
		Breakpoint = 10,
		Step = 11,
		TypeLoad = 12,
		Exception = 13,
		KeepAlive = 14,
		// Not part of the wire protocol
		VMDisconnect = 99
	}
}
