//
// System.Windows.Forms.ItemDragEventArgs.cs
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

    public class ItemDragEventArgs : EventArgs {
		private MouseButtons buttons;
		private object itemdrageobject;
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
		
		//  --- Public Properties
		
		public MouseButtons Button {
			get {
				return buttons;
			}
		}
		public object Item {
			get {
				return itemdrageobject;
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
