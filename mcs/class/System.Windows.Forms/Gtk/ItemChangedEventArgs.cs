//
// System.Windows.Forms.ItemChangedEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//	 Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

	public class ItemChangedEventArgs : EventArgs {

		#region Fields
		// After looking at ItemChangedEvent, It seems index is always zero.
		// The ItemChangedEventhandler quierys the sender object to find the item changed.
		private int index = 0;		//Never assigned. default to 0.
		#endregion

		#region Public Properties
		public int Index 
		{
			get {
				return index;
			}
		}
		#endregion

	}
}
