
namespace Mono.Debugger.Soft
{
	public class ThreadStartEvent : Event {
		internal ThreadStartEvent (VirtualMachine vm, int req_id, long id) : base (EventType.ThreadStart, vm, req_id, id) {
		}
	}
}
