using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Mono.Debugger.Soft
{
	public class ParameterInfoMirror : Mirror {

		MethodMirror method;
		TypeMirror type;
		string name;
		int pos;
		ParameterAttributes attrs;

		internal ParameterInfoMirror (MethodMirror method, int pos, TypeMirror type, string name, ParameterAttributes attrs) : base (method.VirtualMachine, 0) {
			this.method = method;
			this.pos = pos;
			this.type = type;
			this.name = name;
			this.attrs = attrs;
		}

		public TypeMirror ParameterType {
			get {
				return type;
			}
		}

		public MethodMirror Method {
			get {
				return method;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public int Position {
			get {
				return pos;
			}
		}

		public ParameterAttributes Attributes {
			get {
				return attrs;
			}
		}

		public bool IsRetval {
			get {
				return (Attributes & ParameterAttributes.Retval) != 0;
			}
		}

		public override string ToString () {
			return String.Format ("ParameterInfo ({0})", Name);
		}
	}
}