using System;
namespace WebAssembly.Core {
	public class Int8Array : TypedArray<Int8Array, sbyte> {
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


		// Define the indexer to allow client code to use [] notation.
		public sbyte? this [int i] {
			get {
				var indexValue = Runtime.GetByIndex (JSHandle, i, out int exception);

				if (exception != 0)
					throw new JSException ((string)indexValue);

				if (indexValue != null)
					// The value returned from the index will be an int32 so use Convert to
					// return a byte value.  
					return Convert.ToSByte (indexValue);
				else
					return null;
			}
			set {
				var res = Runtime.SetByIndex (JSHandle, i, value, out int exception);

				if (exception != 0)
					throw new JSException ((string)res);

			}
		}

		public unsafe int CopyTo (sbyte [] target)
		{
			// target array has to be instantiated.
			ValidateTarget (target);

			// The following fixed statement pins the location of the target object in memory
			// so that they will not be moved by garbage collection.
			fixed (sbyte* pTarget = target) {
				var res = Runtime.TypedArrayCopyTo (JSHandle, (int)pTarget, target.Length, sizeof (sbyte), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)((int)res / sizeof (sbyte));
			}

		}

		public unsafe int CopyFrom (sbyte [] source)
		{
			// target array has to be instantiated.
			ValidateSource (source);

			// The following fixed statement pins the location of the target object in memory
			// so that they will not be moved by garbage collection.
			fixed (sbyte* pTarget = source) {
				var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)pTarget, source.Length, sizeof (sbyte), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)((int)res / sizeof (sbyte));
			}

		}

		/// <summary>
		/// Copies from an array to a <see cref="T:WebAssembly.Core.Int8Array"/>/>.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="source">Source.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="count">Count.</param>
		public unsafe int CopyFrom (sbyte [] source, int offset, int count)
		{
			// target array has to be instantiated.
			ValidateSource (source, offset, count);

			// The following fixed statement pins the location of the target object in memory
			// so that they will not be moved by garbage collection.
			fixed (sbyte* pTarget = &source [offset]) {

				var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)pTarget, count, sizeof (sbyte), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / sizeof (sbyte);
			}

		}

		/// <summary>
		/// Copies from <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.Int8Array"/>/>.
		/// </summary>
		/// <returns>The number of bytes copied.</returns>
		/// <param name="source">Source.</param>
		public int CopyFrom (ArraySegment<sbyte> source) => CopyFrom (source.Array, source.Offset, source.Count);

		/// <summary>
		/// Defines an implicit conversion of an array to a <see cref="T:WebAssembly.Core.Int8Array"/>./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="typedarray">Typedarray.</param>
		public static implicit operator sbyte [] (Int8Array typedarray)
		{
			return typedarray.ToArray ();
		}

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
