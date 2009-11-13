using System;
using System.Collections.Generic;

namespace Mono.Debugger
{
	public class ObjectMirror : Value {
	
		internal ObjectMirror (VirtualMachine vm, long id) : base (vm, id) {
		}
	
		public TypeMirror Type {
			get {
				return vm.GetType (vm.conn.Object_GetType (id));
			}
		}

		public AppDomainMirror Domain {
			get {
				return vm.GetDomain (vm.conn.Object_GetDomain (id));
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

		internal static Value InvokeMethod (VirtualMachine vm, ThreadMirror thread, MethodMirror method, Value this_obj, IList<Value> arguments, InvokeOptions options) {
			if (thread == null)
				throw new ArgumentNullException ("thread");
			if (method == null)
				throw new ArgumentNullException ("method");
			if (arguments == null)
				arguments = new Value [0];

			InvokeFlags f = InvokeFlags.NONE;

			if ((options & InvokeOptions.DisableBreakpoints) != 0)
				f |= InvokeFlags.DISABLE_BREAKPOINTS;

			try {
				ValueImpl exc;
				ValueImpl v = vm.conn.VM_InvokeMethod (thread.Id, method.Id, this_obj != null ? vm.EncodeValue (this_obj) : vm.EncodeValue (vm.CreateValue (null)), vm.EncodeValues (arguments), f, out exc);
				if (v == null)
					throw new InvocationException ((ObjectMirror)vm.DecodeValue (exc));
				else
					return vm.DecodeValue (v);
			} catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.INVALID_ARGUMENT)
					throw new ArgumentException ("Incorrect number or types of arguments", "arguments");
				else
					throw;
			}
		}

	}
}
