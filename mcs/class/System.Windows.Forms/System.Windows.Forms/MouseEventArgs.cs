//
// System.Windows.Forms.MouseEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//	 Gianandrea Terzi (gianandrea.terzi@lario.com)   
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public class MouseEventArgs : EventArgs {

		#region Fields

		private MouseButtons button;
		private int clicks;
		private int x;
		private int y;
		private int delta;
		
		#endregion

		public MouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
		{
			this.button = button;
			this.clicks = clicks;
			this.x = x;
			this.y = y;
			this.delta = delta;
		}

		#region Public Properties

		[ComVisible(true)]
		public MouseButtons Button 
		{
			get {
				return button;
			}
		}

		[ComVisible(true)]
		public int Clicks {
			get {
				return clicks;
			}
		}

		[ComVisible(true)]
		public int Delta {
			get {
				return delta;
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
		///	Compares two MouseEventArgs objects.
		///	The return value is based on the equivalence of
		///	X, Y, Clicks, Delta and Button  Property
		///	of the two MouseEventArgs.
		/// </remarks>
		public static bool operator == (MouseEventArgs MouseEventArgsA, MouseEventArgs MouseEventArgsB) 
		{
			return (MouseEventArgsA.X == MouseEventArgsB.X) && 
				   (MouseEventArgsA.Y == MouseEventArgsB.Y) && 
				   (MouseEventArgsA.Clicks == MouseEventArgsB.Clicks) && 
				   (MouseEventArgsA.Delta == MouseEventArgsB.Delta) &&
				   (MouseEventArgsA.Button == MouseEventArgsB.Button);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two MouseEventArgs objects.
		///	The return value is based on the equivalence of
		///	X, Y, Clicks, Delta and Button  Property
		///	of the two MouseEventArgs.
		/// </remarks>
		public static bool operator != (MouseEventArgs MouseEventArgsA, MouseEventArgs MouseEventArgsB) 
		{
			return (MouseEventArgsA.X != MouseEventArgsB.X) || 
				   (MouseEventArgsA.Y != MouseEventArgsB.Y) || 
				   (MouseEventArgsA.Clicks != MouseEventArgsB.Clicks) || 
				   (MouseEventArgsA.Delta != MouseEventArgsB.Delta) ||
				   (MouseEventArgsA.Button != MouseEventArgsB.Button);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	MouseEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is MouseEventArgs))return false;
			return (this == (MouseEventArgs) obj);
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
