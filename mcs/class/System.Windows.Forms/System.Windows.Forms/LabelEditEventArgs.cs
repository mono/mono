//
// System.Windows.Forms.LabelEditEventArgs.cs
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

    public class LabelEditEventArgs : EventArgs {
		private int item;
		private string label;
		//
		//  --- Constructor
		//
		public LabelEditEventArgs (int item)
		{
			this.item = item;
			// Fixme leave label uninitilized?
		}

		public LabelEditEventArgs (int item, string label)
		{
			this.item = item;
			this.label = label;
		}

		//
		//  --- Public Properties
		//
		//[MonoTODO]
		//public bool CancelEdit {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		public int Item {
			get {
				return item;
			}
		}
		public string Label {
			get {
				return label;
			}
		}

		//
		//  --- Public Methods
		//
		//[MonoTODO]
		//public virtual bool Equals(object o);
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public static bool Equals(object o1, object o2);
		//{
		//	throw new NotImplementedException ();
		//}
	 }
}
