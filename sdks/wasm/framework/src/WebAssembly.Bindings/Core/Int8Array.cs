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

		internal Int8Array (IntPtr js_handle) : base (js_handle)
		{ }

		/// <summary>
		/// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.Int8Array"/>/>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="segment">ArraySegment</param>
		public static implicit operator Int8Array (ArraySegment<sbyte> segment) => From (segment);


		/// <summary>
		/// Defines an implicit conversion of an array to a <see cref="T:WebAssembly.Core.Int8Array"/>./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="typedarray">Typedarray.</param>
		public static implicit operator sbyte [] (Int8Array typedarray) => typedarray.ToArray ();

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Int8Array"/> to an array./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="managedArray">Managed array.</param>
		public static implicit operator Int8Array (sbyte [] managedArray)
		{
			return From (managedArray);
		}


	}
}
