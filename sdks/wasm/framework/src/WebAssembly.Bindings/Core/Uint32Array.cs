using System;
namespace WebAssembly.Core {
	public class Uint32Array : TypedArray<Uint32Array, uint> {
		public Uint32Array () { }

		public Uint32Array (int length) : base (length) { }


		public Uint32Array (ArrayBuffer buffer) : base (buffer) { }

		public Uint32Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Uint32Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Uint32Array (IntPtr js_handle) : base (js_handle) { }


		// Define the indexer to allow client code to use [] notation.
		public uint? this [int i] {
			get {
				var indexValue = Runtime.GetByIndex (JSHandle, i, out int exception);

				if (exception != 0)
					throw new JSException ((string)indexValue);

				if (indexValue != null)
					// The value returned from the index will be an int32 so use Convert to
					// return a byte value.  
					return (uint)indexValue;
				else
					return null;
			}
			set {
				var res = Runtime.SetByIndex (JSHandle, i, value, out int exception);

				if (exception != 0)
					throw new JSException ((string)res);

			}
		}

		public unsafe int CopyTo (uint [] target)
		{
			// target array has to be instantiated.
			ValidateTarget (target);

			fixed (uint* pTarget = target) {
				var res = Runtime.TypedArrayCopyTo (JSHandle, (int)pTarget, target.Length, sizeof (uint), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)((int)res / sizeof (uint));
			}

		}

		public unsafe int CopyFrom (uint [] target)
		{
			// target array has to be instantiated.
			ValidateTarget (target);

			fixed (uint* pTarget = target) {
				var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)pTarget, target.Length, sizeof (uint), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / sizeof (uint);
			}

		}

		/// <summary>
		/// Copies from an array to a <see cref="T:WebAssembly.Core.Uint32Array"/>/>.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="source">Source.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="count">Count.</param>
		public unsafe int CopyFrom (uint [] source, int offset, int count)
		{
			// target array has to be instantiated.
			ValidateSource (source, offset, count);

			// The following fixed statement pins the location of the target object in memory
			// so that they will not be moved by garbage collection.
			fixed (uint* pTarget = &source [offset]) {

				var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)pTarget, count, sizeof (uint), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / sizeof (uint);
			}

		}

		/// <summary>
		/// Copies from <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.Uint32Array"/>/>.
		/// </summary>
		/// <returns>The number of bytes copied.</returns>
		/// <param name="source">Source.</param>
		public int CopyFrom (ArraySegment<uint> source) => CopyFrom (source.Array, source.Offset, source.Count);


		/// <summary>
		/// Defines an implicit conversion of an array to a <see cref="T:WebAssembly.Core.Uint32Array"/>./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="typedarray">Typedarray.</param>
		public static implicit operator uint [] (Uint32Array typedarray)
		{
			return typedarray.ToArray ();
		}

		/// <summary>
		/// Defines an implicit conversion of <see cref="T:WebAssembly.Core.Uint32Array"/> to an array./>
		/// </summary>
		/// <returns>The implicit.</returns>
		/// <param name="managedArray">Managed array.</param>
		public static implicit operator Uint32Array (uint [] managedArray)
		{
			return From (managedArray);
		}


	}
}
