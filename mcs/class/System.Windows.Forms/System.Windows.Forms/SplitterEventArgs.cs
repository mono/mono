//
// System.Windows.Forms.SplitterEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    [ComVisible(true)]
	public class SplitterEventArgs : EventArgs {

		#region Fields
		private int x;
		private int y;
		private int splitx;
		private int splity;
		#endregion

		//
		//  --- Constructor
		//
		public SplitterEventArgs(int x, int y, int splitX, int splitY)
		{
			this.x = x;
			this.y = y;
			splitx = splitX;
			splity = splitY;
		}

		#region Public Properties

		[ComVisible(true)]
		public int SplitX {
			get {
				return splitx;
			}
			set {
				splitx = value;
			}
		}

		[ComVisible(true)]
		public int SplitY {
			get {
				return splity;
			}
			set {
				splity = value;
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
