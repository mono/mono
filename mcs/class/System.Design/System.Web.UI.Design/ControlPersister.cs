//
// System.Web.UI.Design.ControlPersister
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.ComponentModel.Design;
using System.IO;

namespace System.Web.UI.Design
{
	public sealed class ControlPersister
	{
		private ControlPersister ()
		{
		}

		[MonoTODO]
		public static string PersistControl (Control control)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void PersistControl (TextWriter sw, Control control)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string PersistControl (Control control, IDesignerHost host)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void PersistControl (TextWriter sw, Control control, IDesignerHost host)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string PersistInnerProperties (object component, IDesignerHost host)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void PersistInnerProperties (TextWriter sw, object component, IDesignerHost host)
		{
			throw new NotImplementedException ();
		}
	}
}
