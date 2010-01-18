
namespace Mono.Debugger.Soft
{
	public class AppDomainUnloadEvent : Event {
		AppDomainMirror domain;
		long id;

		internal AppDomainUnloadEvent (VirtualMachine vm, int req_id, long thread_id, long id) : base (EventType.AppDomainUnload, vm, req_id, thread_id) {
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
