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

	}
}
