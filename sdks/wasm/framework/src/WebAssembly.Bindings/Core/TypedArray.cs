using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WebAssembly.Core {
	///
	public abstract class TypedArray<T, U> : CoreObject, ITypedArray, ITypedArray<T, U> { 
		protected TypedArray () : base (Runtime.New<T> ())
		{ }
		protected TypedArray (int length) : base (Runtime.New<T> (length))
		{ }

		protected TypedArray (ArrayBuffer buffer) : base (Runtime.New<T> (buffer))
		{ }

		protected TypedArray (ArrayBuffer buffer, int byteOffset) : base (Runtime.New<T> (buffer, byteOffset))
		{ }

		protected TypedArray (ArrayBuffer buffer, int byteOffset, int length) : base (Runtime.New<T> (buffer, byteOffset, length))
		{ }

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

		private unsafe int CopyTo (void* ptrTarget, int offset, int count)
		{
			var res = Runtime.TypedArrayCopyTo (JSHandle, (int)ptrTarget, offset, offset + count, Marshal.SizeOf<U> (), out int exception);
			if (exception != 0)
				throw new JSException ((string)res);
			return (int)res / Marshal.SizeOf<U> ();

		}

		/// <summary>
		/// Copies from a <see cref="T:WebAssembly.Core.TypedArray`2"/> to <see langword="async"/> memory address.
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="target">Target.</param>
		/// <param name="count">Count.</param>
		public unsafe int CopyTo (IntPtr target, int count) => CopyTo (target.ToPointer (), 0, count);

		/// <summary>
		/// Copies from a <see cref="T:WebAssembly.Core.TypedArray`2"/> to an array
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="target">Source.</param>
		public int CopyTo (U [] target) => CopyTo (target, 0, target.Length);

		/// <summary>
		/// Copies from a <see cref="T:WebAssembly.Core.TypedArray`2"/> to an array
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="target">Target.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="count">Count.</param>
		public unsafe int CopyTo (U [] target, int offset, int count)
		{
			// target array has to be instantiated.
			ValidateSource (target, offset, count);

			// The following pins the location of the source object in memory
			// so that they will not be moved by garbage collection.
			GCHandle sourceHandle = GCHandle.Alloc (target, GCHandleType.Pinned);

			try {
				var ptr = sourceHandle.AddrOfPinnedObject ();
				return CopyTo (ptr.ToPointer (), offset, count);

			} finally {
				sourceHandle.Free ();
			}
		}

		/// <summary>
		/// Copies from a <see cref="T:WebAssembly.Core.TypedArray`2"/> to an <see cref="ArraySegment{T}"/>
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="target">Target.</param>
		public int CopyTo (ArraySegment<U> target) => CopyTo (target.Array, target.Offset, target.Count);

		private unsafe int CopyFrom (void* ptrSource, int offset, int count)
		{
			var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)ptrSource, offset, offset + count, Marshal.SizeOf<U>(), out int exception);
			if (exception != 0)
				throw new JSException ((string)res);
			return (int)res / Marshal.SizeOf<U> ();

		}

		/// <summary>
		/// Copies from a memory address to a <see cref="T:WebAssembly.Core.TypedArray`2"/>.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="source">Source.</param>
		/// <param name="count">Count.</param>
		public unsafe int CopyFrom (IntPtr source, int count) => CopyFrom (source.ToPointer(), 0, count);

		/// <summary>
		/// Copies from an array to a <see cref="T:WebAssembly.Core.TypedArray`2"/>
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="source">Source.</param>
		public int CopyFrom (U [] source) => CopyFrom (source, 0, source.Length);

		/// <summary>
		/// Copies from an array to a <see cref="T:WebAssembly.Core.TypedArray`2"/>.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="source">Source.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="count">Count.</param>
		public unsafe int CopyFrom (U [] source, int offset, int count)
		{
			// target array has to be instantiated.
			ValidateSource (source, offset, count);

			// The following pins the location of the source object in memory
			// so that they will not be moved by garbage collection.
			GCHandle sourceHandle = GCHandle.Alloc (source, GCHandleType.Pinned);

			try {
				var ptr = sourceHandle.AddrOfPinnedObject ();
				return CopyFrom (ptr.ToPointer(), offset, count);

			} finally {
				sourceHandle.Free ();
			}
		}

		/// <summary>
		/// Copies from <see cref="ArraySegment{T}"/> to a <see cref="T:WebAssembly.Core.TypedArray`2"/>.
		/// </summary>
		/// <returns>The number of bytes copied.</returns>
		/// <param name="source">Source.</param>
		public int CopyFrom (ArraySegment<U> source) => CopyFrom (source.Array, source.Offset, source.Count);

		protected void ValidateTarget(U[] target)
		{
			// target array has to be instantiated.
			if (target == null || target.Length == 0) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (target)} can not be null and must have a length");
			}

		}

		protected void ValidateTarget (U [] target, int offset, int count)
		{
			ValidateTarget (target);
			// offset can not be past the end of the array
			if (offset > target.Length) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (offset)} can not be greater than length of '{nameof(target)}'");
			}
			// offset plus count can not pass the end of the array.
			if (offset + count > target.Length) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (offset)} plus {nameof(count)} can not be greater than length of '{nameof (target)}'");
			}

		}

		protected void ValidateSource (U [] source)
		{
			// target array has to be instantiated.
			if (source == null || source.Length == 0) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (source)} can not be null and must have a length");
			}

		}

		protected void ValidateSource (U [] source, int offset, int count)
		{
			ValidateSource (source);
			// offset can not be past the end of the array
			if (offset > source.Length) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (offset)} can not be greater than length of '{nameof (source)}'");
			}
			// offset plus count can not pass the end of the array.
			if (offset + count > source.Length) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (offset)} plus {nameof (count)} can not be greater than length of '{nameof (source)}'");
			}

		}

		protected static void ValidateFromSource (U [] source)
		{
			// target array has to be instantiated.
			if (source == null || source.Length == 0) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (source)} can not be null and must have a length");
			}

		}


		protected static void ValidateFromSource (U [] source, int offset, int count)
		{
			// target array has to be instantiated.
			if (source == null || source.Length == 0) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (source)} can not be null and must have a length");
			}

			// offset can not be past the end of the array
			if (offset > source.Length) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (offset)} can not be greater than length of '{nameof (source)}'");
			}
			// offset plus count can not pass the end of the array.
			if (offset + count > source.Length) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (offset)} plus {nameof (count)} can not be greater than length of '{nameof (source)}'");
			}

		}

	}
}
