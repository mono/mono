using System;
namespace WebAssembly.Core {
	public sealed class Float32Array : TypedArray<Float32Array, float> {
		public Float32Array () { }

		public Float32Array (int length) : base (length) { }


		public Float32Array (ArrayBuffer buffer) : base (buffer) { }

		public Float32Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Float32Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		public Float32Array (SharedArrayBuffer buffer) : base (buffer) { }

		public Float32Array (SharedArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Float32Array (SharedArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Float32Array (IntPtr js_handle) : base (js_handle) { }

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Float32Array"/> class to a <see cref="Span<float>"/>
		/// </summary>
		public static implicit operator Span<float>(Float32Array typedarray) => typedarray.ToArray ();

		/// <summary>
		/// Defines an implicit conversion of <see cref="Span<float>"/> to a <see cref="T:WebAssembly.Core.Float32Array"/> class.
		/// </summary>
		public static implicit operator Float32Array (Span<float> span) => From (span);
	}
}
