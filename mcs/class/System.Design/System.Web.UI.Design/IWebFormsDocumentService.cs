//
// System.Web.UI.Design.IWebFormsDocumentService
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

namespace System.Web.UI.Design
{
	public interface IWebFormsDocumentService
	{
		event EventHandler LoadComplete;

		object CreateDiscardableUndoUnit ();
		void DiscardUndoUnit (object discardableUndoUnit);
		void EnableUndo (bool enable);
		void UpdateSelection ();

		string DocumentUrl {
			get;
		}
		bool IsLoading {
			get;
		}
	}
}
