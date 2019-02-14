using System;

namespace WebAssembly.Core {
	public sealed class Int32Array : TypedArray<Int32Array, int> {
		public Int32Array () { }

		public Int32Array (int length) : base (length) { }


		public Int32Array (ArrayBuffer buffer) : base (buffer) { }

		public Int32Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Int32Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Int32Array (IntPtr js_handle) : base (js_handle) { }

		/// <summary>
		/// From the specified segment.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="segment">Segment.</param>
		public static Int32Array From (ArraySegment<int> segment)
		{
			var ta = new Int32Array (segment.Count);
			ta.CopyFrom (segment);
			return ta;
		}

		/// <summary>
		/// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.Int32Array"/>/>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="segment">ArraySegment</param>
		public static implicit operator Int32Array (ArraySegment<int> segment) => From (segment);


		/// <summary>
		/// Defines an implicit conversion of an array to a <see cref="T:WebAssembly.Core.Int32Array"/>./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="typedarray">Typedarray.</param>
		public static implicit operator int [] (Int32Array typedarray)
		{
			return typedarray.ToArray ();
		}

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Int32Array"/> to an array./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="managedArray">Managed array.</param>
		public static implicit operator Int32Array (int [] managedArray)
		{
			return From (managedArray);
		}


	}
}
