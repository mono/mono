using System;

namespace WebAssembly.Core {
	public sealed class Int32Array : TypedArray<Int32Array, int> {
		public Int32Array () { }

		public Int32Array (int length) : base (length) { }


		public Int32Array (ArrayBuffer buffer) : base (buffer) { }

		public Int32Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Int32Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		public Int32Array (SharedArrayBuffer buffer) : base (buffer) { }

		public Int32Array (SharedArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Int32Array (SharedArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Int32Array (IntPtr js_handle) : base (js_handle) { }

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Int32Array"/> class to a <see cref="Span<int>"/>
		/// </summary>
		public static implicit operator Span<int>(Int32Array typedarray) => typedarray.ToArray ();

		/// <summary>
		/// Defines an implicit conversion of <see cref="Span<int>"/> to a <see cref="T:WebAssembly.Core.Int32Array"/> class.
		/// </summary>
		public static implicit operator Int32Array (Span<int> span) => From (span);
	}
}
