//
// System.Windows.Forms.StatusBarDrawItemEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002
//
using System.Drawing;
namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the DrawItem event.
	/// </summary>
	public class StatusBarDrawItemEventArgs : DrawItemEventArgs {

		private StatusBarPanel panel;

		/// --- Constructor ---
		public StatusBarDrawItemEventArgs(Graphics g, Font font, 
			Rectangle r, int itemId, DrawItemState itemState, 
			StatusBarPanel panel, Color foreColor, Color backColor)
			: base(g, font, r, itemId, itemState, foreColor, backColor) {
			this.panel = panel;
		}

		public StatusBarDrawItemEventArgs(Graphics g, Font font, 
			Rectangle r, int itemId, DrawItemState itemState, StatusBarPanel panel)
			: base(g, font, r, itemId, itemState) {
			this.panel = panel;
		}
		
		#region Public Properties
		public StatusBarPanel Panel 
		{
			get {
				return panel;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two StatusBarDrawItemEventArgs objects.
		///	The return value is based on the equivalence of
		///	the BackColor, Bounds, Font, ForeColor, Graphics,
		///	Index, Panel, and State properties of the two
		///	StatusBarDrawItemEventArgs.
		/// </remarks>
		public static bool operator == (StatusBarDrawItemEventArgs objA, StatusBarDrawItemEventArgs objB) 
		{
			return ((objA.panel == objB.panel) && ((DrawItemEventArgs) objA == (DrawItemEventArgs) objB));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two StatusBarDrawItemEventArgs objects.
		///	The return value is based on the equivalence of
		///	the BackColor, Bounds, Font, ForeColor, Graphics,
		///	Index, Panel, and State properties of the two
		///	StatusBarDrawItemEventArgs.
		/// </remarks>
		public static bool operator != (StatusBarDrawItemEventArgs objA, StatusBarDrawItemEventArgs objB) 
		{
			return ((objA.panel != objB.panel) || ((DrawItemEventArgs) objA != (DrawItemEventArgs) objB));
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	StatusBarDrawItemEventArgs and another object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is StatusBarDrawItemEventArgs))return false;
			return (this == (StatusBarDrawItemEventArgs) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		///	Returns DrawItemEventArgs.GetHashCode().
		/// </remarks>
		public override int GetHashCode () 
		{
			// FIXME: In a perfect world, get hashcode would include
			//		  Panel, but this shouldbe good enough.
			return base.GetHashCode();
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the StatusBarDrawItemEventArgs as a string.
		/// </remarks>
		public override string ToString () 
		{
			return base.ToString() + panel.ToString();
		}
		#endregion

	}
}
