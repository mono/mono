//
// System.Windows.Forms.MouseEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//	 Gianandrea Terzi (gianandrea.terzi@lario.com)   
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class MouseEventArgs : EventArgs {

		#region Fields

		private MouseButtons button;
		private int clicks;
		private int x;
		private int y;
		private int delta;
		
		#endregion

		public MouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
		{
			this.button = button;
			this.clicks = clicks;
			this.x = x;
			this.y = y;
			this.delta = delta;
		}

		#region Public Properties

		[ComVisible(true)]
		public MouseButtons Button 
		{
			get {
				return button;
			}
		}

		[ComVisible(true)]
		public int Clicks {
			get {
				return clicks;
			}
		}

		[ComVisible(true)]
		public int Delta {
			get {
				return delta;
			}
		}

		[ComVisible(true)]
		public int X {
			get {
				return x;
			}
		}

		[ComVisible(true)]
		public int Y {
			get {
				return y;
			}
		}
		#endregion
	 }
}
