using System;

namespace Mono.Debugger.Soft
{
	public class VMDeathEvent : Event
	{
		int exit_code;

		public VMDeathEvent (VirtualMachine vm, int req_id, int exit_code) : base (EventType.VMDeath, vm, req_id, -1) {
			this.exit_code = exit_code;
		}

		// Since protocol version 2.27
		public int ExitCode {
			get {
				vm.CheckProtocolVersion (2, 27);
				return exit_code;
			}
		}
    }
}
