
namespace Mono.Debugger.Soft
{
	public class ThreadDeathEvent : Event {
		internal ThreadDeathEvent (VirtualMachine vm, int req_id, long id) : base (EventType.ThreadDeath, vm, req_id, id) {
		}
	}
}
