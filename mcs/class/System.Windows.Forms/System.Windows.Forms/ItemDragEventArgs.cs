//
// System.Windows.Forms.ItemDragEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)   
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	// </summary>

    public class ItemDragEventArgs : EventArgs {

		#region Fields
		private MouseButtons buttons;
		private object itemdrageobject;
		#endregion

		//
		//  --- Constructor
		//
		public ItemDragEventArgs(MouseButtons bttns)
		{
			buttons = bttns;
		}
		public ItemDragEventArgs(MouseButtons bttns, object o)
		{
			buttons = bttns;
			itemdrageobject = o;
		}
		
		#region Public Properties
		public MouseButtons Button 
		{
			get {
				return buttons;
			}
		}

		public object Item 
		{
			get {
				return itemdrageobject;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ItemDragEventArgs objects.
		///	The return value is based on the equivalence of
		///	button and item Property
		///	of the two ItemDragEventArgs.
		/// </remarks>
		public static bool operator == (ItemDragEventArgs ItemDragEventArgsA, ItemDragEventArgs ItemDragEventArgsB) 
		{
			return (ItemDragEventArgsA.Button == ItemDragEventArgsB.Button) && 
				   (ItemDragEventArgsA.Item == ItemDragEventArgsB.Item);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ItemDragEventArgs objects.
		///	The return value is based on the equivalence of
		///	button and item Property
		///	of the two ItemDragEventArgs.
		/// </remarks>
		public static bool operator != (ItemDragEventArgs ItemDragEventArgsA, ItemDragEventArgs ItemDragEventArgsB) 
		{
			return (ItemDragEventArgsA.Button != ItemDragEventArgsB.Button) || 
				(ItemDragEventArgsA.Item != ItemDragEventArgsB.Item);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	ItemDragEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is ItemDragEventArgs))return false;
			return (this == (ItemDragEventArgs) obj);
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
			return base.ToString() + " ItemDragEventArgs";
		}


		#endregion
 
	}
}
