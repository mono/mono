//
// System.Windows.Forms.InvalidateEventArgs.cs
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
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

        public class InvalidateEventArgs : EventArgs {
		private Rectangle InvalidRectangle;
		//
		//  --- Constructor
		//
		public InvalidateEventArgs(Rectangle invalidRect)
		{
			InvalidRectangle = invalidRect;
		}

		//
		//  --- Public Properties
		//
		public Rectangle InvalidRect {
			get {
				return InvalidRectangle;
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
