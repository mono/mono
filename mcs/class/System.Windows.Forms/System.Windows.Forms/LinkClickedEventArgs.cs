//
// System.Windows.Forms.LinkClickedEventArgs.cs
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

	public class LinkClickedEventArgs : EventArgs {
		private string linktext;
		//
		//  --- Constructor
		//
		public LinkClickedEventArgs(string linkText) {
			linktext = linkText;;
		}

		//
		//  --- Public Properties
		//
		public string LinkText {
			get {
				return linktext;
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
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}	 
	}
}
