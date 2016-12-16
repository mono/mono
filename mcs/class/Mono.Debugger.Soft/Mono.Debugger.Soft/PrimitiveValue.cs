namespace Mono.Debugger.Soft
{
	/*
	 * Represents a value of a primitive type in the debuggee
	 */
	public class PrimitiveValue : Value, IInvocableMethodOwnerMirror {
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

		Value IInvocableMethodOwnerMirror.GetThisObject () {
			return this;
		}

		void IInvocableMethodOwnerMirror.ProcessResult (InvokeResult result)
		{
		}

		public override string ToString () {
			object v = Value;

			return "PrimitiveValue<" + (v != null ? v.ToString () : "(null)") + ">";
		}
	}
}