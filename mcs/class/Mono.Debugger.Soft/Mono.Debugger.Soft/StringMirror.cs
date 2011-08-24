using System;
using System.Collections;

namespace Mono.Debugger.Soft
{
	public class StringMirror : ObjectMirror {

		internal StringMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		internal StringMirror (VirtualMachine vm, long id, TypeMirror type, AppDomainMirror domain) : base (vm, id, type, domain) {
		}

		public string Value {
			get {
				return vm.conn.String_GetValue (id);
			}
		}
	}
}
