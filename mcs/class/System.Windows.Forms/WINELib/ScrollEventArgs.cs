//
// System.Windows.Forms.ScrollEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//   Gianandrea Terzi (gterzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	// </summary>

    public class ScrollEventArgs : EventArgs {

		#region Fields
			
		private int newvalue;
		private ScrollEventType type;

		#endregion

		//
		//  --- Constructor
		//
		[MonoTODO]
		public ScrollEventArgs(ScrollEventType type, int newVal)
		{
			throw new NotImplementedException ();
		}

		
		#region Public Properties

		[ComVisible(true)]
		public int NewValue 
		{
			get {
				return newvalue;
			}
			set {
				newvalue = value;
			}
		}
		
		[ComVisible(true)]
		public ScrollEventType Type {
			get {
				return type;
			}
		}

		#endregion
	
		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ScrollEventArgs objects.
		///	The return value is based on the equivalence of
		///	newvalue and type Property
		///	of the two ScrollEventArgs.
		/// </remarks>
		public static bool operator == (ScrollEventArgs ScrollEventArgsA, ScrollEventArgs ScrollEventArgsB) 
		{
			return ((ScrollEventArgsA.NewValue == ScrollEventArgsB.NewValue) && (ScrollEventArgsA.Type == ScrollEventArgsB.Type));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ScrollEventArgs objects.
		///	The return value is based on the equivalence of
		///	newvalue and type Property
		///	of the two ScrollEventArgs.
		/// </remarks>
		public static bool operator != (ScrollEventArgs ScrollEventArgsA, ScrollEventArgs ScrollEventArgsB) 
		{
			return ((ScrollEventArgsA.NewValue != ScrollEventArgsB.NewValue) || (ScrollEventArgsA.Type != ScrollEventArgsB.Type));
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	UICuesEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is ScrollEventArgs))return false;
			return (this == (ScrollEventArgs) obj);
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
			return base.ToString();
		}


		#endregion

	}
}
