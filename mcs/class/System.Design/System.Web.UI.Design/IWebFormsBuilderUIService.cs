//
// System.Web.UI.Design.IWebFormsBuilderUIService
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.Windows.Forms;

namespace System.Web.UI.Design
{
	public interface IWebFormsBuilderUIService
	{
		string BuildColor (System.Windows.Forms.Control owner, string initialColor);
		string BuildUrl (System.Windows.Forms.Control owner, string initialUrl, string baseUrl, string caption, string filter, UrlBuilderOptions options);
	}
}
