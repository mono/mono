using System;
namespace WebAssembly.Core {
	public sealed class Float32Array : TypedArray<Float32Array, float> {
		public Float32Array () { }

		public Float32Array (int length) : base (length) { }


		public Float32Array (ArrayBuffer buffer) : base (buffer) { }

		public Float32Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Float32Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Float32Array (IntPtr js_handle) : base (js_handle) { }

		/// <summary>
		/// From the specified segment.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="segment">Segment.</param>
		public static Float32Array From (ArraySegment<float> segment)
		{
			var ta = new Float32Array (segment.Count);
			ta.CopyFrom (segment);
			return ta;
		}

		/// <summary>
		/// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.Float32Array"/>/>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="segment">ArraySegment</param>
		public static implicit operator Float32Array (ArraySegment<float> segment) => From (segment);


		/// <summary>
		/// Defines an implicit conversion of an array to a <see cref="T:WebAssembly.Core.Float32Array"/>./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="typedarray">Typedarray.</param>
		public static implicit operator float [] (Float32Array typedarray)
		{
			return typedarray.ToArray ();
		}

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Float32Array"/> to an array./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="managedArray">Managed array.</param>
		public static implicit operator Float32Array (float [] managedArray)
		{
			return From (managedArray);
		}


	}
}
