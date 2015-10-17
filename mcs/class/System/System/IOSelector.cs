// System/System.IOSelector.cs
//
// Authors:
//	Ludovic Henry <ludovic@xamarin.com>
//
// Copyright (C) 2015 Xamarin, Inc. (https://www.xamarin.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System
{
	/* Keep in sync with ThreadPoolIOOperation in mono/metadata/threadpool-ms-io.c */
	internal enum IOOperation
	{
		Read  = 1 << 0,
		Write = 1 << 1,
	}

	internal delegate void IOAsyncCallback (IOAsyncResult ioares);

	internal abstract class IOAsyncResult : IAsyncResult
	{
		AsyncCallback async_callback;
		object async_state;

		ManualResetEvent wait_handle;
		bool completed_synchronously;
		bool completed;

		protected IOAsyncResult ()
		{
		}

		protected void Init (AsyncCallback async_callback, object async_state)
		{
			this.async_callback = async_callback;
			this.async_state = async_state;

			completed = false;
			completed_synchronously = false;

			if (wait_handle != null)
				wait_handle.Reset ();
		}

		protected IOAsyncResult (AsyncCallback async_callback, object async_state)
		{
			this.async_callback = async_callback;
			this.async_state = async_state;
		}

		public AsyncCallback AsyncCallback
		{
			get { return async_callback; }
		}

		public object AsyncState
		{
			get { return async_state; }
		}

		public WaitHandle AsyncWaitHandle
		{
			get {
				lock (this) {
					if (wait_handle == null)
						wait_handle = new ManualResetEvent (completed);
					return wait_handle;
				}
			}
		}

		public bool CompletedSynchronously
		{
			get {
				return completed_synchronously;
			}
			protected set {
				completed_synchronously = value;
			}
		}

		public bool IsCompleted
		{
			get {
				return completed;
			}
			protected set {
				completed = value;
				lock (this) {
					if (value && wait_handle != null)
						wait_handle.Set ();
				}
			}
		}

		internal abstract void CompleteDisposed();
	}

	internal struct IOSelectorJob : IThreadPoolWorkItem
	{
		IOOperation operation;
		IOAsyncCallback callback;
		IOAsyncResult state;

		public IOOperation Operation {
			get { return operation; }
		}

		public IOSelectorJob (IOOperation operation, IOAsyncCallback callback, IOAsyncResult state)
		{
			this.operation = operation;
			this.callback = callback;
			this.state = state;
		}

		void IThreadPoolWorkItem.ExecuteWorkItem ()
		{
			this.callback (this.state);
		}

		void IThreadPoolWorkItem.MarkAborted (ThreadAbortException tae)
		{
		}

		public void MarkDisposed ()
		{
			state.CompleteDisposed ();
		}
	}

	internal static class IOSelector
	{
		static FileStream wakeup_pipes_rd;
		static FileStream wakeup_pipes_wr;

		static Thread polling_thread;

		static readonly Update[] updates = new Update [32];
		static int updates_last;

		static readonly byte[] send_buffer = new byte [1];
		static readonly byte[] recv_buffer = new byte [updates.Length];

		static readonly IntPtr invalid_handle = new IntPtr (-1);

		static IOSelector ()
		{
			MonoIOError error;
			IntPtr wakeup_pipes_rd_handle;
			IntPtr wakeup_pipes_wr_handle;
			if (!MonoIO.CreatePipe (out wakeup_pipes_rd_handle, out wakeup_pipes_wr_handle, out error))
				throw MonoIO.GetException (error);

			wakeup_pipes_rd = new FileStream (new SafeFileHandle (wakeup_pipes_rd_handle, true), FileAccess.Read);
			wakeup_pipes_wr = new FileStream (new SafeFileHandle (wakeup_pipes_wr_handle, true), FileAccess.Write);

			polling_thread = new Thread (PollingThread, Marshal.SizeOf (typeof (IntPtr)) * 32 * 1024);
			polling_thread.IsBackground = true;
			polling_thread.Start ();
		}

		public static void Add (IntPtr handle, IOSelectorJob job)
		{
			if (Environment.HasShutdownStarted)
				return;

			lock (updates) {
				while (updates_last == updates.Length)
					Monitor.Wait (updates);

				updates [updates_last].type = UpdateType.Add;
				updates [updates_last].handle = handle;
				updates [updates_last].job = job;

				updates_last += 1;

				wakeup_pipes_wr.Write (send_buffer, 0, send_buffer.Length);
			}
		}

		public static void Remove (IntPtr handle)
		{
			if (Environment.HasShutdownStarted)
				return;

			lock (updates) {
				while (updates_last == updates.Length)
					Monitor.Wait (updates);

				updates [updates_last].type = UpdateType.Remove;
				updates [updates_last].handle = handle;

				updates_last += 1;

				wakeup_pipes_wr.Write (send_buffer, 0, send_buffer.Length);

				Monitor.Wait (updates);
			}
		}

		static bool FirstJobForOperation (List<IOSelectorJob> jobs, IOOperation operation, out IOSelectorJob job)
		{
			job = default (IOSelectorJob);

			for (int i = 0; i < jobs.Count; ++i) {
				job = jobs [i];
				if ((job.Operation & operation) != 0) {
					jobs.RemoveAt (i);
					return true;
				}
			}

			return false;
		}

		static int GetOperationsForJobs (List<IOSelectorJob> jobs)
		{
			int operations = 0;
			for (int i = 0; i < jobs.Count; ++i)
				operations |= (int) jobs [i].Operation;

			return operations;
		}

		static void PollingThread ()
		{
			IntPtr backend = IntPtr.Zero;

			try {
				BackendInitialize (wakeup_pipes_rd.SafeFileHandle, out backend);

				Dictionary<IntPtr, List<IOSelectorJob>> states = new Dictionary<IntPtr, List<IOSelectorJob>> ();
				List<BackendUpdate> backend_updates = new List<BackendUpdate> ();
				BackendEvent[] backend_events = new BackendEvent [8];

				for (;;) {
					lock (updates) {
						for (int i = 0; i < updates_last; ++i) {
							Update u = updates [i];

							switch (u.type) {
							case UpdateType.Empty: {
								break;
							}
							case UpdateType.Add: {
								bool exists = states.ContainsKey (u.handle);
								if (!exists)
									states.Add (u.handle, new List<IOSelectorJob> ());

								states [u.handle].Add (u.job);

								int idx;
								if ((idx = backend_updates.FindLastIndex (bu => bu.handle == u.handle)) == -1) {
									backend_updates.Add (new BackendUpdate (u.handle, BackendUpdateType.Add) {
										operations = GetOperationsForJobs (states [u.handle]),
										is_new = !exists,
									});
								} else {
									BackendUpdate bu = backend_updates [idx];
									switch (bu.type) {
									case BackendUpdateType.Add:
										if (!exists)
											throw new InvalidOperationException (String.Format ("Cannot add handle {0} as new, while it already exists", (int) u.handle));

										backend_updates [idx] = new BackendUpdate (bu) {
											operations = GetOperationsForJobs (states [u.handle]),
										};

										break;
									case BackendUpdateType.Remove:
										if (exists)
											throw new InvalidOperationException (String.Format ("Cannot add handle {0} as existing, while it has previously been removed", (int) u.handle));

										backend_updates.Add (new BackendUpdate (u.handle, BackendUpdateType.Add) {
											operations = GetOperationsForJobs (states [u.handle]),
											is_new = true,
										});

										break;
									default:
										throw new InvalidOperationException ();
									}
								}

								break;
							}
							case UpdateType.Remove: {
								if (states.ContainsKey (u.handle)) {
									List<IOSelectorJob> jobs = states [u.handle];
									for (int j = 0; j < jobs.Count; ++j)
										jobs [j].MarkDisposed ();

									states.Remove (u.handle);

									int idx;
									if ((idx = backend_updates.FindLastIndex (bu => bu.handle == u.handle)) == -1) {
										backend_updates.Add (new BackendUpdate (u.handle, BackendUpdateType.Remove));
									} else {
										BackendUpdate bu = backend_updates [idx];
										switch (bu.type) {
										case BackendUpdateType.Add:
											backend_updates.RemoveAt (idx);
											if (!bu.is_new)
												backend_updates.Add (new BackendUpdate (u.handle, BackendUpdateType.Remove));

											break;
										case BackendUpdateType.Remove:
											throw new InvalidOperationException (String.Format ("Cannot remove handle {0} twice", (int) u.handle));
										default:
											throw new InvalidOperationException ();
										}
									}
								}

								for (int j = i + 1; j < updates_last; ++j) {
									if (updates [j].type == UpdateType.Add && updates [j].handle == u.handle) {
										updates [j].type = UpdateType.Empty;
										updates [j].handle = IntPtr.Zero;
										updates [j].job = default (IOSelectorJob);
									}
								}

								break;
							}
							default:
								throw new InvalidOperationException ();
							}
						}

						if (updates_last > 0) {
							updates_last = 0;
							Array.Clear (updates, 0, updates.Length);
						}

						for (int i = 0; i < backend_updates.Count; ++i) {
							BackendUpdate bu = backend_updates [i];
							switch (bu.type) {
							case BackendUpdateType.Add:
								BackendAddHandle (backend, bu.handle, bu.operations, bu.is_new);
								break;
							case BackendUpdateType.Remove:
								BackendRemoveHandle (backend, bu.handle);
								break;
							default:
								throw new InvalidOperationException ();
							}
						}

						backend_updates.Clear ();

						Monitor.PulseAll (updates);
					}

					if (backend_events.Length < states.Count)
						Array.Resize (ref backend_events, states.Count);

					for (int i = 0; i < backend_events.Length; ++i)
						backend_events [i].handle = invalid_handle;

					BackendPoll (backend, backend_events);

					for (int i = 0; i < backend_events.Length; ++i) {
						BackendEvent e = backend_events [i];

						if (e.handle == invalid_handle)
							continue;

						if (e.handle == wakeup_pipes_rd.Handle) {
							wakeup_pipes_rd.Read (recv_buffer, 0, recv_buffer.Length);
						} else {
							if (!states.ContainsKey (e.handle))
								throw new InvalidOperationException (String.Format ("Handle {0} not found in states dictionary", (int) e.handle));

							IOSelectorJob job;
							List<IOSelectorJob> jobs = states [e.handle];

							if (jobs.Count > 0 && (e.events & (int) BackendEventType.In) != 0) {
								if (FirstJobForOperation (jobs, IOOperation.Read, out job))
									ThreadPool.UnsafeQueueCustomWorkItem (job, false);
							}
							if (jobs.Count > 0 && (e.events & (int) BackendEventType.Out) != 0) {
								if (FirstJobForOperation (jobs, IOOperation.Write, out job))
									ThreadPool.UnsafeQueueCustomWorkItem (job, false);
							}

							if (backend_updates.Exists (bu => bu.handle == e.handle))
								throw new InvalidOperationException (String.Format ("Handle {0} returned in multiple events by BackendPoll", e.handle));

							if ((e.events & (int) BackendEventType.Error) == 0) {
								backend_updates.Add (new BackendUpdate (e.handle, BackendUpdateType.Add) {
									operations = GetOperationsForJobs (jobs),
									is_new = false,
								});
							} else {
								states.Remove (e.handle);
								backend_updates.Add (new BackendUpdate (e.handle, BackendUpdateType.Remove));
							}
						}
					}
				}
			} finally {
				if (backend != IntPtr.Zero)
					BackendCleanup (backend);
			}
		}

		static void BackendInitialize (SafeHandle wakeup_pipe, out IntPtr backend)
		{
			bool release = false;
			try {
				wakeup_pipe.DangerousAddRef (ref release);
				try {
				} finally {
					/* It shouldn't be interrupted by a ThreadAbortException, as that could leak the `backend` resource */
					backend = BackendInitialize (wakeup_pipe.DangerousGetHandle ());
				}
			} finally {
				if (release)
					wakeup_pipe.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern IntPtr BackendInitialize (IntPtr wakeup_pipes_handle);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void BackendCleanup (IntPtr backend);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void BackendAddHandle (IntPtr backend, IntPtr handle, int operations, bool is_new);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void BackendRemoveHandle (IntPtr backend, IntPtr handle);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void BackendPoll (IntPtr backend, BackendEvent[] events);

		struct Update
		{
			public UpdateType type;
			public IntPtr handle;
			public IOSelectorJob job;
		}

		enum UpdateType
		{
			Empty,
			Add,
			Remove,
		}

		struct BackendUpdate
		{
			public IntPtr handle;
			public BackendUpdateType type;
			public int operations;
			public bool is_new;

			public BackendUpdate (IntPtr handle, BackendUpdateType type)
			{
				this.handle = handle;
				this.type = type;
				this.operations = 0;
				this.is_new = false;
			}

			public BackendUpdate (BackendUpdate o)
			{
				this.handle = o.handle;
				this.type = o.type;
				this.operations = o.operations;
				this.is_new = o.is_new;
			}
		}

		enum BackendUpdateType
		{
			Add,
			Remove,
		}

		/* Keep in sync with ThreadPoolIOBackendEvent in mono/metadata/threadpool-ms-io.c */
		[StructLayout (LayoutKind.Sequential)]
		struct BackendEvent
		{
			public IntPtr handle;
			public short events;
		}

		/* Keep in sync with ThreadPoolIOBackendEventType in mono/metadata/threadpool-ms-io.c */
		enum BackendEventType
		{
			In    = 1 << 0,
			Out   = 1 << 1,
			Error = 1 << 2,
		}
	}
}
