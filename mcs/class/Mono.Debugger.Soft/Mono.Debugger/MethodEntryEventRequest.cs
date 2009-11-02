using System;

namespace Mono.Debugger
{
	public sealed class MethodEntryEventRequest : EventRequest {

		internal MethodEntryEventRequest (VirtualMachine vm) : base (vm, EventType.MethodEntry) {
		}
	}
}