using System;
namespace WebAssembly.Core {
	public sealed class Float64Array : TypedArray<Float64Array, double> {
		public Float64Array () { }

		public Float64Array (int length) : base (length) { }


		public Float64Array (ArrayBuffer buffer) : base (buffer) { }

		public Float64Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Float64Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		public Float64Array (SharedArrayBuffer buffer) : base (buffer) { }

		public Float64Array (SharedArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Float64Array (SharedArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Float64Array (IntPtr js_handle) : base (js_handle) { }

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Float64Array"/> class to a <see cref="Span<double>"/>
		/// </summary>
		public static implicit operator Span<double>(Float64Array typedarray) => typedarray.ToArray ();

		/// <summary>
		/// Defines an implicit conversion of <see cref="Span<double>"/> to a <see cref="T:WebAssembly.Core.Float64Array"/> class.
		/// </summary>
		public static implicit operator Float64Array (Span<double> span) => From (span);
	}
}
