using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WebAssembly.Core {
	/// <summary>
	/// Represents a JavaScript TypedArray.
	/// </summary>
	public abstract class TypedArray<T, U> : CoreObject, ITypedArray, ITypedArray<T, U> where U : struct {
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

		protected TypedArray (SharedArrayBuffer buffer) : base (Runtime.New<T> (buffer))
		{ }

		protected TypedArray (SharedArrayBuffer buffer, int byteOffset) : base (Runtime.New<T> (buffer, byteOffset))
		{ }

		protected TypedArray (SharedArrayBuffer buffer, int byteOffset, int length) : base (Runtime.New<T> (buffer, byteOffset, length))
		{ }

		internal TypedArray (IntPtr js_handle) : base (js_handle)
		{ }

		public TypedArrayTypeCode GetTypedArrayType()
		{
			switch (this) {
			case Int8Array _:
				return TypedArrayTypeCode.Int8Array;
			case Uint8Array _:
				return TypedArrayTypeCode.Uint8Array;
			case Uint8ClampedArray _:
				return TypedArrayTypeCode.Uint8ClampedArray;
			case Int16Array _:
				return TypedArrayTypeCode.Int16Array;
			case Uint16Array _:
				return TypedArrayTypeCode.Uint16Array;
			case Int32Array _:
				return TypedArrayTypeCode.Int32Array;
			case Uint32Array _:
				return TypedArrayTypeCode.Uint32Array;
			case Float32Array _:
				return TypedArrayTypeCode.Float32Array;
			case Float64Array _:
				return TypedArrayTypeCode.Float64Array;
			default:
				throw new ArrayTypeMismatchException ("TypedArray is not of correct type.");
			}
		}

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
		public T Slice (int begin) => (T)Invoke ("slice", begin);
		public T Slice (int begin, int end) => (T)Invoke ("slice", begin, end);

		public T SubArray () => (T)Invoke ("subarray");
		public T SubArray (int begin) => (T)Invoke ("subarray", begin);
		public T SubArray (int begin, int end) => (T)Invoke ("subarray", begin, end);

		/// <summary>
		/// Gets or sets the <see cref="T:WebAssembly.Core.TypedArray`2"/> with the specified i.
		/// </summary>
		/// <param name="i">The index.</param>
		public U? this [int i] {
			get {
				var jsValue = Runtime.GetByIndex (JSHandle, i, out int exception);

				if (exception != 0)
					throw new JSException ((string)jsValue);

				// The value returned from the index.
				return UnBoxValue (jsValue);
			}
			set {
				var res = Runtime.SetByIndex (JSHandle, i, value, out int exception);

				if (exception != 0)
					throw new JSException ((string)res);

			}
		}

		private U? UnBoxValue (object jsValue)
		{
			if (jsValue != null) {
				var type = jsValue.GetType ();
				if (type.IsPrimitive) {
					return (U)Convert.ChangeType (jsValue, typeof (U));
				} else {
					throw new InvalidCastException ($"Unable to cast object of type {type} to type {typeof (U)}.");
				}

			} else
				return null;
		}

		/// <summary>
		/// Copies the contents from the <see cref="T:WebAssembly.Core.TypedArray`2"/> memory into a new array.
		/// </summary>
		public U [] ToArray ()
		{
			var res = Runtime.TypedArrayToArray (JSHandle, out int exception);

			if (exception != 0)
				throw new JSException ((string)res);
			return (U [])res;
		}

		/// <summary>
		/// Creates a new <see cref="T:WebAssembly.Core.TypedArray`2"/> from the ReadOnlySpan.
		/// </summary>
		/// <returns>The new TypedArray/</returns>
		/// <param name="span">ReadOnlySpan.</param>
		public unsafe static T From (ReadOnlySpan<U> span)
		{
			// source has to be instantiated.
			ValidateFromSource (span);

			TypedArrayTypeCode type = (TypedArrayTypeCode)Type.GetTypeCode (typeof (U));
			// Special case for Uint8ClampedArray, a clamped array which represents an array of 8-bit unsigned integers clamped to 0-255;
			if (type == TypedArrayTypeCode.Uint8Array && typeof(T) == typeof(Uint8ClampedArray))
				type = TypedArrayTypeCode.Uint8ClampedArray;  // This is only passed to the JavaScript side so it knows it will be a Uint8ClampedArray

			var bytes = MemoryMarshal.AsBytes (span);
			fixed (byte* ptr = bytes) {
				var res = Runtime.TypedArrayFrom ((int)ptr, 0, span.Length, Marshal.SizeOf<U> (), (int)type, out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (T)res;
			}

		}

		/// <summary>
		/// Copies the underlying <see cref="T:WebAssembly.Core.TypedArray`2"/> data to a Span.
		/// </summary>
		/// <returns>Total copied.</returns>
		/// <param name="span">Span.</param>
		public unsafe int CopyTo (Span<U> span)
		{
			var bytes = MemoryMarshal.AsBytes (span);
			fixed (byte* ptr = bytes) {
				var res = Runtime.TypedArrayCopyTo (JSHandle, (int)ptr, 0, span.Length, Marshal.SizeOf<U> (), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / Marshal.SizeOf<U> ();
			}
		}

		private unsafe int CopyFrom (void* ptrSource, int offset, int count)
		{
			var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)ptrSource, offset, offset + count, Marshal.SizeOf<U> (), out int exception);
			if (exception != 0)
				throw new JSException ((string)res);
			return (int)res / Marshal.SizeOf<U> ();

		}

		/// <summary>
		/// Copies from a <see cref="T:Span{T}"/> to the <see cref="T:WebAssembly.Core.TypedArray`2"/> memory.
		/// </summary>
		/// <returns>Total copied</returns>
		/// <param name="span">ReadOnlySpan.</param>
		public unsafe int CopyFrom (ReadOnlySpan<U> span)
		{
			ValidateSource (span);
			var bytes = MemoryMarshal.AsBytes (span);
			fixed (byte* ptr = bytes) {
				var res = Runtime.TypedArrayCopyFrom (JSHandle, (int)ptr, 0, span.Length, Marshal.SizeOf<U> (), out int exception);
				if (exception != 0)
					throw new JSException ((string)res);
				return (int)res / Marshal.SizeOf<U> ();
			}
		}

		protected void ValidateTarget (Span<U> target)
		{
			// target array has to be instantiated.
			if (target == null || target.Length == 0) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (target)} can not be null and must have a length");
			}

		}

		protected void ValidateSource (ReadOnlySpan<U> source)
		{
			// target has to be instantiated.
			if (source == null || source.Length == 0) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (source)} can not be null and must have a length");
			}

		}

		protected static void ValidateFromSource (ReadOnlySpan<U> source)
		{
			// target array has to be instantiated.
			if (source == null) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (source)} can not be null.");
			}

		}


		protected static void ValidateFromSource (U [] source, int offset, int count)
		{
			// target array has to be instantiated.
			if (source == null) {
				throw new System.ArgumentException ($"Invalid argument: {nameof (source)} can not be null.");
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
