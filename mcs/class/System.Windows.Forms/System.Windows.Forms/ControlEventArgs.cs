//
// System.Windows.Forms.ControlEventArgs.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)

// (C) Ximian, Inc., 2002


namespace System.Windows.Forms
{
	/// <summary>
	/// Provides data for the ControlAdded and ControlRemoved events.
	/// ToDo note:
	///  - no methods are implemented
	/// </summary>

	public class ControlEventArgs : EventArgs
	{
		Control control;

		#region Constructors
		public ControlEventArgs(Control control) {
			this.control = control;
		}
		#endregion
		
		#region Properties
		public Control Control {
			get { return control; }
		}
		#endregion
		public string ToString() {
			return base.ToString + control.ToString();
		}
		public int GetHashCode() {
			return unchecked(base.GetHashCode() * control.ToString());
		}

	}
}
