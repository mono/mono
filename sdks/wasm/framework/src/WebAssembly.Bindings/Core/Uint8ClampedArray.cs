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

		public Uint8ClampedArray (SharedArrayBuffer buffer) : base (buffer)
		{ }

		public Uint8ClampedArray (SharedArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset)
		{ }

		public Uint8ClampedArray (SharedArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length)
		{ }

		internal Uint8ClampedArray (IntPtr js_handle) : base (js_handle)
		{ }

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Uint8ClampedArray"/> class to a <see cref="Span<byte>"/>
		/// </summary>
		public static implicit operator Span<byte>(Uint8ClampedArray typedarray) => typedarray.ToArray ();

		/// <summary>
		/// Defines an implicit conversion of <see cref="Span<byte>"/> to a <see cref="T:WebAssembly.Core.Uint8ClampedArray"/> class.
		/// </summary>
		public static implicit operator Uint8ClampedArray (Span<byte> span) => From (span);
	}
}
