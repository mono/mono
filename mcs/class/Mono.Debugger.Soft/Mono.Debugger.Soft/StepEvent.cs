
namespace Mono.Debugger.Soft
{
	public class StepEvent : Event {
		MethodMirror method;
		long id, loc;

		internal StepEvent (VirtualMachine vm, int req_id, long thread_id, long id, long loc) : base (EventType.Step, vm, req_id, thread_id) {
			this.id = id;
			this.loc = loc;
		}

		public MethodMirror Method {
			get {
				if (method == null)
					method = vm.GetMethod (id);
				return method;
			}
		}

		public long Location {
			get {
				return loc;
			}
		}
	}
}
