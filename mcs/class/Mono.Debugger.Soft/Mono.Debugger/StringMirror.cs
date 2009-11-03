using System;
using System.Collections;

namespace Mono.Debugger
{
	public class StringMirror : ObjectMirror {

		internal StringMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		public string Value {
			get {
				return vm.conn.String_GetValue (id);
			}
		}
	}
}
