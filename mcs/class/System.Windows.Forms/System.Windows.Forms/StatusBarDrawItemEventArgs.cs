//
// System.Windows.Forms.StatusBarDrawItemEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002
//
using System.Drawing;
namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the DrawItem event.
	/// </summary>
	public class StatusBarDrawItemEventArgs : DrawItemEventArgs {

		private StatusBarPanel panel;

		/// --- Constructor ---
		public StatusBarDrawItemEventArgs(Graphics g, Font font, 
			Rectangle r, int itemId, DrawItemState itemState, 
			StatusBarPanel panel, Color foreColor, Color backColor)
			: base(g, font, r, itemId, itemState, foreColor, backColor) {
			this.panel = panel;
		}

		public StatusBarDrawItemEventArgs(Graphics g, Font font, 
			Rectangle r, int itemId, DrawItemState itemState, StatusBarPanel panel)
			: base(g, font, r, itemId, itemState) {
			this.panel = panel;
		}
		
		#region Public Properties
		public StatusBarPanel Panel 
		{
			get {
				return panel;
			}
		}
		#endregion
	}
}
