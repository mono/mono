
namespace Mono.Debugger.Soft
{
	public class MethodEntryEvent : Event {
		MethodMirror method;
		long id;

		internal MethodEntryEvent (VirtualMachine vm, int req_id, long thread_id, long id) : base (EventType.MethodEntry, vm, req_id, thread_id) {
			this.id = id;
		}

		public MethodMirror Method {
			get {
				if (method == null)
					method = vm.GetMethod (id);
				return method;
			}
		}
	}
}
