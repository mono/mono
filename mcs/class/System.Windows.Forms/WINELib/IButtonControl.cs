//
// System.Windows.Forms.IButtonControl.cs
//
// Author:
//   Dennis hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	public interface IButtonControl {
		void NotifyDefault(bool value);
		void PerformClick();
		DialogResult DialogResult {get; set;}
	}
}
