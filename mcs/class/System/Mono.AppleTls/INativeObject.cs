using System;

namespace ObjCRuntimeInternal {

	internal interface INativeObject {
		IntPtr Handle { 
			get;
		}
	}

	static class NativeObjectHelper {

		// help to avoid the (too common pattern)
		// 	var p = x == null ? IntPtr.Zero : x.Handle;
		static public IntPtr GetHandle (this INativeObject self)
		{
			return self == null ? IntPtr.Zero : self.Handle;
		}
	}

}
