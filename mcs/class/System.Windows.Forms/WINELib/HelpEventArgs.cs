//
// System.Windows.Forms.HelpEventArgs.cs
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
	// Complete.
	// </summary>

	public class HelpEventArgs : EventArgs {

		#region Fields
		private Point mousePos;
		private  bool handled;
		#endregion

		//
		//  --- Constructor
		//
		public HelpEventArgs(Point mousePos) 
		{
			this.mousePos = mousePos;
			handled = false; // Gian : hadled is false, otherwise all events are managed by user by default.
		}

		#region Public Properties
		public bool Handled 
		{
			get {
				return handled;
			}
			set {
				handled = value;
			}
		}
		public Point MousePos {
			get {
				return mousePos;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two HelpEventArgs objects.
		///	The return value is based on the equivalence of
		///	Handled and MousePos Property
		///	of the two HelpEventArgs.
		/// </remarks>
		public static bool operator == (HelpEventArgs HelpEventArgsA, HelpEventArgs HelpEventArgsB) 
		{
			return (HelpEventArgsA.Handled == HelpEventArgsB.Handled) && 
				   (HelpEventArgsA.MousePos == HelpEventArgsB.MousePos);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two HelpEventArgs objects.
		///	The return value is based on the equivalence of
		///	Handled and MousePos Property
		///	of the two HelpEventArgs.
		/// </remarks>
		public static bool operator != (HelpEventArgs HelpEventArgsA, HelpEventArgs HelpEventArgsB) 
		{
			return (HelpEventArgsA.Handled != HelpEventArgsB.Handled) || 
				   (HelpEventArgsA.MousePos != HelpEventArgsB.MousePos);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	HelpEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is HelpEventArgs))return false;
			return (this == (HelpEventArgs) obj);
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
			return base.ToString() + " HelpEventArgs";
		}


		#endregion
	}
}
