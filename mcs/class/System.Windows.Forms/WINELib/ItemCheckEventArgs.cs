//
// System.Windows.Forms.ItemCheckEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//	 Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

	public class ItemCheckEventArgs : EventArgs {

		#region Fields
		private int index;
		private CheckState newcheckvalue;
		private CheckState currentcheckvalue;
		#endregion

		//
		//  --- Constructor
		//
		public ItemCheckEventArgs(int index,  CheckState newCheckValue, CheckState currentValue ) 
		{
			this.index = index;
			newcheckvalue = newCheckValue;
			currentcheckvalue = currentValue;
		}
		
		#region Public Properties
		public CheckState CurrentValue 
		{
			get {
				return currentcheckvalue;
			}
		}
		public int Index {
			get {
				return index;
			}
		}
		public CheckState NewValue {
			get {
				return newcheckvalue;
			}
			set {
				newcheckvalue = value;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ItemCheckEventArgs objects.
		///	The return value is based on the equivalence of
		///	CurrentValue, Index, NewValue and end Property
		///	of the two ItemCheckEventArgs.
		/// </remarks>
		public static bool operator == (ItemCheckEventArgs ItemCheckEventArgsA, ItemCheckEventArgs ItemCheckEventArgsB) 
		{
			return (ItemCheckEventArgsA.CurrentValue == ItemCheckEventArgsB.CurrentValue) && 
				   (ItemCheckEventArgsA.Index == ItemCheckEventArgsB.Index) && 
				   (ItemCheckEventArgsA.NewValue == ItemCheckEventArgsB.NewValue);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ItemCheckEventArgs objects.
		///	The return value is based on the equivalence of
		///	CurrentValue, Index, NewValue and end Property
		///	of the two ItemCheckEventArgs.
		/// </remarks>
		public static bool operator != (ItemCheckEventArgs ItemCheckEventArgsA, ItemCheckEventArgs ItemCheckEventArgsB) 
		{
			return (ItemCheckEventArgsA.CurrentValue != ItemCheckEventArgsB.CurrentValue) || 
				(ItemCheckEventArgsA.Index != ItemCheckEventArgsB.Index) || 
				(ItemCheckEventArgsA.NewValue != ItemCheckEventArgsB.NewValue);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	ItemCheckEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is ItemCheckEventArgs))return false;
			return (this == (ItemCheckEventArgs) obj);
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
			return base.ToString() + " ItemCheckEventArgs";
		}


		#endregion
	}
}
