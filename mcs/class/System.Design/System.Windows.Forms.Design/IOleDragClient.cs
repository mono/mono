//
// System.Windows.Forms.Design.IOleDragClient.cs
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
// (C) 2004 Ximian, Inc.  http://www.ximian.com
//

using System.ComponentModel;

namespace System.Windows.Forms.Design
{
	internal interface IOleDragClient
	{
		bool AddComponent(IComponent component, string name, bool firstAdd);
		Control GetControlForComponent(object component);
		Control GetDesignerControl();
		bool IsDropOk(IComponent component);

		bool CanModifyComponents { get; }
		IComponent Component { get; }
	}
}
