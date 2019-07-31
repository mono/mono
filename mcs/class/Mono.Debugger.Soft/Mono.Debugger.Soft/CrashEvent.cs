namespace Mono.Debugger.Soft
{
	public class CrashEvent : Event {

		ulong hash;
		string dump;

		internal CrashEvent (VirtualMachine vm, int req_id, long thread_id, string dump, ulong hash) : base (EventType.Crash, vm, req_id, thread_id) {
			this.dump = dump;
			this.hash = hash;
		}

		public ulong Hash {
			get {
				return hash;
			}
		}

		public string Dump {
			get {
				return dump;
			}
		}
	}
}
