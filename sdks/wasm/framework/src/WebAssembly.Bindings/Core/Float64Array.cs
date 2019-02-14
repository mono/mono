using System;
namespace WebAssembly.Core {
	public sealed class Float64Array : TypedArray<Float64Array, double> {
		public Float64Array () { }

		public Float64Array (int length) : base (length) { }


		public Float64Array (ArrayBuffer buffer) : base (buffer) { }

		public Float64Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Float64Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Float64Array (IntPtr js_handle) : base (js_handle) { }

		/// <summary>
		/// From the specified segment.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="segment">Segment.</param>
		public static Float64Array From (ArraySegment<double> segment)
		{
			var ta = new Float64Array (segment.Count);
			ta.CopyFrom (segment);
			return ta;
		}

		/// <summary>
		/// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.Float64Array"/>/>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="segment">ArraySegment</param>
		public static implicit operator Float64Array (ArraySegment<double> segment) => From (segment);


		/// <summary>
		/// Defines an implicit conversion of an array to a <see cref="T:WebAssembly.Core.Float64Array"/>./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="typedarray">Typedarray.</param>
		public static implicit operator double [] (Float64Array typedarray)
		{
			return typedarray.ToArray ();
		}

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Float64Array"/> to an array./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="managedArray">Managed array.</param>
		public static implicit operator Float64Array (double [] managedArray)
		{
			return From (managedArray);
		}


	}
}
