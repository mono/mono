//
// System.Diagnostics.Design.ProcessThreadDesigner
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
	public class ProcessThreadDesigner : ComponentDesigner
	{
		public ProcessThreadDesigner ()
		{
		}

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}
	}
}
