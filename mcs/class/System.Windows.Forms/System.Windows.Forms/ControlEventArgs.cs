//
// System.Windows.Forms.ControlEventArgs.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Completed by Dennis Hayes (dennish@raytek.com)
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms
{
	/// <summary>
	/// Provides data for the ControlAdded and ControlRemoved events.
	/// </summary>
	public class ControlEventArgs : EventArgs
	{
		private Control control;

		public ControlEventArgs(Control control) {
			this.control = control;
		}
		public Control Control {
			get { return control; }
		}
		public string ToString() {
			return base.ToString + control.ToString();
		}
		public int GetHashCode() {
			return unchecked(base.GetHashCode() * control.ToString());
		}
	}
}
