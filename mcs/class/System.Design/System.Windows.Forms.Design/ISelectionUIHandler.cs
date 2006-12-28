//
// System.Windows.Forms.Design.ISelectionUIHandler.cs
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
// (C) 2004 Ximian, Inc.  http://www.ximian.com
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

using System;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms.Design
{
	internal interface ISelectionUIHandler
	{
		bool BeginDrag (object[] components, SelectionRules rules, int initialX, int initialY);
		void DragMoved (object[] components, Rectangle offset);
		void EndDrag (object[] components, bool cancel);
		Rectangle GetComponentBounds (object component);
		SelectionRules GetComponentRules (object component);
		Rectangle GetSelectionClipRect (object component);
		void OleDragDrop (DragEventArgs de);
		void OleDragEnter (DragEventArgs de);
		void OleDragLeave ();
		void OleDragOver (DragEventArgs de);
		void OnSelectionDoubleClick (IComponent component);
		bool QueryBeginDrag (object[] components, SelectionRules rules, int initialX, int initialY);
		void ShowContextMenu (IComponent component);
	}
}
