//
// System.Web.UI.Design.ITemplateEditingService
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.Web.UI.WebControls;

namespace System.Web.UI.Design
{
	public interface ITemplateEditingService
	{
		ITemplateEditingFrame CreateFrame (TemplatedControlDesigner designer, string frameName, string[] templateNames);
		ITemplateEditingFrame CreateFrame (TemplatedControlDesigner designer, string frameName, string[] templateNames, Style controlStyle, Style[] templateStyles);
		string GetContainingTemplateName (Control control);

		bool SupportsNestedTemplateEditing {
			get;
		}
	}
}
