//
// System.Windows.Forms.ControlEventArgs.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)

// (C) Ximian, Inc., 2002


namespace System.Windows.Forms {

	/// <summary>
	/// Complete.
	/// </summary>

	public class ControlEventArgs : EventArgs {

		#region Fields
		Control control;
		#endregion

		public ControlEventArgs(Control control) 
		{
			this.control = control;
		}
		
		#region Public Properties
		public Control Control 
		{
			get 
				{ 
					return control; 
				}
		}
		#endregion
	}
}
