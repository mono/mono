//
// System.Windows.Forms.ItemChangedEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//	 Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

	public class ItemChangedEventArgs : EventArgs {

		#region Fields
		private int index = 0;		//Never assigned. default to 0.
		#endregion

		#region Public Properties
		public int Index 
		{
			get {
				return index;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ItemChangedEventArgs objects.
		///	The return value is based on the equivalence of
		///	Index Property
		///	of the two ItemChangedEventArgs.
		/// </remarks>
		public static bool operator == (ItemChangedEventArgs ItemChangedEventArgsA, ItemChangedEventArgs ItemChangedEventArgsB) 
		{
			return (ItemChangedEventArgsA.Index == ItemChangedEventArgsB.Index);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ItemChangedEventArgs objects.
		///	The return value is based on the equivalence of
		///	Index Property
		///	of the two ItemChangedEventArgs.
		/// </remarks>
		public static bool operator != (ItemChangedEventArgs ItemChangedEventArgsA, ItemChangedEventArgs ItemChangedEventArgsB) 
		{
			return (ItemChangedEventArgsA.Index != ItemChangedEventArgsB.Index);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	ItemChangedEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is ItemChangedEventArgs))return false;
			return (this == (ItemChangedEventArgs) obj);
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
			return base.ToString() + " ItemChangedEventArgs";
		}


		#endregion

	}
}
