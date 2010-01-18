using System;

namespace Mono.Debugger.Soft
{
	public sealed class MethodExitEventRequest : EventRequest {

		internal MethodExitEventRequest (VirtualMachine vm) : base (vm, EventType.MethodExit) {
		}
	}
}