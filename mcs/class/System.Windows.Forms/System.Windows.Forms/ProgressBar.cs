//
// System.Windows.Forms.ProgressBar
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;
using System.Drawing.Printing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows progress bar control.
	/// </summary>

	public sealed class ProgressBar : Control {

		#region Fields
		int maximum = 100;
		int minimum = 0;
		int step    = 10;
		int val     = 0;
		#endregion
		
		#region Constructor

		public ProgressBar() {
		}

		#endregion
		
		#region Properties
		
		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override bool AllowDrop {
			get {	return base.AllowDrop;	}
			set {	base.AllowDrop = value;	}
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Color BackColor {
			get {	return base.BackColor;	}
			set {	base.BackColor = value;	}
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Image BackgroundImage 	{
			get {	return base.BackgroundImage; }
			set {	base.BackgroundImage = value; }
		}

		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new bool CausesValidation {get; set;}
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = "msctls_progress32";

				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE |
					WindowStyles.WS_CLIPCHILDREN |
					WindowStyles.WS_CLIPSIBLINGS );

				return createParams;
			}		
		}

		
		protected override ImeMode DefaultImeMode {
			get {	return ImeMode.Disable;	}
		}

		protected override Size DefaultSize {
			get {	return new Size(100, 23); }
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Font Font {
			get {	return base.Font;  }
			set {	base.Font = value; }
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Color ForeColor  {
			get {	return base.ForeColor;	}
			set {	base.ForeColor = value; }
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new ImeMode ImeMode {get; set;}
		
		public int Maximum {
			get {
				return maximum;
			}
			set {
				if ( value < 0 )
					throw new ArgumentException( 
						string.Format("Value '{0}' must be greater than or equal to 0.", value ));
				maximum = value;
				if ( IsHandleCreated )
					Win32.SendMessage( Handle, (int)ProgressBarMessages.PBM_SETRANGE32, Minimum, Maximum );
			}
		}
		
		public int Minimum {
			get {
				return minimum;
			}
			set {
				if ( value < 0 )
					throw new ArgumentException( 
						string.Format("Value '{0}' must be greater than or equal to 0.", value ));
				minimum = value;
				if ( IsHandleCreated )
					Win32.SendMessage( Handle, (int)ProgressBarMessages.PBM_SETRANGE32, Minimum, Maximum );
			}
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new bool TabStop {get; set;}
		/// 
		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override RightToLeft RightToLeft {
			get {	return base.RightToLeft; }
			set {	base.RightToLeft = value; }
		}
		
		public int Step {
			get { 	return step; }
			set {
				step = value;
				if ( IsHandleCreated )
					Win32.SendMessage( Handle, (int)ProgressBarMessages.PBM_SETSTEP, Step, 0 );
			}
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override string Text {
			get { 	return base.Text; }
			set {	base.Text = value; }
		}
		
		public int Value {
			get {
				if ( IsHandleCreated )
					val = (int)Win32.SendMessage ( Handle, (int)ProgressBarMessages.PBM_GETPOS, 0, 0 );
				return val;
			}
			set {
				if ( value < Minimum || value > Maximum )
					throw new ArgumentException(
						string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));

				val = value; 

				if ( IsHandleCreated )
					Win32.SendMessage(Handle, (int)ProgressBarMessages.PBM_SETPOS, val, 0);
			}
		}
		#endregion
		
		#region Methods

		protected override void CreateHandle() 	{
			initCommonControlsLibrary ( );
			base.CreateHandle();
		}
		
		public void Increment(int value) {
			int newValue = Value + value;
			if ( newValue < Minimum )
				newValue = Minimum;
			if ( newValue > Maximum )
				newValue = Maximum;
			Value = newValue;
		}
		
		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);
			Win32.SendMessage(Handle, (int)ProgressBarMessages.PBM_SETRANGE32, Minimum, Maximum);
			Win32.SendMessage(Handle, (int)ProgressBarMessages.PBM_SETPOS, Value, 0);
			Win32.SendMessage(Handle, (int)ProgressBarMessages.PBM_SETSTEP, Step, 0);
		}
		
		public void PerformStep() {
			if ( IsHandleCreated )
				Win32.SendMessage(Handle, (int)ProgressBarMessages.PBM_STEPIT, 0, 0);
		}
		
		public override string ToString() {
			return string.Format ("{0}, Minimum: {1}, Maximum: {2}, Value: {3}", 
						GetType().FullName.ToString (),
						Maximum.ToString (),
						Minimum.ToString (),
						Value.ToString () );
		}
		#endregion

		private void initCommonControlsLibrary	( ) {
			if ( !RecreatingHandle ) {
				INITCOMMONCONTROLSEX	initEx = new INITCOMMONCONTROLSEX();
				initEx.dwICC = CommonControlInitFlags.ICC_PROGRESS_CLASS;
				Win32.InitCommonControlsEx(initEx);
			}
		}
	}
}
