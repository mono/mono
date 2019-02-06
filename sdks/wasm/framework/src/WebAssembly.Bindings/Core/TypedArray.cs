using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WebAssembly.Core {
	public abstract class TypedArray<T, U> : CoreObject, ITypedArray, ITypedArray<T, U> { 
		protected TypedArray () : base (Runtime.New<T> ())
		{
			Console.WriteLine ($"typedarray: {typeof(T).Name}");
		}
		protected TypedArray (int length) : base (Runtime.New<T> (length))
		{
			Console.WriteLine ($"typedarray: {typeof (T).Name} / {length}");
		}

		protected TypedArray (ArrayBuffer buffer) : base (Runtime.New<T> (buffer))
		{
			Console.WriteLine ($"typedarray: {typeof (T).Name} / {buffer}");
		}

		protected TypedArray (ArrayBuffer buffer, int byteOffset) : base (Runtime.New<T> (buffer, byteOffset))
		{
			Console.WriteLine ($"typedarray: {typeof (T).Name} / {buffer} -> {byteOffset}");
		}

		protected TypedArray (ArrayBuffer buffer, int byteOffset, int length) : base (Runtime.New<T> (buffer, byteOffset, length))
		{
			Console.WriteLine ($"typedarray: {typeof (T).Name} / {buffer} -> {byteOffset} - {length}");
		}

		internal TypedArray (IntPtr js_handle) : base (js_handle)
		{ }


		public int BytesPerElement => (int)GetObjectProperty ("BYTES_PER_ELEMENT");
		public string Name => (string)GetObjectProperty ("name");
		public int ByteLength => (int)GetObjectProperty ("byteLength");
		public ArrayBuffer Buffer => (ArrayBuffer)GetObjectProperty ("buffer");

		public void Fill (U value) => Invoke ("fill", value);
		public void Fill (U value, int start) => Invoke ("fill", value, start);
		public void Fill (U value, int start, int end) => Invoke ("fill", value, start, end);

		public void Set (Array array) => Invoke ("set", array);
		public void Set (Array array, int offset) => Invoke ("set", array, offset);
		public void Set (ITypedArray typedArray) => Invoke ("set", typedArray);
		public void Set (ITypedArray typedArray, int offset) => Invoke ("set", typedArray, offset);

		public T Slice () => (T)Invoke ("slice");
		public T Slice(int begin) => (T)Invoke ("slice", begin);
		public T Slice (int begin, int end) => (T)Invoke ("slice", begin, end);

		public T SubArray () => (T)Invoke ("subarray");
		public T SubArray (int begin) => (T)Invoke ("subarray", begin);
		public T SubArray (int begin, int end) => (T)Invoke ("subarray", begin, end);


		public U [] ToArray ()
		{
			var res = Runtime.TypedArrayToArray (JSHandle, out int exception);

			if (exception != 0)
				throw new JSException ((string)res);
			return (U [])res;
		}

		public static T From (U [] source)
		{
			// target array has to be instantiated.
			ValidateFromSource (source);

			// The following pins the location of the source object in memory
			// so that they will not be moved by garbage collection.
			GCHandle sourceHandle = GCHandle.Alloc (source, GCHandleType.Pinned);

			var res = Runtime.TypedArrayFromArray (source, out int exception);

			sourceHandle.Free ();

			return (T)res;
		}


		protected void ValidateTarget(U[] target)
		{
			// target array has to be instantiated.
			if (target == null || target.Length == 0) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (target)} can not be null and must have a length");
			}

		}

		protected void ValidateSource (U [] source)
		{
			// target array has to be instantiated.
			if (source == null || source.Length == 0) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (source)} can not be null and must have a length");
			}

		}


		protected static void ValidateFromSource (U [] source)
		{
			// target array has to be instantiated.
			if (source == null || source.Length == 0) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (source)} can not be null and must have a length");
			}

		}


	}
}
