//
// System.Windows.Forms.HelpEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class HelpEventArgs : EventArgs {
		private Point mousePos;
		private  bool handled;
		//
		//  --- Constructor
		//
		public HelpEventArgs(Point mousePos) {
			this.mousePos = mousePos;
			handled = false; // FixMe what should we default to?
		}

		//
		//  --- Public Properties
		//
		public bool Handled {
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

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public override bool Equals(object o) {
			throw new NotImplementedException ();
		}
		//public static bool Equals(object o1, object o2) {
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override string ToString() {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}	 
	}
}
