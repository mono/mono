//
// System.Windows.Forms.HelpEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	// Complete.
	// </summary>

	public class HelpEventArgs : EventArgs {

		#region Fields
		private Point mousePos;
		private  bool handled;
		#endregion

		//
		//  --- Constructor
		//
		public HelpEventArgs(Point mousePos) 
		{
			this.mousePos = mousePos;
			handled = false; // Gian : hadled is false, otherwise all events are managed by user by default.
		}

		#region Public Properties
		public bool Handled 
		{
			get {
				return handled;
			}
			set {
				handled = value;
			}
		}
		public Point MousePos {
			get {
				return mousePos;
			}
		}
		#endregion
	}
}
