//
// System.Windows.Forms.ItemDragEventArgs.cs
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

    public class ItemDragEventArgs : EventArgs {

		#region Fields
		private MouseButtons buttons;
		private object itemdrageobject;
		#endregion

		//
		//  --- Constructor
		//
		public ItemDragEventArgs(MouseButtons bttns)
		{
			buttons = bttns;
		}
		public ItemDragEventArgs(MouseButtons bttns, object o)
		{
			buttons = bttns;
			itemdrageobject = o;
		}
		
		#region Public Properties
		public MouseButtons Button 
		{
			get {
				return buttons;
			}
		}

		public object Item 
		{
			get {
				return itemdrageobject;
			}
		}
		#endregion
	}
}
