using System;
using System.Runtime.InteropServices;

namespace WebAssembly.Core {
	public class Uint8Array : TypedArray<Uint8Array, byte> {

		/// <summary>
		/// Initializes a new instance of the <see cref="T:WebAssembly.Core.Uint8Array"/> class.
		/// </summary>
		public Uint8Array ()
		{ }

		public Uint8Array (int length) : base (length)
		{ }


		public Uint8Array (ArrayBuffer buffer) : base (buffer)
		{ }

		public Uint8Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset)
		{ }

		public Uint8Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length)
		{ }

		internal Uint8Array (IntPtr js_handle) : base (js_handle)
		{ }


		// Define the indexer to allow client code to use [] notation.
		public byte? this [int i] {
			get {
				var indexValue = Runtime.GetByIndex (JSHandle, i, out int exception);

				if (exception != 0)
					throw new JSException ((string)indexValue);

				if (indexValue != null)
					// The value returned from the index will be an int32 so use Convert to
					// return a byte value.  
					return Convert.ToByte (indexValue);
				else
					return null;
			}
			set {
				var res = Runtime.SetByIndex (JSHandle, i, value, out int exception);

				if (exception != 0)
					throw new JSException ((string)res);

			}
		}

		public unsafe int CopyTo (byte [] target)
		{
			// target array has to be instantiated.
			ValidateTarget (target);

			// The following fixed statement pins the location of the target object in memory
	    		// so that they will not be moved by garbage collection.
			fixed (byte* pTarget = target) {
				var res = Runtime.TypedArrayCopyTo (JSHandle, (int)pTarget, target.Length, sizeof (byte), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / sizeof (byte);
			}

		}

		public unsafe int CopyFrom (byte [] source)
		{
			// target array has to be instantiated.
			ValidateSource (source);

			// The following fixed statement pins the location of the target object in memory
			// so that they will not be moved by garbage collection.
			fixed (byte* pTarget = source) {
				var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)pTarget, source.Length, sizeof (byte), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / sizeof (byte);
			}

		}

		/// <summary>
		/// Copies from an array to a <see cref="T:WebAssembly.Core.Uint8Array"/>/>.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="source">Source.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="count">Count.</param>
		public unsafe int CopyFrom (byte [] source, int offset, int count)
		{
			// target array has to be instantiated.
			ValidateSource (source, offset, count);

			// The following fixed statement pins the location of the target object in memory
			// so that they will not be moved by garbage collection.
			fixed (byte* pTarget = &source[offset]) {

				var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)pTarget, count, sizeof (byte), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / sizeof (byte);
			}

		}

		/// <summary>
		/// Copies from <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.Uint8Array"/>/>.
		/// </summary>
		/// <returns>The number of bytes copied.</returns>
		/// <param name="source">Source.</param>
		public int CopyFrom (ArraySegment<byte> source) => CopyFrom (source.Array, source.Offset, source.Count);

		/// <summary>
		/// Defines an implicit conversion of an array to a <see cref="T:WebAssembly.Core.Uint8Array"/>./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="typedarray">Typedarray.</param>
		public static implicit operator byte [] (Uint8Array typedarray) => typedarray.ToArray();

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Uint8Array"/> to an array./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="managedArray">Managed array.</param>
		public static implicit operator Uint8Array (byte[] managedArray) => From(managedArray);

		/// <summary>
		/// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.Uint8Array"/>/>
		/// </summary>
		//public static implicit operator Uint8Array(ArraySegment<byte> segment) => CopyFrom(segment.Array, segment.Offset, segment.Count);

	}
}
