//
// System.Web.UI.Design.IControlDesignerBehavior
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

namespace System.Web.UI.Design
{
	public interface IControlDesignerBehavior
	{
		void OnTemplateModeChanged ();

		object DesignTimeElementView {
			get;
		}
		string DesignTimeHtml {
			get;
			set;
		}
	}
}