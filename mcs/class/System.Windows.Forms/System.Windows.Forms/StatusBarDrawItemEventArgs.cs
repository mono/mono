//
// System.Windows.Forms.StatusBarDrawItemEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms
{
	/// <summary>
	/// Provides data for the DrawItem event.
	/// </summary>
	public class StatusBarDrawItemEventArgs : DrawItemEventArgs
	{
		private StatusBarPanel panel;

		/// --- Constructor ---
		public StatusBarDrawItemEventArgs(Graphics g, Font font, Rectangle r, int itemId, DrawItemState itemState, StatusBarPanel panel)
		: base(g, font, r, itemId, itemState) {
			this.panel = panel;
		}

		/// --- Properties ---
		public StatusBarPanel Panel {
			get {
				return panel;
			}
		}

		// add. remove comment after modifing for this class
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
		public static bool operator == (StatusBarDrawItemEventArgs objA, StatusBarDrawItemEventArgs objB) {
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
		public static bool operator != (StatusBarDrawItemEventArgs objA, StatusBarDrawItemEventArgs objB) {
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
		public override bool Equals (object o) {
			if (!(o is StatusBarDrawItemEventArgs))return false;
			return (this == (StatusBarDrawItemEventArgs) o);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		//[MonoTODO]
		//public override int GetHashCode () {
		//	throw new NotImplementedException();
		//}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the StatusBarDrawItemEventArgs as a string.
		/// </remarks>
		//[MonoTODO]
		//public override string ToString () {
		//	throw new NotImplementedException();
		//}
  
		//end add. remove comment after modifing for this class

	}
}