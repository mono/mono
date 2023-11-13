using System;
namespace WebAssembly.Core {
	public sealed class Uint32Array : TypedArray<Uint32Array, uint> {
		public Uint32Array () { }

		public Uint32Array (int length) : base (length) { }


		public Uint32Array (ArrayBuffer buffer) : base (buffer) { }

		public Uint32Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Uint32Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		public Uint32Array (SharedArrayBuffer buffer) : base (buffer) { }

		public Uint32Array (SharedArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Uint32Array (SharedArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Uint32Array (IntPtr js_handle) : base (js_handle) { }

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Uint32Array"/> class to a <see cref="Span<uint>"/>
		/// </summary>
		public static implicit operator Span<uint>(Uint32Array typedarray) => typedarray.ToArray ();

		/// <summary>
		/// Defines an implicit conversion of <see cref="Span<uint>"/> to a <see cref="T:WebAssembly.Core.Uint32Array"/> class.
		/// </summary>
		public static implicit operator Uint32Array (Span<uint> span) => From (span);
	}
}
