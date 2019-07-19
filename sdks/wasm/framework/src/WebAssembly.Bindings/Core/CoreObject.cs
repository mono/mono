using System;
namespace WebAssembly.Core {
	public abstract class CoreObject : JSObject {

		protected CoreObject (int js_handle) : base (js_handle, true)
		{
			var result = Runtime.BindCoreObject (js_handle, (int)(IntPtr)AnyRefHandle, out int exception);
			if (exception != 0)
				throw new JSException ($"CoreObject Error binding: {result.ToString ()}");

		}

		internal CoreObject (IntPtr js_handle, bool ownsHandle) : base (js_handle, ownsHandle)
		{ }
	}
}
