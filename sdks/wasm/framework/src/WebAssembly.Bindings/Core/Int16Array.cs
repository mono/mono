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


		// Define the indexer to allow client code to use [] notation.
		public int? this [int i] {
			get {
				var indexValue = Runtime.GetByIndex (JSHandle, i, out int exception);

				if (exception != 0)
					throw new JSException ((string)indexValue);

				if (indexValue != null)
					// The value returned from the index will be an int32 so use Convert to
					// return a byte value.  
					return (int)indexValue;
				else
					return null;
			}
			set {
				var res = Runtime.SetByIndex (JSHandle, i, value, out int exception);

				if (exception != 0)
					throw new JSException ((string)res);

			}
		}

		public unsafe int CopyTo (short [] target)
		{
			// target array has to be instantiated.
			ValidateTarget (target);

			fixed (short* pTarget = target) {
				var res = Runtime.TypedArrayCopyTo (JSHandle, (int)pTarget, target.Length, sizeof (short), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)((int)res / sizeof (short));
			}

		}

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
