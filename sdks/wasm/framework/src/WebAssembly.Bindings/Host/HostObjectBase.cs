using System;
namespace WebAssembly.Host {
	public abstract class HostObjectBase : JSObject, IHostObject {

		protected HostObjectBase (int js_handle) : base (js_handle)
		{
			var result = Runtime.BindHostObject (js_handle, (int)(IntPtr)Handle, out int exception);
			if (exception != 0)
				throw new JSException ($"HostObject Error binding: {result.ToString ()}");

		}

		internal HostObjectBase (IntPtr js_handle) : base (js_handle)
		{ }
	}
}
