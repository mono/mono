using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Mono.Debugger.Soft
{
	public class ObjectMirror : Value {
		TypeMirror type;
		AppDomainMirror domain;
	
		internal ObjectMirror (VirtualMachine vm, long id) : base (vm, id) {
		}
	
		internal ObjectMirror (VirtualMachine vm, long id, TypeMirror type, AppDomainMirror domain) : base (vm, id) {
			this.type = type;
			this.domain = domain;
		}

		public TypeMirror Type {
			get {
				if (type == null)
				 	type = vm.GetType (vm.conn.Object_GetType (id));
				return type;
			}
		}

		public AppDomainMirror Domain {
			get {
				if (domain == null)
				 	domain = vm.GetDomain (vm.conn.Object_GetDomain (id));
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
				if (ex.ErrorCode == ErrorCode.INVALID_FIELDID)
					throw new ArgumentException ("One of the fields is not valid for this type.", "fields");
				else
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

			public ValueImpl Exception {
				get; set;
			}

			public int ID {
				get; set;
			}

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

			InvokeAsyncResult r = new InvokeAsyncResult { AsyncState = state, AsyncWaitHandle = new ManualResetEvent (false), VM = vm, Thread = thread, Callback = callback };

			r.ID = vm.conn.VM_BeginInvokeMethod (thread.Id, method.Id, this_obj != null ? vm.EncodeValue (this_obj) : vm.EncodeValue (vm.CreateValue (null)), vm.EncodeValues (arguments), f, InvokeCB, r);

			return r;
		}

		// This is called when the result of an invoke is received
		static void InvokeCB (ValueImpl v, ValueImpl exc, ErrorCode error, object state) {
			InvokeAsyncResult r = (InvokeAsyncResult)state;

			if (error != 0) {
				r.ErrorCode = error;
			} else {
				r.Value = v;
				r.Exception = exc;
			}

			r.IsCompleted = true;
			((ManualResetEvent)r.AsyncWaitHandle).Set ();

			if (r.Callback != null)
				r.Callback.BeginInvoke (r, null, null);
		}

	    internal static Value EndInvokeMethodInternal (IAsyncResult asyncResult) {
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
					else
						throw;
				}
				throw new NotImplementedException ();
			} else {
				if (r.Exception != null)
					throw new InvocationException ((ObjectMirror)r.VM.DecodeValue (r.Exception));
				else
					return r.VM.DecodeValue (r.Value);
			}
		}

		internal static Value InvokeMethod (VirtualMachine vm, ThreadMirror thread, MethodMirror method, Value this_obj, IList<Value> arguments, InvokeOptions options) {
			return EndInvokeMethodInternal (BeginInvokeMethod (vm, thread, method, this_obj, arguments, options, null, null));
		}

		internal static void AbortInvoke (VirtualMachine vm, ThreadMirror thread, int id)
		{
			vm.conn.VM_AbortInvoke (thread.Id, id);
		}
	}
}
