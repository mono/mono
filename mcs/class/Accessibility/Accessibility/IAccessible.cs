//
// IAccessible.cs: Interface for accessible elements
//
// What a horrible interface.
//

namespace Accessibility {
	
	public interface IAccessible {
		
		void   accDoDefaultAction (object childID);
		object accHitTest (int xLeft, int yTop);
		void   accLocation (out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, object childID);
		object accNavigate(int navDir, object childID);
		void   accSelect(int flagsSelect, object childID);
		object get_accChild(object childID);
		string get_accDefaultAction(object childID);
		string get_accDescription(object childID);
		string get_accHelp(object childID);
		int    get_accHelpTopic(out string pszHelpFile,object childID);
		string get_accKeyboardShortcut(object childID);
		string get_accName(object childID);
		object get_accRole(object childID);
		object get_accState(object childID);
		string get_accValue(object childID);
		void   set_accName(object childID, string newName);
		void   set_accValue(object childID, string newValue);


		int    accChildCount { get; }
		object accFocus { get; }
		object accParent { get;}
		object accSelection { get; } 
	}
}
