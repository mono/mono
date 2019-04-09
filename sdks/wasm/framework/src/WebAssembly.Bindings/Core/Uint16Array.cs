using System;
namespace WebAssembly.Core {
	public sealed class Uint16Array : TypedArray<Uint16Array, ushort> {
		public Uint16Array () { }

		public Uint16Array (int length) : base (length) { }


		public Uint16Array (ArrayBuffer buffer) : base (buffer) { }

		public Uint16Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Uint16Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		public Uint16Array (SharedArrayBuffer buffer) : base (buffer) { }

		public Uint16Array (SharedArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Uint16Array (SharedArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Uint16Array (IntPtr js_handle) : base (js_handle)
		{ }

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Uint16Array"/> class to a <see cref="Span<ushort>"/>
		/// </summary>
		public static implicit operator Span<ushort>(Uint16Array typedarray) => typedarray.ToArray ();

		/// <summary>
		/// Defines an implicit conversion of <see cref="Span<ushort>"/> to a <see cref="T:WebAssembly.Core.Uint16Array"/> class.
		/// </summary>
		public static implicit operator Uint16Array (Span<ushort> span) => From (span);
	}
}
