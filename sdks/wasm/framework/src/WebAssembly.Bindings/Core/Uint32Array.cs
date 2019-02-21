using System;
namespace WebAssembly.Core {
	public sealed class Uint32Array : TypedArray<Uint32Array, uint> {
		public Uint32Array () { }

		public Uint32Array (int length) : base (length) { }


		public Uint32Array (ArrayBuffer buffer) : base (buffer) { }

		public Uint32Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Uint32Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Uint32Array (IntPtr js_handle) : base (js_handle) { }

	}
}
