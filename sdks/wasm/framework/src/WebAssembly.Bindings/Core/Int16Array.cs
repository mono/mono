using System;
namespace WebAssembly.Core {
	public sealed class Int16Array : TypedArray<Int16Array, short> {
		public Int16Array () { }

		public Int16Array (int length) : base (length) { }


		public Int16Array (ArrayBuffer buffer) : base (buffer) { }

		public Int16Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Int16Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		public Int16Array (SharedArrayBuffer buffer) : base (buffer) { }

		public Int16Array (SharedArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Int16Array (SharedArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Int16Array (IntPtr js_handle) : base (js_handle)
		{ }

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Int16Array"/> class to a <see cref="Span<short>"/>
		/// </summary>
		public static implicit operator Span<short>(Int16Array typedarray) => typedarray.ToArray ();

		/// <summary>
		/// Defines an implicit conversion of <see cref="Span<short>"/> to a <see cref="T:WebAssembly.Core.Int16Array"/> class.
		/// </summary>
		public static implicit operator Int16Array (Span<short> span) => From (span);
	}
}
