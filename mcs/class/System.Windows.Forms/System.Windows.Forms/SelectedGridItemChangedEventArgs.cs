//
// System.Windows.Forms.SelectedGridItemChangedEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms 
{

	// <summary>
	// </summary>

	public class SelectedGridItemChangedEventArgs : EventArgs {
		GridItem old;
		GridItem newGridItem;

		//
		//  --- Constructor
		//
		public SelectedGridItemChangedEventArgs(GridItem old, GridItem newGridItem) {
			this.newGridItem = newGridItem;
			this.old = old;
		}

		#region Public Properties
		public GridItem NewSelection 
		{
			get {
				return newGridItem;
			}
		}
		public GridItem OldSelection {
			get {
				return old;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two SelectedGridItemChangedEventArgs objects.
		///	The return value is based on the equivalence of
		///	NewSelection and OldSelection Property
		///	of the two SelectedGridItemChangedEventArgs.
		/// </remarks>
		public static bool operator == (SelectedGridItemChangedEventArgs SelectedGridItemChangedEventArgsA, SelectedGridItemChangedEventArgs SelectedGridItemChangedEventArgsB) 
		{
			return (SelectedGridItemChangedEventArgsA.NewSelection == SelectedGridItemChangedEventArgsB.NewSelection) && (SelectedGridItemChangedEventArgsA.OldSelection == SelectedGridItemChangedEventArgsB.OldSelection);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two UICuesEventArgs objects.
		///	The return value is based on the equivalence of
		///	Changed Property
		///	of the two UICuesEventArgs.
		/// </remarks>
		public static bool operator != (SelectedGridItemChangedEventArgs SelectedGridItemChangedEventArgsA, SelectedGridItemChangedEventArgs SelectedGridItemChangedEventArgsB) 
		{
			return (SelectedGridItemChangedEventArgsA.NewSelection != SelectedGridItemChangedEventArgsB.NewSelection) || (SelectedGridItemChangedEventArgsA.OldSelection != SelectedGridItemChangedEventArgsB.OldSelection);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	SelectedGridItemChangedEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is SelectedGridItemChangedEventArgs))return false;
			return (this == (SelectedGridItemChangedEventArgs) obj);
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
		///	Formats the SelectedGridItemChangedEventArgs as a string.
		/// </remarks>
		[MonoTODO]
		public override string ToString () 
		{
			//FIXME: add class specific stuff;
			return base.ToString();
		}

		#endregion  
	}
}
