using System;

namespace WebAssembly.Core {
	public sealed class Int32Array : TypedArray<Int32Array, int> {
		public Int32Array () { }

		public Int32Array (int length) : base (length) { }


		public Int32Array (ArrayBuffer buffer) : base (buffer) { }

		public Int32Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Int32Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Int32Array (IntPtr js_handle) : base (js_handle) { }

	}
}
