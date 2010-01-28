
namespace Mono.Debugger.Soft
{
	public abstract class Event {
		protected VirtualMachine vm;
		EventType evtype;
		ThreadMirror thread;
		int req_id;
		long thread_id;

		internal Event (EventType evtype, VirtualMachine vm, int req_id, long thread_id) {
			this.evtype = evtype;
			this.vm = vm;
			this.req_id = req_id;
			this.thread_id = thread_id;
		}

		internal Event (EventType evtype, VirtualMachine vm) {
			this.evtype = evtype;
			this.vm = vm;
			this.thread_id = -1;
		}

		public EventType EventType {
			get {
				return evtype;
			}
		}

		public override string ToString () {
			return evtype.ToString ();
		}

		public ThreadMirror Thread {
			get {
				if (thread_id == -1)
					return null;
				if (thread == null)
					thread = vm.GetThread (thread_id);
				return thread;
			}
	    }

		public EventRequest Request {
			get {
				return vm.GetRequest (req_id);
			}
		}
	}
}
