//
// System.Windows.Forms.ContextMenu.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc., 2002
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

//using System.ComponentModel;
//using System.Collections;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a shortcut menu
	/// </summary>

	[MonoTODO]
	public class ContextMenu : Menu {

		// private fields
		RightToLeft rightToLeft;
		Control sourceControl;

		// --- Constructor ---
		[MonoTODO]
		public ContextMenu() : base(null)
		{
			rightToLeft = RightToLeft.Inherit;
			sourceControl = null;
			isPopupMenu   = true;
		}

		[MonoTODO]
		public ContextMenu(MenuItem[] menuItems) : base(menuItems)//menu does not have public constructor. Is this a gtk menu?
		{
			isPopupMenu   = true;
		}
		
		// --- Properties ---
		public virtual RightToLeft RightToLeft {
			get { return rightToLeft; }
			set { rightToLeft=value; }
		}
		
		public Control SourceControl {
			get { return sourceControl; }
		}
		
		/// --- Methods ---
		/// internal .NET framework supporting methods, not stubbed out:
		/// - protected internal virtual void OnPopup(EventArgs e);
		[MonoTODO]
		public void Show(Control control, Point pos) 
		{
			if ( Handle == IntPtr.Zero )
				return;
			
			sourceControl = control;

			POINT pt = new POINT ();
			pt.x = pos.X;
			pt.y = pos.Y;

			Win32.ClientToScreen ( control.Handle, ref pt );
			Win32.TrackPopupMenu (  Handle, ( uint ) ( TrackPopupMenuFlags.TPM_LEFTALIGN | TrackPopupMenuFlags.TPM_RIGHTBUTTON ),
						pt.x, pt.y, 0, control.Handle , IntPtr.Zero );
		}
		
		/// events
		[MonoTODO]
		public event EventHandler Popup;
	}
}
