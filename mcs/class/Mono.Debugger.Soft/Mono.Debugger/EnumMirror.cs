using System;
using System.Collections.Generic;

namespace Mono.Debugger
{
	/*
	 * Represents an enum value in the debuggee
	 */
	public class EnumMirror : StructMirror {
	
		internal EnumMirror (VirtualMachine vm, TypeMirror type, Value[] fields) : base (vm, type, fields) {
		}

		public object Value {
			get {
				return ((PrimitiveValue)Fields [0]).Value;
			}
			set {
				SetField (0, vm.CreateValue (value));
			}
		}

		public string StringValue {
			get {
				foreach (FieldInfoMirror f in Type.GetFields ()) {
					if (f.IsStatic) {
						object v = (Type.GetValue (f) as EnumMirror).Value;
						if (f.IsStatic && v.Equals (Value))
							return f.Name;
					}
				}
				return Value.ToString ();
			}
		}
	}
}