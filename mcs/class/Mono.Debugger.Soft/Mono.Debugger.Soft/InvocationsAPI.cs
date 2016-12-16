using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mono.Debugger.Soft
{
	/// <summary>
	/// A bunch of extension methods to <see cref="IInvocableMethodOwnerMirror"/> to perform invocations on objects
	/// </summary>
	public static class InvocationsAPI
	{
		public static Value InvokeMethod (this IInvocableMethodOwnerMirror mirror, ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options = InvokeOptions.None) {
			return InvokeMethod (mirror, mirror.VirtualMachine, thread, method, mirror.GetThisObject (), arguments, options);
		}

		[Obsolete ("Use the overload without the 'vm' argument")]
		public static IAsyncResult BeginInvokeMethod (this IInvocableMethodOwnerMirror mirror, VirtualMachine vm, ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			return BeginInvokeMethod (vm, thread, method, mirror.GetThisObject (), arguments, options, callback, state);
		}

		public static IAsyncResult BeginInvokeMethod (this IInvocableMethodOwnerMirror mirror, ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			return BeginInvokeMethod (mirror.VirtualMachine, thread, method, mirror.GetThisObject (), arguments, options, callback, state);
		}

		public static Value EndInvokeMethod (this IInvocableMethodOwnerMirror mirror, IAsyncResult asyncResult) {
			return EndInvokeMethodInternal (mirror, asyncResult);
		}

		public static InvokeResult EndInvokeMethodWithResult (this IInvocableMethodOwnerMirror mirror, IAsyncResult asyncResult) {
			return  EndInvokeMethodInternalWithResult (mirror, asyncResult);
		}

		public static Task<Value> InvokeMethodAsync (this IInvocableMethodOwnerMirror mirror, ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options = InvokeOptions.None) {
			var tcs = new TaskCompletionSource<Value> ();
			BeginInvokeMethod (mirror, thread, method, arguments, options, iar =>
			{
				try {
					tcs.SetResult (EndInvokeMethod (mirror, iar));
				} catch (OperationCanceledException) {
					tcs.TrySetCanceled ();
				} catch (Exception ex) {
					tcs.TrySetException (ex);
				}
			}, null);
			return tcs.Task;
		}

		public static Task<InvokeResult> InvokeMethodAsyncWithResult (this IInvocableMethodOwnerMirror mirror, ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options = InvokeOptions.None) {
			var tcs = new TaskCompletionSource<InvokeResult> ();
			BeginInvokeMethod (mirror, thread, method, arguments, options, iar =>
			{
				try {
					tcs.SetResult (EndInvokeMethodInternalWithResult (mirror, iar));
				} catch (OperationCanceledException) {
					tcs.TrySetCanceled ();
				} catch (Exception ex) {
					tcs.TrySetException (ex);
				}
			}, null);
			return tcs.Task;
		}

		/// <summary>
		/// Invoke the members of METHODS one-by-one, calling CALLBACK after each invoke was finished. The IAsyncResult will be marked as completed after all invokes have
		/// finished. The callback will be called with a different IAsyncResult that represents one method invocation.
		/// From protocol version 2.22.
		/// </summary>
		public static IAsyncResult BeginInvokeMultiple (this IInvocableMethodOwnerMirror mirror, ThreadMirror thread, MethodMirror[] methods, IList<IList<Value>> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			return BeginInvokeMultiple (mirror.VirtualMachine, thread, methods, mirror.GetThisObject (), arguments, options, callback, state);
		}

		public static void EndInvokeMultiple (IAsyncResult asyncResult) {
			EndInvokeMultipleInternal (asyncResult);
		}

		/*
		 * Common implementation for invokes
		 */

		class InvokeAsyncResult : IInvokeAsyncResult {

			public object AsyncState {
				get; set;
			}

			public WaitHandle AsyncWaitHandle {
				get; set;
			}

			public bool CompletedSynchronously {
				get {
					return false;
				}
			}

			public bool IsCompleted {
				get; set;
			}

			public AsyncCallback Callback {
				get; set;
			}

			public ErrorCode ErrorCode {
				get; set;
			}

			public VirtualMachine VM {
				get; set;
			}

			public ThreadMirror Thread {
				get; set;
			}

			public ValueImpl Value {
				get; set;
			}

			public ValueImpl OutThis {
				get; set;
			}

			public ValueImpl[] OutArgs {
				get; set;
			}

			public ValueImpl Exception {
				get; set;
			}

			public int ID {
				get; set;
			}

			public bool IsMultiple {
				get; set;
			}

			public int NumPending;

			public void Abort ()
			{
				if (ID == 0) // Ooops
					return;

				AbortInvoke (VM, Thread, ID);
			}
		}

		static IInvokeAsyncResult BeginInvokeMethod (VirtualMachine vm, ThreadMirror thread, MethodMirror method, Value this_obj, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			if (thread == null)
				throw new ArgumentNullException ("thread");
			if (method == null)
				throw new ArgumentNullException ("method");
			if (arguments == null)
				arguments = new Value [0];

			InvokeFlags f = InvokeFlags.NONE;

			if ((options & InvokeOptions.DisableBreakpoints) != 0)
				f |= InvokeFlags.DISABLE_BREAKPOINTS;
			if ((options & InvokeOptions.SingleThreaded) != 0)
				f |= InvokeFlags.SINGLE_THREADED;
			if ((options & InvokeOptions.ReturnOutThis) != 0)
				f |= InvokeFlags.OUT_THIS;
			if ((options & InvokeOptions.ReturnOutArgs) != 0)
				f |= InvokeFlags.OUT_ARGS;
			if ((options & InvokeOptions.Virtual) != 0)
				f |= InvokeFlags.VIRTUAL;

			InvokeAsyncResult r = new InvokeAsyncResult { AsyncState = state, AsyncWaitHandle = new ManualResetEvent (false), VM = vm, Thread = thread, Callback = callback };
			thread.InvalidateFrames ();
			r.ID = vm.conn.VM_BeginInvokeMethod (thread.Id, method.Id, this_obj != null ? vm.EncodeValue (this_obj) : vm.EncodeValue (vm.CreateValue (null)), vm.EncodeValues (arguments), f, InvokeCB, r);

			return r;
		}

		// This is called when the result of an invoke is received
		static void InvokeCB (ValueImpl v, ValueImpl exc, ValueImpl out_this, ValueImpl[] out_args, ErrorCode error, object state) {
			InvokeAsyncResult r = (InvokeAsyncResult)state;

			if (error != 0) {
				r.ErrorCode = error;
			} else {
				r.Value = v;
				r.Exception = exc;
			}

			r.OutThis = out_this;
			r.OutArgs = out_args;

			r.IsCompleted = true;
			((ManualResetEvent)r.AsyncWaitHandle).Set ();

			if (r.Callback != null)
				r.Callback.BeginInvoke (r, null, null);
		}

		static InvokeResult EndInvokeMethodInternalWithResult (IInvocableMethodOwnerMirror mirror, IAsyncResult asyncResult) {
			var result = EndInvokeMethodInternalWithResultImpl (asyncResult);
			mirror.ProcessResult (result);
			return result;
		}

		static InvokeResult EndInvokeMethodInternalWithResultImpl (IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			InvokeAsyncResult r = (InvokeAsyncResult)asyncResult;

			if (!r.IsCompleted)
				r.AsyncWaitHandle.WaitOne ();

			if (r.ErrorCode != 0) {
				try {
					r.VM.ErrorHandler (null, new ErrorHandlerEventArgs () { ErrorCode = r.ErrorCode });
				} catch (CommandException ex) {
					if (ex.ErrorCode == ErrorCode.INVALID_ARGUMENT)
						throw new ArgumentException ("Incorrect number or types of arguments", "arguments");

					throw;
				}
				throw new NotImplementedException ();
			} else {
				if (r.Exception != null)
					throw new InvocationException ((ObjectMirror)r.VM.DecodeValue (r.Exception));

				Value out_this = null;
				if (r.OutThis != null)
					out_this = r.VM.DecodeValue (r.OutThis);
				Value[] out_args = null;
				if (r.OutArgs != null)
					out_args = r.VM.DecodeValues (r.OutArgs);

				return new InvokeResult () { Result = r.VM.DecodeValue (r.Value), OutThis = out_this, OutArgs = out_args };
			}
		}

		static Value EndInvokeMethodInternal (IInvocableMethodOwnerMirror mirror, IAsyncResult asyncResult) {
			InvokeResult res = EndInvokeMethodInternalWithResult (mirror, asyncResult);
			return res.Result;
		}

		static void EndInvokeMultipleInternal (IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			InvokeAsyncResult r = (InvokeAsyncResult)asyncResult;

			if (!r.IsCompleted)
				r.AsyncWaitHandle.WaitOne ();
		}

		static Value InvokeMethod (IInvocableMethodOwnerMirror mirror, VirtualMachine vm, ThreadMirror thread, MethodMirror method, Value this_obj, IList<Value> arguments, InvokeOptions options) {
			return EndInvokeMethodInternal (mirror, BeginInvokeMethod (vm, thread, method, this_obj, arguments, options, null, null));
		}

		static void AbortInvoke (VirtualMachine vm, ThreadMirror thread, int id)
		{
			vm.conn.VM_AbortInvoke (thread.Id, id);
		}

		//
		// Implementation of InvokeMultiple
		//

		static IInvokeAsyncResult BeginInvokeMultiple (VirtualMachine vm, ThreadMirror thread, MethodMirror[] methods, Value this_obj, IList<IList<Value>> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			if (thread == null)
				throw new ArgumentNullException ("thread");
			if (methods == null)
				throw new ArgumentNullException ("methods");
			foreach (var m in methods)
				if (m == null)
					throw new ArgumentNullException ("method");
			if (arguments == null) {
				arguments = new List<IList<Value>> ();
				for (int i = 0; i < methods.Length; ++i)
					arguments.Add (new Value [0]);
			} else {
				// FIXME: Not needed for property evaluation
				throw new NotImplementedException ();
			}
			if (callback == null)
				throw new ArgumentException ("A callback argument is required for this method.", "callback");

			InvokeFlags f = InvokeFlags.NONE;

			if ((options & InvokeOptions.DisableBreakpoints) != 0)
				f |= InvokeFlags.DISABLE_BREAKPOINTS;
			if ((options & InvokeOptions.SingleThreaded) != 0)
				f |= InvokeFlags.SINGLE_THREADED;

			InvokeAsyncResult r = new InvokeAsyncResult { AsyncState = state, AsyncWaitHandle = new ManualResetEvent (false), VM = vm, Thread = thread, Callback = callback, NumPending = methods.Length, IsMultiple = true };

			var mids = new long [methods.Length];
			for (int i = 0; i < methods.Length; ++i)
				mids [i] = methods [i].Id;
			var args = new List<ValueImpl[]> ();
			for (int i = 0; i < methods.Length; ++i)
				args.Add (vm.EncodeValues (arguments [i]));
			thread.InvalidateFrames ();
			r.ID = vm.conn.VM_BeginInvokeMethods (thread.Id, mids, this_obj != null ? vm.EncodeValue (this_obj) : vm.EncodeValue (vm.CreateValue (null)), args, f, InvokeMultipleCB, r);

			return r;
		}

		// This is called when the result of an invoke is received
		static void InvokeMultipleCB (ValueImpl v, ValueImpl exc, ValueImpl out_this, ValueImpl[] out_args, ErrorCode error, object state) {
			var r = (InvokeAsyncResult)state;

			Interlocked.Decrement (ref r.NumPending);

			if (error != 0)
				r.ErrorCode = error;

			if (r.NumPending == 0) {
				r.IsCompleted = true;
				((ManualResetEvent)r.AsyncWaitHandle).Set ();
			}

			// Have to pass another asyncresult to the callback since multiple threads can execute it concurrently with results of multiple invocations
			var r2 = new InvokeAsyncResult { AsyncState = r.AsyncState, AsyncWaitHandle = null, VM = r.VM, Thread = r.Thread, Callback = r.Callback, IsCompleted = true };

			if (error != 0) {
				r2.ErrorCode = error;
			} else {
				r2.Value = v;
				r2.Exception = exc;
			}

			r.Callback.BeginInvoke (r2, null, null);
		}

	}
}