using System;
using System.Runtime.InteropServices;

namespace WebAssembly {

	/// <summary>
	///   JSObjects are wrappers for a native JavaScript object, and
	///   they retain a reference to the JavaScript object for the lifetime of this C# object.
	/// </summary>
	public class JSObject : IDisposable {
		public int JSHandle { get; internal set; }
		internal GCHandle Handle;
		internal object RawObject;
		// to detect redundant calls
		public bool IsDisposed { get; internal set; }

		internal JSObject (int js_handle)
		{
			this.JSHandle = js_handle;
			this.Handle = GCHandle.Alloc (this);
		}

		internal JSObject (int js_id, object raw_obj)
		{
			this.JSHandle = js_id;
			this.Handle = GCHandle.Alloc (this);
			this.RawObject = raw_obj;
		}


		/// <returns>
		///   <para>
		///     The return value can either be a primitive (string, int, double), a <see
		///     <see cref="T:WebAssembly.JSObject"/> for JavaScript objects, a 
		///     <see cref="T:System.Threading.Tasks.Task"/>(object) for JavaScript promises, an array of
		///     a byte, int or double (for Javascript objects typed as ArrayBuffer) or a 
		///     <see cref="T:System.Func"/> to represent JavaScript functions.  The specific version of
		///     the Func that will be returned depends on the parameters of the Javascript function
		///     and return value.
		///   </para>
		///   <para>
		///     The value of a returned promise (The Task(object) return) can in turn be any of the above
		///     valuews.
		///   </para>
		/// </returns>
		public object Invoke (string method, params object [] args)
		{
			var res = Runtime.InvokeJSWithArgs (JSHandle, method, args, out int exception);
			if (exception != 0)
				throw new JSException ((string)res);
			return res;
		}

		/// <summary>
		///   Returns the named property from the object, or throws a JSException on error.
		/// </summary>
		/// <param name="name">The name of the property to lookup</param>
		/// <remarks>
		///   This method can raise a <see cref="T:WebAssembly.JSException"/> if fetching the property in Javascript raises an exception.
		/// </remarks>
		/// <returns>
		///   <para>
		///     The return value can either be a primitive (string, int, double), a 
		///     <see cref="T:WebAssembly.JSObject"/> for JavaScript objects, a 
		///     <see cref="T:System.Threading.Tasks.Task"/>(object) for JavaScript promises, an array of
		///     a byte, int or double (for Javascript objects typed as ArrayBuffer) or a 
		///     <see cref="T:System.Func"/> to represent JavaScript functions.  The specific version of
		///     the Func that will be returned depends on the parameters of the Javascript function
		///     and return value.
		///   </para>
		///   <para>
		///     The value of a returned promise (The Task(object) return) can in turn be any of the above
		///     valuews.
		///   </para>
		/// </returns>
		public object GetObjectProperty (string name)
		{

			var propertyValue = Runtime.GetObjectProperty (JSHandle, name, out int exception);

			if (exception != 0)
				throw new JSException ((string)propertyValue);

			return propertyValue;

		}

		/// <summary>
		///   Sets the named property to the provided value.
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <param name="name">The name of the property to lookup</param>
		/// <param name="value">The value can be a primitive type (int, double, string, bool), an
		/// array that will be surfaced as a typed ArrayBuffer (byte[], sbyte[], short[], ushort[],
		/// float[], double[]) </param>
		/// <param name="createIfNotExists">Defaults to <see langword="true"/> and creates the property on the javascript object if not found, if set to <see langword="false"/> it will not create the property if it does not exist.  If the property exists, the value is updated with the provided value.</param>
		/// <param name="hasOwnProperty"></param>
		public void SetObjectProperty (string name, object value, bool createIfNotExists = true, bool hasOwnProperty = false)
		{

			var setPropResult = Runtime.SetObjectProperty (JSHandle, name, value, createIfNotExists, hasOwnProperty, out int exception);
			if (exception != 0)
				throw new JSException ($"Error setting {name} on (js-obj js '{JSHandle}' mono '{(IntPtr)Handle} raw '{RawObject != null})");

		}

		protected void FreeHandle ()
		{
			Runtime.ReleaseHandle (JSHandle, out int exception);
			if (exception != 0)
				throw new JSException ($"Error releasing handle on (js-obj js '{JSHandle}' mono '{(IntPtr)Handle} raw '{RawObject != null})");
		}

		public override bool Equals (System.Object obj)
		{
			if (obj == null || GetType () != obj.GetType ()) {
				return false;
			}
			return JSHandle == (obj as JSObject).JSHandle;
		}

		public override int GetHashCode ()
		{
			return JSHandle;
		}

		~JSObject ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			// Dispose of unmanaged resources.
			Dispose (true);
			// Suppress finalization.
			GC.SuppressFinalize (this);
		}

		// Protected implementation of Dispose pattern.
		protected virtual void Dispose (bool disposing)
		{

			if (!IsDisposed) {
				if (disposing) {

					// Free any other managed objects here.
					//
					RawObject = null;
				}

				IsDisposed = true;

				// Free any unmanaged objects here.
				FreeHandle ();

			}
		}

		public override string ToString ()
		{
			return $"(js-obj js '{JSHandle}' mono '{(IntPtr)Handle} raw '{RawObject != null})";
		}

	}
}
