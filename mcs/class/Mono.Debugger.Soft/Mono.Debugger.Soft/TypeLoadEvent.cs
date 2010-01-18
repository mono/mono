
namespace Mono.Debugger.Soft
{
	public class TypeLoadEvent : Event {
		TypeMirror type;
		long id;

		internal TypeLoadEvent (VirtualMachine vm, int req_id, long thread_id, long id) : base (EventType.TypeLoad, vm, req_id, thread_id) {
			this.id = id;
		}

		public TypeMirror Type {
			get {
				if (type == null)
					type = vm.GetType (id);
				return type;
			}
		}
	}
}
