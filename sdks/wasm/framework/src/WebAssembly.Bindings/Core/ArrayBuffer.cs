using System;
namespace WebAssembly.Core {
	public class ArrayBuffer : CoreObject {

		/// <summary>
		/// Initializes a new instance of the <see cref="T:WebAssembly.Core.ArrayBuffer"/> class.
		/// </summary>
		public ArrayBuffer () : base (Runtime.New<ArrayBuffer> ())
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:WebAssembly.Core.ArrayBuffer"/> class.
		/// </summary>
		/// <param name="length">Length.</param>
		public ArrayBuffer (int length) : base (Runtime.New<ArrayBuffer> (length))
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:WebAssembly.Core.ArrayBuffer"/> class.
		/// </summary>
		/// <param name="js_handle">Js handle.</param>
		internal ArrayBuffer (IntPtr js_handle) : base (js_handle)
		{ }

		/// <summary>
		/// The length of an ArrayBuffer in bytes. 
		/// </summary>
		/// <value>The length of the underlying ArrayBuffer in bytes.</value>
		public int ByteLength => (int)GetObjectProperty ("byteLength");
		/// <summary>
		/// Gets a value indicating whether this <see cref="T:WebAssembly.Core.ArrayBuffer"/> is view.
		/// </summary>
		/// <value><c>true</c> if is view; otherwise, <c>false</c>.</value>
		public bool IsView => (bool)GetObjectProperty ("isView");
		/// <summary>
		/// Slice the specified begin.
		/// </summary>
		/// <returns>The slice.</returns>
		/// <param name="begin">Begin.</param>
		public ArrayBuffer Slice (int begin) => (ArrayBuffer)Invoke ("slice", begin);
		/// <summary>
		/// Slice the specified begin and end.
		/// </summary>
		/// <returns>The slice.</returns>
		/// <param name="begin">Begin.</param>
		/// <param name="end">End.</param>
		public ArrayBuffer Slice (int begin, int end) => (ArrayBuffer)Invoke ("slice", begin, end);

	}
}
