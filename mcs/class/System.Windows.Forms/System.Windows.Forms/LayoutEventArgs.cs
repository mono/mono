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
	//	This is only a template.  Nothing is implemented yet.
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

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two LayoutEventArgs objects.
		///	The return value is based on the equivalence of
		///	AffectedControl and AffectedProperty Property
		///	of the two LayoutEventArgs.
		/// </remarks>
		public static bool operator == (LayoutEventArgs LayoutEventArgsA, LayoutEventArgs LayoutEventArgsB) 
		{
			return (LayoutEventArgsA.AffectedControl == LayoutEventArgsB.AffectedControl) && (LayoutEventArgsA.AffectedProperty == LayoutEventArgsB.AffectedProperty);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two LayoutEventArgs objects.
		///	The return value is based on the equivalence of
		///	AffectedControl and AffectedProperty Property
		///	of the two LayoutEventArgs.
		/// </remarks>
		public static bool operator != (LayoutEventArgs LayoutEventArgsA, LayoutEventArgs LayoutEventArgsB) 
		{
			return (LayoutEventArgsA.AffectedControl != LayoutEventArgsB.AffectedControl) || (LayoutEventArgsA.AffectedProperty != LayoutEventArgsB.AffectedProperty);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	LayoutEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is LayoutEventArgs))return false;
			return (this == (LayoutEventArgs) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		[MonoTODO]
		public override int GetHashCode () 
		{
			//FIXME: add class specific stuff;
			return base.GetHashCode();
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the object as a string.
		/// </remarks>
		[MonoTODO]
		public override string ToString () 
		{
			//FIXME: add class specific stuff;
			return base.ToString() + " LayoutEventArgs";
		}


		#endregion

	}
}
