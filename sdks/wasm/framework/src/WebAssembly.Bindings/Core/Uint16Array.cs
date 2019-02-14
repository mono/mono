using System;
namespace WebAssembly.Core {
	public sealed class Uint16Array : TypedArray<Uint16Array, ushort> {
		public Uint16Array () { }

		public Uint16Array (int length) : base (length) { }


		public Uint16Array (ArrayBuffer buffer) : base (buffer) { }

		public Uint16Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Uint16Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Uint16Array (IntPtr js_handle) : base (js_handle)
		{ }

		/// <summary>
		/// From the specified segment.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="segment">Segment.</param>
		public static Uint16Array From (ArraySegment<ushort> segment)
		{
			var ta = new Uint16Array (segment.Count);
			ta.CopyFrom (segment);
			return ta;
		}

		/// <summary>
		/// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.Uint16Array"/>/>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="segment">ArraySegment</param>
		public static implicit operator Uint16Array (ArraySegment<ushort> segment) => From (segment);


		/// <summary>
		/// Defines an implicit conversion of an array to a <see cref="T:WebAssembly.Core.Uint16Array"/>./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="typedarray">Typedarray.</param>
		public static implicit operator ushort [] (Uint16Array typedarray)
		{
			return typedarray.ToArray ();
		}

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Uint16Array"/> to an array./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="managedArray">Managed array.</param>
		public static implicit operator Uint16Array (ushort [] managedArray)
		{
			return From (managedArray);
		}


	}
}
