//
// System.Windows.Forms.StatusBarPanelClickEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms
{
	/// <summary>
	/// Provides data for the PanelClick event.
	/// </summary>
	public class StatusBarPanelClickEventArgs : MouseEventArgs
	{
		private StatusBarPanel panel;

		/// --- Constructor ---
		public StatusBarPanelClickEventArgs(StatusBarPanel panel, MouseButtons button, int clicks, int x, int y) : base(button, clicks, x, y, 0) {
			this.panel = panel;
		}

		/// --- Properties ---
		public StatusBarPanel StatusBarPanel {
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
		///	Compares two StatusBarPanelClickEventArgs objects.
		///	The return value is based on the equivalence of
		///	the StatusBarPanel, Button, Clicks, X, and Y
		///	properties of the two StatusBarPanelClickEventArgs.
		/// </remarks>
		public static bool operator == (StatusBarPanelClickEventArgs objA, StatusBarPanelClickEventArgs objB) {
			return ((objA.panel == objB.panel) && (objA.Button == objB.Button) && (objA.Clicks == objB.Clicks) && (objA.X == objB.X) && (objA.Y == objB.Y));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two StatusBarPanelClickEventArgs objects.
		///	The return value is based on the equivalence of
		///	the StatusBarPanel, Button, Clicks, X, and Y
		///	properties of the two StatusBarPanelClickEventArgs.
		/// </remarks>
		public static bool operator != (StatusBarPanelClickEventArgs objA, StatusBarPanelClickEventArgs objB) {
			return ((objA.panel != objB.panel) || (objA.Button != objB.Button) || (objA.Clicks != objB.Clicks) || (objA.X != objB.X) || (objA.Y != objB.Y));
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	StatusBarPanelClickEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object o) {
			if (!(o is StatusBarPanelClickEventArgs))return false;
			return (this == (StatusBarPanelClickEventArgs) o);
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
		///	Formats the StatusBarPanelClickEventArgs as a string.
		/// </remarks>
		//[MonoTODO]
		//public override string ToString () {
		//	throw new NotImplementedException();
		//}
  
		//end add. remove comment after modifing for this class

	}
}