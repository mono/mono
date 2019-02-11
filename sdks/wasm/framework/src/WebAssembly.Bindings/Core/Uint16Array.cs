using System;
namespace WebAssembly.Core {
	public class Uint16Array : TypedArray<Uint16Array, ushort> {
		public Uint16Array () { }

		public Uint16Array (int length) : base (length) { }


		public Uint16Array (ArrayBuffer buffer) : base (buffer) { }

		public Uint16Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Uint16Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Uint16Array (IntPtr js_handle) : base (js_handle)
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

		public unsafe int CopyTo (ushort [] target)
		{
			// target array has to be instantiated.
			ValidateTarget (target);

			fixed (ushort* pTarget = target) {
				var res = Runtime.TypedArrayCopyTo (JSHandle, (int)pTarget, target.Length, sizeof (short), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)((int)res / sizeof (short));
			}

		}

		public unsafe int CopyFrom (ushort [] target)
		{
			// target array has to be instantiated.
			ValidateTarget (target);

			fixed (ushort* pTarget = target) {
				var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)pTarget, target.Length, sizeof (ushort), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / sizeof (ushort);
			}

		}


		/// <summary>
		/// Defines an implicit conversion of an array to a <see cref="T:WebAssembly.Core.Uint16Array"/>./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="typedarray">Typedarray.</param>
		public static implicit operator ushort [] (Uint16Array typedarray)
		{
			return typedarray.ToArray ();
		}

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Uint16Array"/> to an array./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="managedArray">Managed array.</param>
		public static implicit operator Uint16Array (ushort [] managedArray)
		{
			return From (managedArray);
		}


	}
}
