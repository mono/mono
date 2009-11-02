using System;

namespace Mono.Debugger
{
	public class VMDisconnectEvent : Event
	{
		public VMDisconnectEvent (VirtualMachine vm, int req_id) : base (EventType.VMDisconnect, vm, req_id, -1) {
		}
    }
}
