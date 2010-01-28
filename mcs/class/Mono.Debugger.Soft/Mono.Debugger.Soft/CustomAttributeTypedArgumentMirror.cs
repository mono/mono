using System;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Mono.Debugger.Soft {

	public struct CustomAttributeTypedArgumentMirror {
		Type type;
		object value;

		internal CustomAttributeTypedArgumentMirror (Type type, object value)
		{
			this.type = type;
			this.value = value;

			if (value != null)
				this.type = value.GetType ();
			else
				this.type = typeof (void);

			// MS seems to convert arrays into a ReadOnlyCollection
			if (value is Array) {
				Array a = (Array)value;

				Type etype = a.GetType ().GetElementType ();
				CustomAttributeTypedArgumentMirror[] new_value = new CustomAttributeTypedArgumentMirror [a.GetLength (0)];
				for (int i = 0; i < new_value.Length; ++i)
					new_value [i] = new CustomAttributeTypedArgumentMirror (etype, a.GetValue (i));
				this.value = new ReadOnlyCollection <CustomAttributeTypedArgumentMirror> (new_value);
			}
		}

		public Type ArgumentType {
			get {
				return type;
			}
		}

		public object Value {
			get {
				return value;
			}
		}

		public override string ToString ()
		{
			string val = value != null ? value.ToString () : String.Empty;
			if (ArgumentType == typeof (string))
				return "\"" + val + "\"";
			if (ArgumentType == typeof (Type)) 
				return "typeof (" + val + ")";
			if (ArgumentType.IsEnum)
				return "(" + ArgumentType.Name + ")" + val;

			return val;
		}
	}
}
