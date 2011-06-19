
namespace Mono.Debugger.Soft
{
	public class UserBreakEvent : Event {
		internal UserBreakEvent (VirtualMachine vm, int req_id, long thread_id) : base (EventType.UserBreak, vm, req_id, thread_id) {
		}
	}
}
