//
// System.Windows.Forms.SplitterEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    [ComVisible(true)]
	public class SplitterEventArgs : EventArgs {
		private int x;
		private int y;
		private int splitx;
		private int splity;
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
		//
		//  --- Public Properties
		//
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

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}	 
	}
}
