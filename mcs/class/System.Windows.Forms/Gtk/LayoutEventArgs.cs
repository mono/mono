//
// System.Windows.Forms.LayoutEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//  Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public sealed class LayoutEventArgs : EventArgs {

		#region Fields

		private Control affectedcontrol;
		private string affectedproperty;
		
		#endregion
		//
		//  --- Constructor
		//
		public LayoutEventArgs (Control affectedControl, string affectedProperty)
		{
			affectedproperty = affectedProperty;
			affectedcontrol = affectedControl;
		}

		#region Public Properties
		
		public Control AffectedControl {
			get {
				return affectedcontrol;
			}
		}
		public string AffectedProperty {
			get {
				return affectedproperty;
			}
		}

		#endregion
	}
}
