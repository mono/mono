//
// System.Web.UI.Design.IWebFormReferenceManager
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

namespace System.Web.UI.Design
{
	public interface IWebFormReferenceManager
	{
		Type GetObjectType (string tagPrefix, string typeName);
		string GetRegisterDirectives ();
		string GetTagPrefix (Type objectType);
	}
}
