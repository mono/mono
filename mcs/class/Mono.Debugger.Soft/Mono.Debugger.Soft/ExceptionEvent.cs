
namespace Mono.Debugger.Soft
{
	public class ExceptionEvent : Event {
		ObjectMirror exc;
		long exc_id;

		internal ExceptionEvent (VirtualMachine vm, int req_id, long thread_id, long exc_id, long loc) : base (EventType.Exception, vm, req_id, thread_id) {
			this.exc_id = exc_id;
		}

		public ObjectMirror Exception {
			get {
				if (exc == null)
					exc = vm.GetObject (exc_id);
				return exc;
			}
		}
	}
}
