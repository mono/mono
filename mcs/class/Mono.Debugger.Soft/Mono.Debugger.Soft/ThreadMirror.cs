using System;
using System.Threading;
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	public class ThreadMirror : ObjectMirror
	{
		string name;
		bool cacheInvalid = true;
		bool fetching;
		object fetchingLocker = new object ();
		ManualResetEvent fetchingEvent = new ManualResetEvent (false);
		ThreadInfo info;
		StackFrame[] frames;
		bool threadStateInvalid = true;
		ThreadState threadState;

		internal ThreadMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		internal ThreadMirror (VirtualMachine vm, long id, TypeMirror type, AppDomainMirror domain) : base (vm, id, type, domain) {
		}

		public StackFrame[] GetFrames () {
			FetchFrames (true);
			if (WaitHandle.WaitAny (new []{ vm.conn.DisconnectedEvent, fetchingEvent }) == 0) {
				throw new VMDisconnectedException ();
			}
			return frames;
		}

		public long ElapsedTime () {
			vm.CheckProtocolVersion (2, 50);
			long elapsedTime = GetElapsedTime ();
			return elapsedTime;
		}

		internal void InvalidateFrames () {
			cacheInvalid = true;
			threadStateInvalid = true;
		}

		internal long GetElapsedTime () {
			return vm.conn.Thread_GetElapsedTime (id);
		}

		internal void FetchFrames (bool mustFetch = false) {
			lock (fetchingLocker) {
				if (fetching || !cacheInvalid)
					return;
				cacheInvalid = false;
				fetching = true;
				fetchingEvent.Reset ();
			}
			vm.conn.Thread_GetFrameInfo (id, 0, -1, (frame_info) => {
				var framesList = new List<StackFrame> ();
				for (int i = 0; i < frame_info.Length; ++i) {
					var frameInfo = (FrameInfo)frame_info [i];
					var method = vm.GetMethod (frameInfo.method);
					var f = new StackFrame (vm, frameInfo.id, this, method, frameInfo.il_offset, frameInfo.flags);
					if (!(f.IsNativeTransition && !NativeTransitions))
						framesList.Add (f);
				}
				lock (fetchingLocker) {
					vm.AddThreadToInvalidateList (this);
					fetching = false;
					//In case it was invalidated during waiting for response from
					//runtime and mustFetch was set refetch
					if (cacheInvalid && mustFetch) {
						FetchFrames (mustFetch);
						return;
					}
					frames = framesList.ToArray ();
					fetchingEvent.Set ();
				}
			});
		}

		public static void FetchFrames(IList<ThreadMirror> threads)
		{
			if (threads.Count == 0)
				return;
			threads [0].vm.conn.StartBuffering ();
			foreach (var thread in threads) {
				thread.FetchFrames ();
			}
			threads [0].vm.conn.StopBuffering ();
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
				if (threadStateInvalid) {
					threadState = (ThreadState) vm.conn.Thread_GetState (id);
					threadStateInvalid = false;
				}
				return threadState;
			}
		}

		public bool IsThreadPoolThread {
			get {
				if (info == null)
					info = vm.conn.Thread_GetInfo (id);
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

		/*
		 * Set the location where execution will return when this thread is
		 * resumed.
		 * Throws:
		 * ArgumentException - if L doesn't refer to a location in the
		 * current method of this thread.
		 * NotSupportedException - if continuing at L is not supported
		 * for any other reason.
		 * Since protocol version 29.
		 */
		public void SetIP (Location loc) {
			if (loc == null)
				throw new ArgumentNullException ("loc");
			try {
				vm.conn.Thread_SetIP (id, loc.Method.Id, loc.ILOffset);
				if (vm.conn.Version.AtLeast(2, 52)) {
					InvalidateFrames();
					FetchFrames(true);
				}
			} catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.INVALID_ARGUMENT)
					throw new ArgumentException ("loc doesn't refer to a location in the current method of this thread.", "loc");

				throw;
			}
		}
    }
}
