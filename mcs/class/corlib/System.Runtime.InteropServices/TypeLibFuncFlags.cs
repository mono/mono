// System.Runtime.InteropServices.TypeLibFuncFlags.cs
// 
// Name: Duncan Mak  (duncan@ximian.com)
// 
// (C) Ximian, Inc.
// 

namespace System.Runtime.InteropServices {
	[Flags] [Serializable]
	public enum TypeLibFuncFlags {
		FRestricted = 1,
		FSource = 2,
		FBindable = 4,
		FRequestEdit = 8,
		FDisplayBind = 16,
		FDefaultBind = 32,
		FHidden = 64,
		FUsesGetLastError = 128,
		FDefaultCollelem = 256,
		FUiDefault = 512,
		FNonBrowsable = 1024,
		FReplaceable = 2048,
		FImmediateBind = 4096,
	}
}
