using System;
namespace WebAssembly.Core {
	public sealed class Int16Array : TypedArray<Int16Array, short> {
		public Int16Array () { }

		public Int16Array (int length) : base (length) { }


		public Int16Array (ArrayBuffer buffer) : base (buffer) { }

		public Int16Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Int16Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Int16Array (IntPtr js_handle) : base (js_handle)
		{ }

	}
}
