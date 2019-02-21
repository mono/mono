using System;
using System.Runtime.InteropServices;

namespace WebAssembly.Core {
	public sealed class Uint8Array : TypedArray<Uint8Array, byte> {

		/// <summary>
		/// Initializes a new instance of the <see cref="T:WebAssembly.Core.Uint8Array"/> class.
		/// </summary>
		public Uint8Array ()
		{ }

		public Uint8Array (int length) : base (length)
		{ }


		public Uint8Array (ArrayBuffer buffer) : base (buffer)
		{ }

		public Uint8Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset)
		{ }

		public Uint8Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length)
		{ }

		internal Uint8Array (IntPtr js_handle) : base (js_handle)
		{ }

	}
}
