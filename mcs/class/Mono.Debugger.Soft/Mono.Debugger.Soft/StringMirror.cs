using System;
using System.Collections;

namespace Mono.Debugger.Soft
{
	public class StringMirror : ObjectMirror {

		int length;

		internal StringMirror (VirtualMachine vm, long id) : base (vm, id) {
			length = -1;
		}

		internal StringMirror (VirtualMachine vm, long id, TypeMirror type, AppDomainMirror domain) : base (vm, id, type, domain) {
			length = -1;
		}

		public string Value {
			get {
				return vm.conn.String_GetValue (id);
			}
		}

		// Since protocol version 2.10
		public int Length {
			get {
				if (length == -1)
					length = vm.conn.String_GetLength (id);
				return length;
			}
		}

		// Since protocol version 2.10
		public char[] GetChars (int index, int length) {
			// re-ordered to avoid possible integer overflow
			if (index > Length - length)
				throw new ArgumentException (Locale.GetText (
					"index and length do not specify a valid range in string."));

			return vm.conn.String_GetChars (id, index, length);
		}
	}
}
