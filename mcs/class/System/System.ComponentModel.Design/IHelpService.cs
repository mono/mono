//
// System.ComponentModel.Design.IHelpService
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	public interface IHelpService
	{
		void AddContextAttribute (string name, string value,
					  HelpKeywordType keywordType);

		void ClearContextAttributes();
		IHelpService CreateLocalContext (HelpContextType contextType);
		void RemoveContextAttribute (string name, string value);
		void RemoveLocalContext (IHelpService localContext);
		void ShowHelpFromKeyword (string helpKeyword);
		void ShowHelpFromUrl (string helpUrl);
	}
}
