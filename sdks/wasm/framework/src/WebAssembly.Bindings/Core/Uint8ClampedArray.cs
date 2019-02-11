using System;
using System.Runtime.InteropServices;

namespace WebAssembly.Core {
	public class Uint8ClampedArray : TypedArray<Uint8ClampedArray, byte> {
		public Uint8ClampedArray ()
		{ }

		public Uint8ClampedArray (int length) : base (length)
		{ }


		public Uint8ClampedArray (ArrayBuffer buffer) : base (buffer)
		{ }

		public Uint8ClampedArray (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset)
		{ }

		public Uint8ClampedArray (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length)
		{ }

		internal Uint8ClampedArray (IntPtr js_handle) : base (js_handle)
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
				return (int)((int)res / sizeof (byte));
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
				return (int)((int)res / sizeof (byte));
			}

		}


		/// <summary>
		/// Defines an implicit conversion of an array to a <see cref="T:WebAssembly.Core.Uint8ClampedArray"/>./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="typedarray">Typedarray.</param>
		public static implicit operator byte [] (Uint8ClampedArray typedarray)
		{
			return typedarray.ToArray ();
		}

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Uint8ClampedArray"/> to an array./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="managedArray">Managed array.</param>
		public static implicit operator Uint8ClampedArray (byte [] managedArray)
		{
			return From (managedArray);
		}


	}
}
