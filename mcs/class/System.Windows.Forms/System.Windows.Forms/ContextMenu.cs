//
// System.Windows.Forms.ContextMenu.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

//using System.ComponentModel;
//using System.Collections;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows combo box control.
	/// ToDo note:
	///  - nothing is implemented
	/// </summary>

	[MonoTODO]
	public class ContextMenu : Menu {

		// private fields
		RightToLeft rightToLeft;
		
		// --- Constructor ---
		[MonoTODO]
		public ContextMenu() //: base(null) 
		{
			rightToLeft = RightToLeft.Inherit;
		}

		[MonoTODO]
		public ContextMenu(MenuItem[] menuItems) //: base(menuItems)//menu does not have public constructor. Is this a gtk menu?
		{
		}
		
		
		
		
		// --- Properties ---
		public virtual RightToLeft RightToLeft {
			get { return rightToLeft; }
			set { rightToLeft=value; }
		}
		
		[MonoTODO]
		public Control SourceControl {
			get { throw new NotImplementedException (); }
		}
		
		
		
		
		/// --- Methods ---
		/// internal .NET framework supporting methods, not stubbed out:
		/// - protected internal virtual void OnPopup(EventArgs e);
		[MonoTODO]
		public void Show(Control control,Point pos) 
		{
			throw new NotImplementedException ();
		}
		
		
		
		
		/// events
		[MonoTODO]
		public event EventHandler Popup {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}

	}
}
