//
// System.Windows.Forms.SplitterEventArgs.cs
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
	// </summary>

    [ComVisible(true)]
	public class SplitterEventArgs : EventArgs {

		#region Fields
		private int x;
		private int y;
		private int splitx;
		private int splity;
		#endregion

		//
		//  --- Constructor
		//
		public SplitterEventArgs(int x, int y, int splitX, int splitY)
		{
			this.x = x;
			this.y = y;
			splitx = splitX;
			splity = splitY;
		}

		#region Public Properties
		[ComVisible(true)]
		public int SplitX {
			get {
				return splitx;
			}
			set {
				splitx = value;
			}
		}
		[ComVisible(true)]
		public int SplitY {
			get {
				return splity;
			}
			set {
				splity = value;
			}
		}
		[ComVisible(true)]
		public int X {
			get {
				return x;
			}
		}
		[ComVisible(true)]
		public int Y {
			get {
				return y;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two SplitterEventArgs objects.
		///	The return value is based on the equivalence of
		///	SplitX, SplitY, X, Y Property
		///	of the two SplitterEventArgs.
		/// </remarks>
		public static bool operator == (SplitterEventArgs SplitterEventArgsA, SplitterEventArgs SplitterEventArgsB) 
		{
			return (SplitterEventArgsA.SplitX == SplitterEventArgsB.SplitX) && (SplitterEventArgsA.SplitY == SplitterEventArgsB.SplitY) && (SplitterEventArgsA.X == SplitterEventArgsB.X) && (SplitterEventArgsA.Y == SplitterEventArgsB.Y);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two SplitterEventArgs objects.
		///	The return value is based on the equivalence of
		///	SplitX, SplitY, X, Y Property
		///	of the two SplitterEventArgs.
		/// </remarks>
		public static bool operator != (SplitterEventArgs SplitterEventArgsA, SplitterEventArgs SplitterEventArgsB) 
		{
			return (SplitterEventArgsA.SplitX != SplitterEventArgsB.SplitX) || (SplitterEventArgsA.SplitY != SplitterEventArgsB.SplitY) || (SplitterEventArgsA.X != SplitterEventArgsB.X) || (SplitterEventArgsA.Y != SplitterEventArgsB.Y);
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
			if (!(obj is SplitterEventArgs))return false;
			return (this == (SplitterEventArgs) obj);
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
		///	Formats the SplitterEventArgs as a string.
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
