//
// System.Web.UI.Design.TemplateEditingVerb
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
	public class TemplateEditingVerb : DesignerVerb, IDisposable
	{
		public TemplateEditingVerb (string text, int index, TemplatedControlDesigner designer) : base (text, designer.TemplateEditingVerbHandler)
		{
			_index = index;
		}

		~TemplateEditingVerb ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
			}
		}

		public int Index {
			get {
				return _index;
			}
		}

		private int _index;
	}
}
