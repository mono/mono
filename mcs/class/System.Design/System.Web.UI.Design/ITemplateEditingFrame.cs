//
// System.Web.UI.Design.ITemplateEditingFrame
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.Web.UI.WebControls;

namespace System.Web.UI.Design
{
	public interface ITemplateEditingFrame : IDisposable
	{
		void Close (bool saveChanges);
		void Open ();
		void Resize (int width, int height);
		void Save ();
		void UpdateControlName (string newName);

		Style ControlStyle {
			get;
		}
		int InitialHeight {
			get;
			set;
		}
		int InitialWidth {
			get;
			set;
		}
		string Name {
			get;
		}
		string[] TemplateNames {
			get;
		}
		Style[] TemplateStyles {
			get;
		}
		TemplateEditingVerb Verb {
			get;
			set;
		}
	}
}
