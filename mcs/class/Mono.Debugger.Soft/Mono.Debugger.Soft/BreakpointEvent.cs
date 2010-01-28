
namespace Mono.Debugger.Soft
{
	public class BreakpointEvent : Event {
		MethodMirror method;
		long id;

		internal BreakpointEvent (VirtualMachine vm, int req_id, long thread_id, long id, long loc) : base (EventType.Breakpoint, vm, req_id, thread_id) {
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
