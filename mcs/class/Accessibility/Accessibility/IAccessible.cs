//
// IAccessible.cs: Interface for accessible elements
//
// What a horrible interface.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
