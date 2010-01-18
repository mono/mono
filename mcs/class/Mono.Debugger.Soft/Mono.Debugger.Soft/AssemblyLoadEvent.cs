
namespace Mono.Debugger.Soft
{
	public class AssemblyLoadEvent : Event {
		AssemblyMirror assembly;
		long id;

		internal AssemblyLoadEvent (VirtualMachine vm, int req_id, long thread_id, long id) : base (EventType.AssemblyLoad, vm, req_id, thread_id) {
			this.id = id;
		}

		public AssemblyMirror Assembly {
			get {
				if (assembly == null)
					assembly = vm.GetAssembly (id);
				return assembly;
			}
		}
	}
}
