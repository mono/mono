//
// System.Windows.Forms.ItemCheckEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

        public class ItemCheckEventArgs : EventArgs {
			private int index;
			private CheckState newcheckvalue;
			private CheckState currentcheckvalue;
		//
		//  --- Constructor
		//
		public ItemCheckEventArgs(int index,  CheckState newCheckValue, CheckState currentValue )
		{
			this.index = index;
			newcheckvalue = newCheckValue;
			currentcheckvalue = currentValue;
		}
		
		//  --- Public Properties
		
		public CheckState CurrentValue {
			get {
				return currentcheckvalue;
			}
		}
		public int Index {
			get {
				return index;
			}
		}
		public CheckState NewValue {
			get {
				return newcheckvalue;
			}
			set {
				newcheckvalue = value;
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
