
namespace Mono.Debugger.Soft
{
	public class MethodExitEvent : Event {
		MethodMirror method;
		long id;

		internal MethodExitEvent (VirtualMachine vm, int req_id, long thread_id, long id) : base (EventType.MethodExit, vm, req_id, thread_id) {
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
