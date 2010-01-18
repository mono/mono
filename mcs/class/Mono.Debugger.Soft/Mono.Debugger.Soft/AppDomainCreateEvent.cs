
namespace Mono.Debugger.Soft
{
	public class AppDomainCreateEvent : Event {
		AppDomainMirror domain;
		long id;

		internal AppDomainCreateEvent (VirtualMachine vm, int req_id, long thread_id, long id) : base (EventType.AppDomainCreate, vm, req_id, thread_id) {
			this.id = id;
		}

		public AppDomainMirror Domain {
			get {
				if (domain == null)
					domain = vm.GetDomain (id);
				return domain;
			}
		}
	}
}
