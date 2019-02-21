using System;
using System.Runtime.InteropServices;

namespace WebAssembly.Core {
	public sealed class Uint8ClampedArray : TypedArray<Uint8ClampedArray, byte> {
		public Uint8ClampedArray ()
		{ }

		public Uint8ClampedArray (int length) : base (length)
		{ }


		public Uint8ClampedArray (ArrayBuffer buffer) : base (buffer)
		{ }

		public Uint8ClampedArray (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset)
		{ }

		public Uint8ClampedArray (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length)
		{ }

		internal Uint8ClampedArray (IntPtr js_handle) : base (js_handle)
		{ }

	}
}
