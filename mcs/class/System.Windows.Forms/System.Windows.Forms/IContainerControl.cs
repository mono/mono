//
// System.Windows.Forms.IContainerControl.cs
//
// Author:
// William Lamb (wdlamb@notwires.com)
// Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

namespace System.Windows.Forms {

	public interface IContainerControl {

		bool ActivateControl(Control active);
		Control ActiveControl {get; set;}
	}
}
