using System;

namespace Mono.Debugger.Soft
{
	public class EventSet {
		protected VirtualMachine vm;
		SuspendPolicy suspend_policy;
		Event[] events;

		internal EventSet (VirtualMachine vm, SuspendPolicy suspend_policy, Event[] events) {
			this.vm = vm;
			this.suspend_policy = suspend_policy;
			this.events = events;
		}

		public SuspendPolicy SuspendPolicy {
			get {
				return suspend_policy;
			}
		}

		public Event[] Events {
			get {
				return events;
			}
		}

		public Event this [int index] {
			get {
				return Events [index];
			}
		}
	}
}
