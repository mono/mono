//
// System.Windows.Forms.IWindowTarget.cs
//
// Author:
// William Lamb (wdlamb@notwires.com)
// Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

namespace System.Windows.Forms {

	public interface IWindowTarget {

		// There is no documentation for this interface's members!
		// Only a note saying that it supports the .NET infrastructure
		// and is not intended to be used directly from your code.
		// The following methods had their own listing in the documentation;
		// I don't know what other methods and properties there may be.

		void OnHandleChange(IntPtr newHandle);
		void OnMessage(ref Message m);
	}
}

