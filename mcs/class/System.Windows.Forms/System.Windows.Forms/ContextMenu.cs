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
