//
// System.Windows.Forms.ItemCheckEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//	 Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

	public class ItemCheckEventArgs : EventArgs {

		#region Fields
		private int index;
		private CheckState newcheckvalue;
		private CheckState currentcheckvalue;
		#endregion

		//
		//  --- Constructor
		//
		public ItemCheckEventArgs(int index,  CheckState newCheckValue, CheckState currentValue ) 
		{
			this.index = index;
			newcheckvalue = newCheckValue;
			currentcheckvalue = currentValue;
		}
		
		#region Public Properties

		public CheckState CurrentValue 
		{
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
		#endregion
	}
}
