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
	//	This is only a template.  Nothing is implemented yet.
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

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two InvalidateEventArgs objects.
		///	The return value is based on the equivalence of
		///	InvalidRect Property
		///	of the two InvalidateEventArgs.
		/// </remarks>
		public static bool operator == (InvalidateEventArgs InvalidateEventArgsA, InvalidateEventArgs InvalidateEventArgsB) 
		{
			return (InvalidateEventArgsA.InvalidRect == InvalidateEventArgsB.InvalidRect);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two InvalidateEventArgs objects.
		///	The return value is based on the equivalence of
		///	InvalidRect Property
		///	of the two InvalidateEventArgs.
		/// </remarks>
		public static bool operator != (InvalidateEventArgs InvalidateEventArgsA, InvalidateEventArgs InvalidateEventArgsB) 
		{
			return (InvalidateEventArgsA.InvalidRect != InvalidateEventArgsB.InvalidRect);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	InvalidateEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is InvalidateEventArgs))return false;
			return (this == (InvalidateEventArgs) obj);
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
			return base.ToString() + " InvalidateEventArgs";
		}


		#endregion	 
	}
}
