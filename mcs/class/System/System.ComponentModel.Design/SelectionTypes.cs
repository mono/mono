//
// System.ComponentModel.Design.SelectionTypes.cs
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
		Click = 16,
		MouseDown = 4,
		MouseUp = 8,
		Normal = 1,
		Replace = 2,
		Valid = 31
	}
}
