//
// System.ComponentModel.DesignerSerializationVisibility.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

using System.Runtime.InteropServices;

namespace System.ComponentModel {

	[ComVisible (true)]
	public enum DesignerSerializationVisibility {
		Hidden, Visible, Content
	}
}
