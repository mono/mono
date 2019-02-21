using System;
namespace WebAssembly.Core {
	public sealed class Float32Array : TypedArray<Float32Array, float> {
		public Float32Array () { }

		public Float32Array (int length) : base (length) { }


		public Float32Array (ArrayBuffer buffer) : base (buffer) { }

		public Float32Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Float32Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Float32Array (IntPtr js_handle) : base (js_handle) { }


	}
}
