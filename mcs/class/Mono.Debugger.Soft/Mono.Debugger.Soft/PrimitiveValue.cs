using System;
using System.Collections.Generic;

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
			if (obj != null && obj is PrimitiveValue)
				return value == (obj as PrimitiveValue).Value;
			return base.Equals (obj);
		}

		public override int GetHashCode () {
			return base.GetHashCode ();
		}

		public override string ToString () {
			return "PrimitiveValue<" + Value + ">";
		}
	}
}