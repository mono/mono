using System;
namespace WebAssembly.Host {
	public abstract class HostObjectBase : JSObject, IHostObject {

		protected HostObjectBase (int js_handle) : base (js_handle, true)
		{
			var result = Runtime.BindHostObject (js_handle, (int)(IntPtr)AnyRefHandle, out int exception);
			if (exception != 0)
				throw new JSException ($"HostObject Error binding: {result.ToString ()}");

		}

		internal HostObjectBase (IntPtr js_handle, bool ownsHandle) : base (js_handle, ownsHandle)
		{ }
	}
}
