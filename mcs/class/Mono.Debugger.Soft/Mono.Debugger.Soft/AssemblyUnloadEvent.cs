
namespace Mono.Debugger.Soft
{
	public class AssemblyUnloadEvent : Event {
		AssemblyMirror assembly;
		long id;

		internal AssemblyUnloadEvent (VirtualMachine vm, int req_id, long thread_id, long id) : base (EventType.AssemblyUnload, vm, req_id, thread_id) {
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
