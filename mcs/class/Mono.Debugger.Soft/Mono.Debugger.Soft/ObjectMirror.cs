using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Mono.Debugger.Soft
{
	public class InvokeResult {
		public Value Result { get; set; }
		//
		// The value of the receiver after the call for calls to valuetype methods or null.
		// Only set when using the InvokeOptions.ReturnOutThis flag.
		// Since protocol version 2.35
		//
		public Value OutThis { get; set; }
		//
		// The value of the arguments after the call
		// Only set when using the InvokeOptions.ReturnOutArgs flag.
		// Since protocol version 2.35
		//
		public Value[] OutArgs { get; set; }
	}

	public interface IInvokable {
		Value InvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments);
		Value InvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options);
		IAsyncResult BeginInvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state);
		Value EndInvokeMethod (IAsyncResult asyncResult);
		InvokeResult EndInvokeMethodWithResult (IAsyncResult asyncResult);
		Task<Value> InvokeMethodAsync (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options = InvokeOptions.None);
		Task<InvokeResult> InvokeMethodAsyncWithResult (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options = InvokeOptions.None);
	}

	public class ObjectMirror : Value, IInvokable {
		TypeMirror type;
		AppDomainMirror domain;
	
		internal ObjectMirror (VirtualMachine vm, long id) : base (vm, id) {
		}
	
		internal ObjectMirror (VirtualMachine vm, long id, TypeMirror type, AppDomainMirror domain) : base (vm, id) {
			this.type = type;
			this.domain = domain;
		}

		void GetInfo () {
			var info = vm.conn.Object_GetInfo (id);
			type = vm.GetType (info.type_id);
			domain = vm.GetDomain (info.domain_id);
		}

		public TypeMirror Type {
			get {
				if (type == null) {
					if (vm.conn.Version.AtLeast (2, 5))
						GetInfo ();
					else
				 		type = vm.GetType (vm.conn.Object_GetType (id));
				}
				return type;
			}
		}

		public AppDomainMirror Domain {
			get {
				if (domain == null) {
					if (vm.conn.Version.AtLeast (2, 5))
						GetInfo ();
					else
						domain = vm.GetDomain (vm.conn.Object_GetDomain (id));
				}
				return domain;
			}
		}

		public bool IsCollected {
			get {
				return vm.conn.Object_IsCollected (id);
			}
		}

		public Value GetValue (FieldInfoMirror field) {
			return GetValues (new FieldInfoMirror [] { field }) [0];
		}

		public Value[] GetValues (IList<FieldInfoMirror> fields) {
			if (fields == null)
				throw new ArgumentNullException ("fields");
			foreach (FieldInfoMirror f in fields) {
				if (f == null)
					throw new ArgumentNullException ("field");
				CheckMirror (f);
			}
			long[] ids = new long [fields.Count];
			for (int i = 0; i < fields.Count; ++i)
				ids [i] = fields [i].Id;
			try {
				return vm.DecodeValues (vm.conn.Object_GetValues (id, ids));
			} catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.INVALID_FIELDID) {
					if (fields.Count == 1)
						throw new ArgumentException (string.Format ("The field '{0}' is not valid for this type.", fields[0].Name));
					throw new ArgumentException ("One of the fields is not valid for this type.", "fields");
				} else
					throw;
			}
		}

		public void SetValues (IList<FieldInfoMirror> fields, Value[] values) {
			if (fields == null)
				throw new ArgumentNullException ("fields");
			if (values == null)
				throw new ArgumentNullException ("values");
			foreach (FieldInfoMirror f in fields) {
				if (f == null)
					throw new ArgumentNullException ("field");
				CheckMirror (f);
			}
			foreach (Value v in values) {
				if (v == null)
					throw new ArgumentNullException ("values");
				CheckMirror (v);
			}
			long[] ids = new long [fields.Count];
			for (int i = 0; i < fields.Count; ++i)
				ids [i] = fields [i].Id;
			try {
				vm.conn.Object_SetValues (id, ids, vm.EncodeValues (values));
			} catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.INVALID_FIELDID)
					throw new ArgumentException ("One of the fields is not valid for this type.", "fields");
				else if (ex.ErrorCode == ErrorCode.INVALID_ARGUMENT)
					throw new ArgumentException ("One of the values is not valid for its field.", "values");
				else
					throw;
			}
		}

		public void SetValue (FieldInfoMirror field, Value value) {
			SetValues (new FieldInfoMirror [] { field }, new Value [] { value });
		}

		/*
		 * The current address of the object. It can change during garbage 
		 * collections. Use a long since the debuggee might have a different 
		 * pointer size. 
		 */
		public long Address {
			get {
				return vm.conn.Object_GetAddress (id);
			}
		}

		public Value InvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments) {
			return InvokeMethod (vm, thread, method, this, arguments, InvokeOptions.None);
		}

		public Value InvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options) {
			return InvokeMethod (vm, thread, method, this, arguments, options);
		}

		[Obsolete ("Use the overload without the 'vm' argument")]
		public IAsyncResult BeginInvokeMethod (VirtualMachine vm, ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			return BeginInvokeMethod (vm, thread, method, this, arguments, options, callback, state);
		}

		public IAsyncResult BeginInvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			return BeginInvokeMethod (vm, thread, method, this, arguments, options, callback, state);
		}

		public Value EndInvokeMethod (IAsyncResult asyncResult) {
			return EndInvokeMethodInternal (asyncResult);
		}

		public InvokeResult EndInvokeMethodWithResult (IAsyncResult asyncResult) {
			return ObjectMirror.EndInvokeMethodInternalWithResult (asyncResult);
		}

		public Task<Value> InvokeMethodAsync (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options = InvokeOptions.None) {
			return InvokeMethodAsync (vm, thread, method, this, arguments, options);
		}

		internal static Task<Value> InvokeMethodAsync (VirtualMachine vm, ThreadMirror thread, MethodMirror method, Value this_obj, IList<Value> arguments, InvokeOptions options) {
			return InvokeMethodAsync (vm, thread, method, this_obj, arguments, options, EndInvokeMethodInternal);
		}

		public Task<InvokeResult> InvokeMethodAsyncWithResult (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options = InvokeOptions.None) {
			return InvokeMethodAsyncWithResult (vm, thread, method, this, arguments, options);
		}

		internal static Task<InvokeResult> InvokeMethodAsyncWithResult (VirtualMachine vm, ThreadMirror thread, MethodMirror method, Value this_obj, IList<Value> arguments, InvokeOptions options) {
			return InvokeMethodAsync (vm, thread, method, this_obj, arguments, options, EndInvokeMethodInternalWithResult);
		}

		internal static Task<TResult> InvokeMethodAsync<TResult> (VirtualMachine vm, ThreadMirror thread, MethodMirror method, Value this_obj, IList<Value> arguments, InvokeOptions options, Func<IAsyncResult, TResult> callback) {
			var tcs = new TaskCompletionSource<TResult> ();
			BeginInvokeMethod (vm, thread, method, this_obj, arguments, options, iar =>
			{
				try {
					tcs.SetResult (callback (iar));
				} catch (OperationCanceledException) {
					tcs.TrySetCanceled ();
				} catch (Exception ex) {
					tcs.TrySetException (ex);
				}
			}, null);
			return tcs.Task;
		}

		//
		// Invoke the members of METHODS one-by-one, calling CALLBACK after each invoke was finished. The IAsyncResult will be marked as completed after all invokes have
		// finished. The callback will be called with a different IAsyncResult that represents one method invocation.
		// From protocol version 2.22.
		//
		public IAsyncResult BeginInvokeMultiple (ThreadMirror thread, MethodMirror[] methods, IList<IList<Value>> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			return BeginInvokeMultiple (vm, thread, methods, this, arguments, options, callback, state);
		}

		public void EndInvokeMultiple (IAsyncResult asyncResult) {
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

				ObjectMirror.AbortInvoke (VM, Thread, ID);
			}
		}

		internal static IInvokeAsyncResult BeginInvokeMethod (VirtualMachine vm, ThreadMirror thread, MethodMirror method, Value this_obj, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state) {
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

	    internal static InvokeResult EndInvokeMethodInternalWithResult (IAsyncResult asyncResult) {
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
				
				//refresh frames from thread after running an invoke
				r.Thread.GetFrames();
				
				Value out_this = null;
				if (r.OutThis != null)
					out_this = r.VM.DecodeValue (r.OutThis);
				Value[] out_args = null;
				if (r.OutArgs != null)
					out_args = r.VM.DecodeValues (r.OutArgs);

				return new InvokeResult () { Result = r.VM.DecodeValue (r.Value), OutThis = out_this, OutArgs = out_args };
			}
		}

 	    internal static Value EndInvokeMethodInternal (IAsyncResult asyncResult) {
			InvokeResult res = EndInvokeMethodInternalWithResult (asyncResult);
			return res.Result;
		}

	    internal static void EndInvokeMultipleInternal (IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			InvokeAsyncResult r = (InvokeAsyncResult)asyncResult;

			if (!r.IsCompleted)
				r.AsyncWaitHandle.WaitOne ();
		}

		internal static Value InvokeMethod (VirtualMachine vm, ThreadMirror thread, MethodMirror method, Value this_obj, IList<Value> arguments, InvokeOptions options) {
			return EndInvokeMethodInternal (BeginInvokeMethod (vm, thread, method, this_obj, arguments, options, null, null));
		}

		internal static void AbortInvoke (VirtualMachine vm, ThreadMirror thread, int id)
		{
			vm.conn.VM_AbortInvoke (thread.Id, id);
		}

		//
		// Implementation of InvokeMultiple
		//

		internal static IInvokeAsyncResult BeginInvokeMultiple (VirtualMachine vm, ThreadMirror thread, MethodMirror[] methods, Value this_obj, IList<IList<Value>> arguments, InvokeOptions options, AsyncCallback callback, object state) {
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
