using System;
namespace WebAssembly.Core {
	public class Float32Array : TypedArray<Float32Array, float> {
		public Float32Array () { }

		public Float32Array (int length) : base (length) { }


		public Float32Array (ArrayBuffer buffer) : base (buffer) { }

		public Float32Array (ArrayBuffer buffer, int byteOffset) : base (buffer, byteOffset) { }

		public Float32Array (ArrayBuffer buffer, int byteOffset, int length) : base (buffer, byteOffset, length) { }

		internal Float32Array (IntPtr js_handle) : base (js_handle) { }


		// Define the indexer to allow client code to use [] notation.
		public float? this [int i] {
			get {
				var indexValue = Runtime.GetByIndex (JSHandle, i, out int exception);

				if (exception != 0)
					throw new JSException ((string)indexValue);

				if (indexValue != null)
					// The value returned from the index will be an int32 so use Convert to
					// return a byte value.  
					return (float)indexValue;
				else
					return null;
			}
			set {
				var res = Runtime.SetByIndex (JSHandle, i, value, out int exception);

				if (exception != 0)
					throw new JSException ((string)res);

			}
		}

		public unsafe int CopyTo (float [] target)
		{
			// target array has to be instantiated.
			ValidateTarget (target);

			fixed (float* pTarget = target) {
				var res = Runtime.TypedArrayCopyTo (JSHandle, (int)pTarget, target.Length, sizeof (float), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)((int)res / sizeof (float));
			}

		}

		public unsafe int CopyFrom (float [] target)
		{
			// target array has to be instantiated.
			ValidateTarget (target);

			fixed (float* pTarget = target) {
				var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)pTarget, target.Length, sizeof (float), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / sizeof (float);
			}

		}

	}
}
