using System;
namespace WebAssembly.Core {
	public class Int16Array : TypedArray<Int16Array, short> {
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

		public unsafe int CopyFrom (short [] target)
		{
			// target array has to be instantiated.
			ValidateTarget (target);

			fixed (short* pTarget = target) {
				var res = Runtime.TypedArrayCopyTo (JSHandle, (int)pTarget, target.Length, sizeof (short), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / sizeof (short);
			}

		}

		/// <summary>
		/// Copies from an array to a <see cref="T:WebAssembly.Core.Int16Array"/>/>.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="source">Source.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="count">Count.</param>
		public unsafe int CopyFrom (short [] source, int offset, int count)
		{
			// target array has to be instantiated.
			ValidateSource (source, offset, count);

			// The following fixed statement pins the location of the target object in memory
			// so that they will not be moved by garbage collection.
			fixed (short* pTarget = &source [offset]) {

				var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)pTarget, count, sizeof (short), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / sizeof (short);
			}

		}

		/// <summary>
		/// Copies from <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.Int16Array"/>/>.
		/// </summary>
		/// <returns>The number of bytes copied.</returns>
		/// <param name="source">Source.</param>
		public int CopyFrom (ArraySegment<short> source) => CopyFrom (source.Array, source.Offset, source.Count);

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
