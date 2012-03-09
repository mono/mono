using System;
using System.Threading;
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	public class ThreadMirror : ObjectMirror
	{
		string name;

		internal ThreadMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		internal ThreadMirror (VirtualMachine vm, long id, TypeMirror type, AppDomainMirror domain) : base (vm, id, type, domain) {
		}

		// FIXME: Cache, invalidate when the thread/runtime is resumed
		public StackFrame[] GetFrames () {
			FrameInfo[] frame_info = vm.conn.Thread_GetFrameInfo (id, 0, -1);

			var frames = new List<StackFrame> ();

			for (int i = 0; i < frame_info.Length; ++i) {
				FrameInfo info = (FrameInfo)frame_info [i];
				MethodMirror method = vm.GetMethod (info.method);
				var f = new StackFrame (vm, info.id, this, method, info.il_offset, info.flags);
				if (!(f.IsNativeTransition && !NativeTransitions))
					frames.Add (f);
			}

			return frames.ToArray ();
	    }

		public string Name {
			get {
				if (name == null)
					name = vm.conn.Thread_GetName (id);
				return name;
			}
	    }

		public new long Id {
			get {
				return id;
			}
		}

		public ThreadState ThreadState {
			get {
				return (ThreadState)vm.conn.Thread_GetState (id);
			}
		}

		public bool IsThreadPoolThread {
			get {
				ThreadInfo info = vm.conn.Thread_GetInfo (id);

				return info.is_thread_pool;
			}
		}

		long? thread_id;
		/*
		 * Return a unique identifier for this thread, multiple ThreadMirror objects
		 * may have the same ThreadId because of appdomains.
		 */
		public long ThreadId {
			get {
				if (thread_id == null)
				 	thread_id = vm.conn.Thread_GetId (id);
				return (long)thread_id;
			}
		}

		/*
		 * Return the system thread id (TID) for this thread, this id is not unique since
		 * a newly started thread might reuse a dead thread's id.
		 */
		public long TID {
			get {
				return vm.conn.Thread_GetTID (id);
			}
		}

		/*
		 * Get/set whenever GetFrames () should return frames for managed-to-native
		 * transitions, i.e frames whose IsNativeTransition property is set.
		 * This is needed because some clients might not be able to deal with those
		 * frames.
		 */
		public static bool NativeTransitions {
			get; set;
		}
    }
}
