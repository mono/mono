// TODO:
//    DispatcherObject returned by BeginInvoke must allow:
//       * Waiting until delegate is invoked.
//       See: BeginInvoke documentation for details
//
//    Implement the "Invoke" methods, they are currently not working.
//
//    Add support for disabling the dispatcher and resuming it.
//    Add support for Waiting for new tasks to be pushed, so that we dont busy loop.
//    Add support for aborting an operation (emit the hook.operationaborted too) 
//
// Very confusing information about Shutdown: it states that shutdown is
// not over, until all events are unwinded, and also states that all events
// are aborted at that point.  See 'Dispatcher.InvokeShutdown' docs,
//
// Testing reveals that 
//     -> InvokeShutdown() stops processing, even if events are available,
//        there is no "unwinding" of events, even of higher priority events,
//        they are just ignored.
//
// The documentation for the Dispatcher family is poorly written, complete
// sections are cut-and-pasted that add no value and the important pieces
// like (what is a frame) is not on the APIs, but scattered everywhere else
//
// -----------------------------------------------------------------------
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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Miguel de Icaza (miguel@novell.com)
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using System.Threading;

namespace System.Windows.Threading {

	[Flags]
	internal enum Flags {
		ShutdownStarted = 1,
		Shutdown = 2,
		Disabled = 4
	}
	
	public sealed class Dispatcher {
		static Dictionary<Thread, Dispatcher> dispatchers = new Dictionary<Thread, Dispatcher> ();
		static object olock = new object ();
		static DispatcherFrame main_execution_frame = new DispatcherFrame ();
		
		const int TOP_PRIO = (int)DispatcherPriority.Send;
		Thread base_thread;
		PokableQueue [] priority_queues = new PokableQueue [TOP_PRIO+1];

		Flags flags;
		int queue_bits;

		//
		// Used to notify the dispatcher thread that new data is available
		//
		EventWaitHandle wait;

		//
		// The hooks for this Dispatcher
		//
		DispatcherHooks hooks;

		//
		// The current DispatcherFrame active in a given Dispatcher, we use this to
		// keep a linked list of all active frames, so we can "ExitAll" frames when
		// requested
		DispatcherFrame current_frame;
		
		Dispatcher (Thread t)
		{
			base_thread = t;
			for (int i = 1; i <= (int) DispatcherPriority.Send; i++)
				priority_queues [i] = new PokableQueue ();
			wait = new EventWaitHandle (false, EventResetMode.AutoReset);
			hooks = new DispatcherHooks (this);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool CheckAccess ()
		{
			return Thread.CurrentThread == base_thread;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void VerifyAccess ()
		{
			if (Thread.CurrentThread != base_thread)
				throw new InvalidOperationException ("Invoked from a different thread");
		}

		public static void ValidatePriority (DispatcherPriority priority, string parameterName)
		{
			if (priority < DispatcherPriority.Inactive || priority > DispatcherPriority.Send)
				throw new InvalidEnumArgumentException (parameterName);
		}

		public DispatcherOperation BeginInvoke (Delegate method, params object[] args)
		{
			throw new NotImplementedException ();
		}
		
		public DispatcherOperation BeginInvoke (Delegate method, DispatcherPriority priority, params object[] args)
		{
			throw new NotImplementedException ();
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public DispatcherOperation BeginInvoke (DispatcherPriority priority, Delegate method)
		{
			if (priority < 0 || priority > DispatcherPriority.Send)
				throw new InvalidEnumArgumentException ("priority");
			if (priority == DispatcherPriority.Inactive)
				throw new ArgumentException ("priority can not be inactive", "priority");
			if (method == null)
				throw new ArgumentNullException ("method");

			DispatcherOperation op = new DispatcherOperation (this, priority, method);
			Queue (priority, op);

			return op;
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public DispatcherOperation BeginInvoke (DispatcherPriority priority, Delegate method, object arg)
		{
			if (priority < 0 || priority > DispatcherPriority.Send)
				throw new InvalidEnumArgumentException ("priority");
			if (priority == DispatcherPriority.Inactive)
				throw new ArgumentException ("priority can not be inactive", "priority");
			if (method == null)
				throw new ArgumentNullException ("method");

			DispatcherOperation op = new DispatcherOperation (this, priority, method, arg);
			
			Queue (priority, op);
			
			return op;
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public DispatcherOperation BeginInvoke (DispatcherPriority priority, Delegate method, object arg, params object [] args)
		{
			if (priority < 0 || priority > DispatcherPriority.Send)
				throw new InvalidEnumArgumentException ("priority");
			if (priority == DispatcherPriority.Inactive)
				throw new ArgumentException ("priority can not be inactive", "priority");
			if (method == null)
				throw new ArgumentNullException ("method");

			DispatcherOperation op = new DispatcherOperation (this, priority, method, arg, args);
			Queue (priority, op);

			return op;
		}

		public object Invoke (Delegate method, params object[] args)
		{
			throw new NotImplementedException ();
		}

		public object Invoke (Delegate method, TimeSpan timeout, params object[] args)
		{
			throw new NotImplementedException ();
		}

		public object Invoke (Delegate method, TimeSpan timeout, DispatcherPriority priority, params object[] args)
		{
			throw new NotImplementedException ();
		}

		public object Invoke (Delegate method, DispatcherPriority priority, params object[] args)
		{
			throw new NotImplementedException ();
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public object Invoke (DispatcherPriority priority, Delegate method)
		{
			if (priority < 0 || priority > DispatcherPriority.Send)
				throw new InvalidEnumArgumentException ("priority");
			if (priority == DispatcherPriority.Inactive)
				throw new ArgumentException ("priority can not be inactive", "priority");
			if (method == null)
				throw new ArgumentNullException ("method");

			DispatcherOperation op = new DispatcherOperation (this, priority, method);
			Queue (priority, op);
			PushFrame (new DispatcherFrame ());
			
			throw new NotImplementedException ();
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public object Invoke (DispatcherPriority priority, Delegate method, object arg)
		{
			if (priority < 0 || priority > DispatcherPriority.Send)
				throw new InvalidEnumArgumentException ("priority");
			if (priority == DispatcherPriority.Inactive)
				throw new ArgumentException ("priority can not be inactive", "priority");
			if (method == null)
				throw new ArgumentNullException ("method");

			Queue (priority, new DispatcherOperation (this, priority, method, arg));
			throw new NotImplementedException ();
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public object Invoke (DispatcherPriority priority, Delegate method, object arg, params object [] args)
		{
			if (priority < 0 || priority > DispatcherPriority.Send)
				throw new InvalidEnumArgumentException ("priority");
			if (priority == DispatcherPriority.Inactive)
				throw new ArgumentException ("priority can not be inactive", "priority");
			if (method == null)
				throw new ArgumentNullException ("method");

			Queue (priority, new DispatcherOperation (this, priority, method, arg, args));

			throw new NotImplementedException ();
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public object Invoke (DispatcherPriority priority, TimeSpan timeout, Delegate method)
		{
			throw new NotImplementedException ();
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public object Invoke (DispatcherPriority priority, TimeSpan timeout, Delegate method, object arg)
		{
			throw new NotImplementedException ();
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public object Invoke (DispatcherPriority priority, TimeSpan timeout, Delegate method, object arg, params object [] args)
		{
			throw new NotImplementedException ();
		}

		void Queue (DispatcherPriority priority, DispatcherOperation x)
		{
			int p = ((int) priority);
			PokableQueue q = priority_queues [p];
			
			lock (q){
				int flag = 1 << p;
				q.Enqueue (x);
				queue_bits |= flag;
			}
			hooks.EmitOperationPosted (x);

			if (Thread.CurrentThread != base_thread)
				wait.Set ();
		}

		internal void Reprioritize (DispatcherOperation op, DispatcherPriority oldpriority)
		{
			int oldp = (int) oldpriority;
			PokableQueue q = priority_queues [oldp];

			lock (q){
				q.Remove (op);
			}
			Queue (op.Priority, op);
			hooks.EmitOperationPriorityChanged (op);
		}
		
		public static Dispatcher CurrentDispatcher {
			get {
				lock (olock){
					Thread t = Thread.CurrentThread;
					Dispatcher dis = FromThread (t);

					if (dis != null)
						return dis;
				
					dis = new Dispatcher (t);
					dispatchers [t] = dis;
					return dis;
				}
			}
		}

		public static Dispatcher FromThread (Thread thread)
		{
			Dispatcher dis;
			
			if (dispatchers.TryGetValue (thread, out dis))
				return dis;

			return null;
		}

		public Thread Thread {
			get {
				return base_thread;
			}
		}

		[SecurityCritical]
		public static void Run ()
		{
			PushFrame (main_execution_frame);
		}
		
		[SecurityCritical]
		public static void PushFrame (DispatcherFrame frame)
		{
			if (frame == null)
				throw new ArgumentNullException ("frame");

			Dispatcher dis = CurrentDispatcher;

			if (dis.HasShutdownFinished)
				throw new InvalidOperationException ("The Dispatcher has shut down");
			if (frame.dispatcher != null)
				throw new InvalidOperationException ("Frame is already running on a different dispatcher");
			if ((dis.flags & Flags.Disabled) != 0)
				throw new InvalidOperationException ("Dispatcher processing has been disabled");

			frame.ParentFrame = dis.current_frame;
			dis.current_frame = frame;
			
			frame.dispatcher = dis;

			dis.RunFrame (frame);
		}

		void PerformShutdown ()
		{
			EventHandler h;
			
			h = ShutdownStarted;
			if (h != null)
				h (this, new EventArgs ());
			
			flags |= Flags.Shutdown;
			
			h = ShutdownFinished;
			if (h != null)
				h (this, new EventArgs ());

			priority_queues = null;
			wait = null;
		}
		
		void RunFrame (DispatcherFrame frame)
		{
			do {
				while (queue_bits != 0){
					for (int i = TOP_PRIO; i > 0 && queue_bits != 0; i--){
						int current_bit = queue_bits & (1 << i);
						if (current_bit != 0){
							PokableQueue q = priority_queues [i];

							do {
								DispatcherOperation task;
								
								lock (q){
									task = (DispatcherOperation) q.Dequeue ();
								}
								
								task.Invoke ();

								//
								// call hooks.
								//
								if (task.Status == DispatcherOperationStatus.Aborted)
									hooks.EmitOperationAborted (task);
								else
									hooks.EmitOperationCompleted (task);

								if (!frame.Continue)
									return;
								
								if (HasShutdownStarted){
									PerformShutdown ();
									return;
								}
								
								// if we are done with this queue, leave.
								lock (q){
									if (q.Count == 0){
										queue_bits &= ~(1 << i);
										break;
									}
								}

								//
								// If a higher-priority task comes in, go do that
								//
								if (current_bit < (queue_bits & ~current_bit))
									break;
							} while (true);
						}
					}
				}
				hooks.EmitInactive ();
				
				wait.WaitOne ();
				wait.Reset ();
			} while (frame.Continue);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DispatcherHooks Hooks {
			[SecurityCritical]
			get { throw new NotImplementedException (); }
		}

		public bool HasShutdownStarted {
			get {
				return (flags & Flags.ShutdownStarted) != 0;
			}
		}

		public bool HasShutdownFinished {
			get {
				return (flags & Flags.Shutdown) != 0;
			}
		}

		//
		// Do no work here, so that any events are thrown on the owning thread
		//
		[SecurityCritical]
		public void InvokeShutdown ()
		{
			flags |= Flags.ShutdownStarted;
		}

		[SecurityCritical]
		public void BeginInvokeShutdown (DispatcherPriority priority)
		{
			throw new NotImplementedException ();
		}

		[SecurityCritical]
		public static void ExitAllFrames ()
		{
			Dispatcher dis = CurrentDispatcher;
			
			for (DispatcherFrame frame = dis.current_frame; frame != null; frame = frame.ParentFrame){
				if (frame.exit_on_request)
					frame.Continue = false;
				else {
					//
					// Stop unwinding the frames at the first frame that is
					// long running
					break;
				}
			}
		}

		public DispatcherProcessingDisabled DisableProcessing ()
		{
			throw new NotImplementedException ();
		}

		public event EventHandler ShutdownStarted;
		public event EventHandler ShutdownFinished;
		public event DispatcherUnhandledExceptionEventHandler UnhandledException;
		public event DispatcherUnhandledExceptionFilterEventHandler UnhandledExceptionFilter;
	}

	internal class PokableQueue {
		const int initial_capacity = 32;

		int size, head, tail;
		object [] array;

		internal PokableQueue (int capacity) 
		{
			array = new object [capacity];
		}

		internal PokableQueue () : this (initial_capacity)
		{
		}

		public void Enqueue (object obj)
		{
                        if (size == array.Length)
                                Grow ();
                        array[tail] = obj;
                        tail = (tail+1) % array.Length;
                        size++;
		}

		public object Dequeue ()
                {
                        if (size < 1)
                                throw new InvalidOperationException ();
                        object result = array[head];
                        array [head] = null;
                        head = (head + 1) % array.Length;
                        size--;
                        return result;
                }

		void Grow () {
                        int newc = array.Length * 2;
                        object[] new_contents = new object[newc];
                        array.CopyTo (new_contents, 0);
                        array = new_contents;
                        head = 0;
                        tail = head + size;
                }

		public int Count {
			get {
				return size;
			}
		}

		public void Remove (object obj)
		{
			for (int i = 0; i < size; i++){
				if (array [(head+i) % array.Length] == obj){
					for (int j = i; j < size-i; j++)
						array [(head +j) % array.Length] = array [(head+j+1) % array.Length];
					size--;
					if (size < 0)
						size = array.Length-1;
					tail--;
				}
			}
		}
	}
}
