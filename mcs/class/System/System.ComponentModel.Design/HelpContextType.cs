//
// System.ComponentModel.Design.HelpContextType.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	[Serializable]
	public enum HelpContextType
	{
		Ambient = 0,
		Window = 1,
		Selection = 2,
		ToolWindowSelection = 3
	}
}
