using System;
namespace WebAssembly.Core {
	public sealed class Uint16Array : TypedArray<Uint16Array, ushort> {
		public Uint16Array () { }

		public Uint16Array (int length) : base (length) { }


		public Uint16Array (ArrayBuffer buffer) : base (buffer) { }

		public Uint16Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Uint16Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Uint16Array (IntPtr js_handle) : base (js_handle)
		{ }

	}
}
