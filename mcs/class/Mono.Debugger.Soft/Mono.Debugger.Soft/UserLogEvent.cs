
namespace Mono.Debugger.Soft
{
	public class UserLogEvent : Event {

		int level;
		string category, message;

		internal UserLogEvent (VirtualMachine vm, int req_id, long thread_id, int level, string category, string message) : base (EventType.UserLog, vm, req_id, thread_id) {
			this.level = level;
			this.category = category;
			this.message = message;
		}

		public int Level {
			get {
				return level;
			}
		}

		public string Category {
			get {
				return category;
			}
		}

		public string Message {
			get {
				return message;
			}
		}
	}
}
