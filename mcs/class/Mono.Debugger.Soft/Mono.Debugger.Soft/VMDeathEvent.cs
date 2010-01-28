using System;

namespace Mono.Debugger.Soft
{
	public class VMDeathEvent : Event
	{
		public VMDeathEvent (VirtualMachine vm, int req_id) : base (EventType.VMDeath, vm, req_id, -1) {
		}
    }
}
