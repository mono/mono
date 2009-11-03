using System;

namespace Mono.Debugger
{
	public sealed class MethodExitEventRequest : EventRequest {

		internal MethodExitEventRequest (VirtualMachine vm) : base (vm, EventType.MethodExit) {
		}
	}
}