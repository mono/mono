//
// System.Windows.Forms.Button.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//	Dennis Hayes (dennish@raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows button control.
	/// </summary>

	public class Button : ButtonBase, IButtonControl {

		// private fields
		DialogResult dialogResult;

		// --- Constructor ---
		public Button() : base()
		{
			dialogResult = DialogResult.None;
		}
		
		// --- Properties ---
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "BUTTON";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE);
				window.CreateHandle (createParams);
				return createParams;
			}
		}
		
		// --- IButtonControl property ---
		public virtual DialogResult DialogResult {
			get { return dialogResult; }
			set { dialogResult = value; }
		}

		// --- IButtonControl method ---
		[MonoTODO]
		public virtual void NotifyDefault(bool value) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void PerformClick() 
		{
			throw new NotImplementedException ();
		}
		
		// --- Button methods for events ---
		protected override void OnClick(EventArgs e) 
		{
			base.OnClick (e);
		}
		
		protected override void OnMouseUp(MouseEventArgs e) 
		{
			base.OnMouseUp (e);
		}
		
		// --- Button methods ---
		protected override bool ProcessMnemonic (char charCode) 
		{
			return base.ProcessMnemonic (charCode);
		}
		
		[MonoTODO]
		public override string ToString () 
		{
			throw new NotImplementedException ();
		}
		
		protected override void WndProc (ref Message m) 
		{
			base.WndProc (ref m);
		}
		
		/// --- Button events ---
		/// commented out, cause it only supports the .NET Framework infrastructure
		/*
		[MonoTODO]
		public new event EventHandler DoubleClick {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		*/
	}
}
