//
// System.Windows.Forms.StatusBarPanelClickEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the PanelClick event.
	/// </summary>
	public class StatusBarPanelClickEventArgs : MouseEventArgs {

		private StatusBarPanel panel;

		/// --- Constructor ---
		public StatusBarPanelClickEventArgs(StatusBarPanel panel, MouseButtons button, int clicks, int x, int y) : base(button, clicks, x, y, 0) 
		{
			this.panel = panel;
		}

		#region Public Properties
		
		/// <summary>
		///	StatusBarPanel Property
		/// </summary>
		///
		/// <remarks>
		///	Gets the StatusBarPanel to draw.
		/// </remarks>
		public StatusBarPanel StatusBarPanel 
		{
			get 
			{
				return panel;
			}
		}

		#endregion
	}
}
