// System.Runtime.InteropServices.TypeLibVarFlags.cs
// 
// Name: Duncan Mak  (duncan@ximian.com)
// 
// (C) Ximian, Inc.
// 

namespace System.Runtime.InteropServices {
	
	[Flags] [Serializable]
	public enum TypeLibVarFlags {
		FReadOnly = 1,
		FSource = 2,
		FBindable = 4,
		FRequestEdit = 8,
		FDisplayBind = 16,
		FDefaultBind = 32,
		FHidden = 64,
		FRestricted = 128,
		FDefaultCollelem = 256,
		FUiDefault = 512,
		FNonBrowable = 1024,
		FReplaceable = 2048,
		FImmediateBind = 4096,
	}
}
