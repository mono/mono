//
// System.Windows.Forms.IWin32Window.cs
//
// Author:
// William Lamb (wdlamb@notwires.com)
// Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

namespace System.Windows.Forms {

	[ComVisible(true)]
	// FixMe [Guid("")]
	// FixMe [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWin32Window {

		IntPtr Handle {get;}
	}
}
