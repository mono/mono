//
// System.Windows.Forms.LabelEditEventArgs.cs
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
	// </summary>

	public class LabelEditEventArgs : EventArgs {

		#region Fields

		private int item;
		private string label = "";			//Gian : Initialized string to empty...
		private bool canceledit = false;	
		
		#endregion
		//
		//  --- Constructor
		//
		public LabelEditEventArgs (int item) 
		{
			this.item = item;
		}

		public LabelEditEventArgs (int item, string label) {
			this.item = item;
			this.label = label;
		}


		#region Public Properties
		public bool CancelEdit 
		{
			get {
				return canceledit;
			}
			set {
				canceledit = value;
			}
		}
		public int Item {
			get {
				return item;
			}
		}
		public string Label {
			get {
				return label;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two LabelEditEventArgs objects.
		///	The return value is based on the equivalence of
		///	CancelEdit, Item and Label Property
		///	of the two LabelEditEventArgs.
		/// </remarks>
		public static bool operator == (LabelEditEventArgs LabelEditEventArgsA, LabelEditEventArgs LabelEditEventArgsB) 
		{
			return (LabelEditEventArgsA.CancelEdit == LabelEditEventArgsB.CancelEdit) &&
				   (LabelEditEventArgsA.Item == LabelEditEventArgsB.Item) &&
				   (LabelEditEventArgsA.Label == LabelEditEventArgsB.Label);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two LabelEditEventArgs objects.
		///	The return value is based on the equivalence of
		///	CancelEdit, Item and Label Property
		///	of the two LabelEditEventArgs.
		/// </remarks>
		public static bool operator != (LabelEditEventArgs LabelEditEventArgsA, LabelEditEventArgs LabelEditEventArgsB) 
		{
			return (LabelEditEventArgsA.CancelEdit != LabelEditEventArgsB.CancelEdit) ||
				   (LabelEditEventArgsA.Item != LabelEditEventArgsB.Item) ||
				   (LabelEditEventArgsA.Label != LabelEditEventArgsB.Label);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	LabelEditEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is LabelEditEventArgs))return false;
			return (this == (LabelEditEventArgs) obj);
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
			return base.ToString() + " LabelEditEventArgs";
		}


		#endregion
	}
}
