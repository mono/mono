using System;
namespace WebAssembly.Core {
	public sealed class Int8Array : TypedArray<Int8Array, sbyte> {
		public Int8Array ()
		{ }

		public Int8Array (int length) : base (length)
		{ }


		public Int8Array (ArrayBuffer buffer) : base (buffer)
		{ }

		public Int8Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset)
		{ }

		public Int8Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length)
		{ }

		public Int8Array (SharedArrayBuffer buffer) : base (buffer)
		{ }

		public Int8Array (SharedArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset)
		{ }

		public Int8Array (SharedArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length)
		{ }

		internal Int8Array (IntPtr js_handle) : base (js_handle)
		{ }

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Int8Array"/> class to a <see cref="Span<sbyte>"/>
		/// </summary>
		public static implicit operator Span<sbyte>(Int8Array typedarray) => typedarray.ToArray ();

		/// <summary>
		/// Defines an implicit conversion of <see cref="Span<sbyte>"/> to a <see cref="T:WebAssembly.Core.Int8Array"/> class.
		/// </summary>
		public static implicit operator Int8Array (Span<sbyte> span) => From (span);
	}
}
