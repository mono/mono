//
// System.ComponentModel.Design.SelectionTypes
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[Flags]
	[Serializable]
	[ComVisible(true)]
        public enum SelectionTypes
	{
		Click,
		MouseDown,
		MouseUp,
		Normal,
		Replace,
		Valid,
	}
}
