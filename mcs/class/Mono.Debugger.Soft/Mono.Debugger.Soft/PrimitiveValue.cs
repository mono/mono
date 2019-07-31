using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mono.Debugger.Soft
{
	/*
	 * Represents a value of a primitive type in the debuggee
	 */
	public class PrimitiveValue : Value {
		object value;

		public PrimitiveValue (VirtualMachine vm, object value) : base (vm, 0) {
			this.value = value;
		}

		public object Value {
			get {
				return value;
			}
		}

		public override bool Equals (object obj) {
			if (value == obj)
				return true;

			var primitive = obj as PrimitiveValue;
			if (primitive != null)
				return value == primitive.Value;

			return base.Equals (obj);
		}

		public override int GetHashCode () {
			return base.GetHashCode ();
		}

		public override string ToString () {
			object v = Value;

			return "PrimitiveValue<" + (v != null ? v.ToString () : "(null)") + ">";
		}

		public Value InvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments) {
			return ObjectMirror.InvokeMethod (vm, thread, method, this, arguments, InvokeOptions.None);
		}

		public Value InvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options) {
			return ObjectMirror.InvokeMethod (vm, thread, method, this, arguments, options);
		}

		public IAsyncResult BeginInvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			return ObjectMirror.BeginInvokeMethod (vm, thread, method, this, arguments, options, callback, state);
		}

		public Value EndInvokeMethod (IAsyncResult asyncResult) {
			return ObjectMirror.EndInvokeMethodInternal (asyncResult);
		}

		public InvokeResult EndInvokeMethodWithResult (IAsyncResult asyncResult) {
			return ObjectMirror.EndInvokeMethodInternalWithResult (asyncResult);
		}

		public Task<Value> InvokeMethodAsync (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options = InvokeOptions.None) {
			return ObjectMirror.InvokeMethodAsync (vm, thread, method, this, arguments, options);
		}

		public Task<InvokeResult> InvokeMethodAsyncWithResult (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options = InvokeOptions.None) {
			return ObjectMirror.InvokeMethodAsyncWithResult (vm, thread, method, this, arguments, options);
		}
	}
}