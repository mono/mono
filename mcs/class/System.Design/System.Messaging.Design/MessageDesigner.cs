//
// System.Messaging.Design.MessageDesigner
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.Collections;
using System.ComponentModel.Design;

namespace System.Messaging.Design 
{
	public class MessageDesigner : ComponentDesigner
	{
		public MessageDesigner ()
		{
		}

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}
	}
}
