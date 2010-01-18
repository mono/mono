using System;

namespace Mono.Debugger.Soft
{
	public sealed class MethodEntryEventRequest : EventRequest {

		internal MethodEntryEventRequest (VirtualMachine vm) : base (vm, EventType.MethodEntry) {
		}
	}
}