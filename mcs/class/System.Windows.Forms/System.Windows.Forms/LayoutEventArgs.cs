//
// System.Windows.Forms.LayoutEventArgs.cs
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

    public sealed class LayoutEventArgs : EventArgs {
		private Control affectedcontrol;
		private string affectedproperty;
		//
		//  --- Constructor
		//
		public LayoutEventArgs (Control affectedControl, string affectedProperty)
		{
			affectedproperty = affectedProperty;
			affectedcontrol = affectedControl;
		}

		
		//  --- Public Properties
		
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
