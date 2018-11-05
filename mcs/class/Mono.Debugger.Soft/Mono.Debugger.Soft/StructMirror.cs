using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mono.Debugger.Soft
{
	/*
	 * Represents a valuetype value in the debuggee
	 */
	public class StructMirror : Value, IInvokable {
	
		TypeMirror type;
		Value[] fields;

		internal StructMirror (VirtualMachine vm, TypeMirror type, Value[] fields) : base (vm, 0) {
			this.type = type;
			this.fields = fields;
		}

		public TypeMirror Type {
			get {
				return type;
			}
		}

		public Value[] Fields {
			get {
				return fields;
			}
		}

		public Value this [String field] {
			get {
				FieldInfoMirror[] field_info = Type.GetFields ();
				int nf = 0;
				for (int i = 0; i < field_info.Length; ++i) {
					if (!field_info [i].IsStatic) {
						if (field_info [i].Name == field)
							return Fields [nf];
						nf++;
					}
				}
				throw new ArgumentException ("Unknown struct field '" + field + "'.", "field");
			}
			set {
				FieldInfoMirror[] field_info = Type.GetFields ();
				int nf = 0;
				for (int i = 0; i < field_info.Length; ++i) {
					if (!field_info [i].IsStatic) {
						if (field_info [i].Name == field) {
							fields [nf] = value;
							return;
						}
						nf++;
					}
				}
				throw new ArgumentException ("Unknown struct field '" + field + "'.", "field");
			}
		}

		internal void SetFields (Value[] fields) {
			this.fields = fields;
		}

		internal void SetField (int index, Value value) {
			fields [index] = value;
		}

		public Value InvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments) {
			return ObjectMirror.InvokeMethod (vm, thread, method, this, arguments, InvokeOptions.None);
		}

		public Value InvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options) {
			return ObjectMirror.InvokeMethod (vm, thread, method, this, arguments, options);
		}

		[Obsolete ("Use the overload without the 'vm' argument")]
		public IAsyncResult BeginInvokeMethod (VirtualMachine vm, ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			return ObjectMirror.BeginInvokeMethod (vm, thread, method, this, arguments, options, callback, state);
		}

		public IAsyncResult BeginInvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			return ObjectMirror.BeginInvokeMethod (vm, thread, method, this, arguments, options, callback, state);
		}

		public Value EndInvokeMethod (IAsyncResult asyncResult) {
			var result = EndInvokeMethodWithResult (asyncResult);
			return result.Result;
		}

		public InvokeResult EndInvokeMethodWithResult (IAsyncResult asyncResult) {
			var result = ObjectMirror.EndInvokeMethodInternalWithResult (asyncResult);
			var outThis = result.OutThis as StructMirror;
			if (outThis != null) {
				SetFields (outThis.Fields);
			}
			return result;
		}

		public Task<Value> InvokeMethodAsync (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options = InvokeOptions.None) {
			return ObjectMirror.InvokeMethodAsync (vm, thread, method, this, arguments, options, EndInvokeMethod);
		}

		public Task<InvokeResult> InvokeMethodAsyncWithResult (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options = InvokeOptions.None) {
			return ObjectMirror.InvokeMethodAsync (vm, thread, method, this, arguments, options, EndInvokeMethodWithResult);
		}
	}
}
