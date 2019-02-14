using System;
namespace WebAssembly.Core {
	public sealed class Int16Array : TypedArray<Int16Array, short> {
		public Int16Array () { }

		public Int16Array (int length) : base (length) { }


		public Int16Array (ArrayBuffer buffer) : base (buffer) { }

		public Int16Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Int16Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Int16Array (IntPtr js_handle) : base (js_handle)
		{ }

		/// <summary>
		/// From the specified segment.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="segment">Segment.</param>
		public static Int16Array From (ArraySegment<short> segment)
		{
			var ta = new Int16Array (segment.Count);
			ta.CopyFrom (segment);
			return ta;
		}

		/// <summary>
		/// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.Int16Array"/>/>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="segment">ArraySegment</param>
		public static implicit operator Int16Array (ArraySegment<short> segment) => From (segment);


		/// <summary>
		/// Defines an implicit conversion of an array to a <see cref="T:WebAssembly.Core.Int16Array"/>./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="typedarray">Typedarray.</param>
		public static implicit operator short [] (Int16Array typedarray)
		{
			return typedarray.ToArray ();
		}

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Int16Array"/> to an array./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="managedArray">Managed array.</param>
		public static implicit operator Int16Array (short [] managedArray)
		{
			return From (managedArray);
		}


	}
}
