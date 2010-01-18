using System;

namespace Mono.Debugger.Soft
{
	public class VMStartEvent : Event
	{
		public VMStartEvent (VirtualMachine vm, int req_id, long thread_id) : base (EventType.VMStart, vm, req_id, thread_id) {
		}
    }
}
