using System;
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	public enum StepDepth {
		Into = 0,
		Over = 1,
		Out = 2
	}

	public enum StepSize {
		Min = 0,
		Line = 1
	}

	/*
	 * Filter which kinds of methods to skip during single stepping
	 */
	[Flags]
	public enum StepFilter {
		None = 0,
		StaticCtor = 1,
		/* Since protocol version 2.20 */
		/* Methods which have the [DebuggerHidden] attribute */
		DebuggerHidden = 2,
	}

	public sealed class StepEventRequest : EventRequest {

		ThreadMirror step_thread;
		StepDepth depth;
		StepSize size;
		StepFilter filter;
		
		internal StepEventRequest (VirtualMachine vm, ThreadMirror thread) : base (vm, EventType.Step) {
			if (thread == null)
				throw new ArgumentNullException ("thread");
			CheckMirror (vm, thread);
			this.step_thread = thread;
			Depth = StepDepth.Into;
			Size = StepSize.Min;
		}

		public override void Enable () {
			var mods = new List <Modifier> ();
			mods.Add (new StepModifier () { Thread = step_thread.Id, Depth = (int)Depth, Size = (int)Size, Filter = (int)Filter });
			SendReq (mods);
		}

		public new ThreadMirror Thread {
			get {
				return step_thread;
			}
		}

		public StepDepth Depth {
			get {
				return depth;
			}
			set {
				CheckDisabled ();
				depth = value;
			}
		}

		public StepSize Size {
			get {
				return size;
			}
			set {
				CheckDisabled ();
				size = value;
			}
		}

		public StepFilter Filter {
			get {
				return filter;
			}
			set {
				CheckDisabled ();
				filter = value;
			}
		}
	}
}
