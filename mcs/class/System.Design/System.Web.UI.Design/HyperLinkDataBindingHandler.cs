//
// System.Web.UI.Design.HyperLinkDataBindingHandler
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System;
using System.ComponentModel.Design;

namespace System.Web.UI.Design
{
	public class HyperLinkDataBindingHandler : DataBindingHandler
	{
		public HyperLinkDataBindingHandler ()
		{
		}

		[MonoTODO]
		public override void DataBindControl (IDesignerHost designerHost, Control control)
		{
			throw new NotImplementedException ();
		}
	}
}
