//
// System.Web.UI.Design.TemplateEditingService
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.Web.UI.WebControls;
using System.ComponentModel.Design;

namespace System.Web.UI.Design
{
	public sealed class TemplateEditingService : ITemplateEditingService, IDisposable
	{
		public TemplateEditingService (IDesignerHost designerHost)
		{
			if (designerHost == null)
				throw new ArgumentNullException ("designerHost");

			_designerHost = designerHost;
		}

		~TemplateEditingService ()
		{
			Dispose (false);
		}

		[MonoTODO]
		public ITemplateEditingFrame CreateFrame (TemplatedControlDesigner designer, string frameName, string[] templateNames)
		{
			return CreateFrame (designer, frameName, templateNames, null, null);
		}

		[MonoTODO]
		public ITemplateEditingFrame CreateFrame (TemplatedControlDesigner designer, string frameName, string[] templateNames, Style controlStyle, Style[] templateStyles)
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private void Dispose (bool disposing)
		{
			if (disposing)
				_designerHost = null;
		}

		[MonoTODO]
		public string GetContainingTemplateName (Control control)
		{
			throw new NotImplementedException ();
		}

		public bool SupportsNestedTemplateEditing {
			get {
				return false;
			}
		}

		private IDesignerHost _designerHost;
	}
}
