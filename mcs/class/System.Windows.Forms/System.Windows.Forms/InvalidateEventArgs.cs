//
// System.Windows.Forms.InvalidateEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

	public class InvalidateEventArgs : EventArgs {

		#region Fields
		private Rectangle InvalidRectangle;
		#endregion

		//
		//  --- Constructor
		//
		public InvalidateEventArgs(Rectangle invalidRect) 
		{
			InvalidRectangle = invalidRect;
		}

		#region Public Properties
		public Rectangle InvalidRect 
		{
			get {
				return InvalidRectangle;
			}
		}
		#endregion
	}
}
