//
// System.Diagnostics.Design.ProcessModuleDesigner
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.Collections;
using System.ComponentModel.Design;

namespace System.Diagnostics.Design
{
	public class ProcessModuleDesigner : ComponentDesigner
	{
		public ProcessModuleDesigner ()
		{
		}

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}
	}
}
