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

		public Uint8Array (SharedArrayBuffer buffer) : base (buffer)
		{ }

		public Uint8Array (SharedArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset)
		{ }

		public Uint8Array (SharedArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length)
		{ }

		internal Uint8Array (IntPtr js_handle) : base (js_handle)
		{ }

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Uint8Array"/> class to a <see cref="Span<byte>"/>
		/// </summary>
		public static implicit operator Span <byte> (Uint8Array typedarray) => typedarray.ToArray ();

		/// <summary>
		/// Defines an implicit conversion of <see cref="Span<byte>"/> to a <see cref="T:WebAssembly.Core.Uint8Array"/> class.
		/// </summary>
		public static implicit operator Uint8Array (Span<byte> span) => From(span);
	}
}
