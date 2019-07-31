﻿using System;
namespace WebAssembly.Core {
	public abstract class CoreObject : JSObject {

		protected CoreObject (int js_handle) : base (js_handle)
		{
			var result = Runtime.BindCoreObject (js_handle, (int)(IntPtr)Handle, out int exception);
			if (exception != 0)
				throw new JSException ($"CoreObject Error binding: {result.ToString ()}");

		}

		internal CoreObject (IntPtr js_handle) : base (js_handle)
		{ }
	}
}
