//
// System.ServiceProcess.Design.ServiceControllerDesigner
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.Collections;
using System.ComponentModel.Design;

namespace System.ServiceProcess.Design
{
	public class ServiceControllerDesigner : ComponentDesigner
	{
		public ServiceControllerDesigner ()
		{
		}

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}
	}
}
