//
// System.Windows.Forms.RadioButton.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//   implemented by Aleksey Ryabchuk (ryabchuk@yahoo.com)
// (C) 2002/3 Ximian, Inc
//
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	// Represents a Windows radio button
	// </summary>

    public class RadioButton : ButtonBase {

		Appearance appearance;
		bool       autoCheck;
		ContentAlignment checkAlign;
		bool       checked_;

		public RadioButton()
		{
			SubClassWndProc_ = true;
			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			callWinControlProcMask &= ~(CallWinControlProcMask.MOUSE_MESSAGES | CallWinControlProcMask.KEYBOARD_MESSAGES);
			
			appearance = Appearance.Normal;
			autoCheck  = true;
			checkAlign = ContentAlignment.MiddleLeft;
			checked_   = false;
			TextAlign  = ContentAlignment.MiddleLeft;
		}

		public Appearance Appearance {
			get {	return appearance; }
			set {
				if ( !Enum.IsDefined ( typeof(Appearance), value ) )
					throw new InvalidEnumArgumentException( "Appearance",
						(int)value,
						typeof(Appearance));

				if ( appearance != value ) {
					int oldStyle = AppearanceStyle;
					appearance = value;
					
					if ( IsHandleCreated )
						Win32.UpdateWindowStyle ( Handle, oldStyle, AppearanceStyle );

					if ( AppearanceChanged != null )
						AppearanceChanged ( this, EventArgs.Empty );
				}
			}
		}

		public bool AutoCheck {
			get {	return autoCheck; }
			set {
				if ( autoCheck != value ) {
					int oldStyle = AutoCheckStyle;
					autoCheck = value;

					if ( IsHandleCreated )
						Win32.UpdateWindowStyle ( Handle, oldStyle, AutoCheckStyle );
				}
			}
		}

		[MonoTODO]
		public ContentAlignment CheckAlign {
			get {	return checkAlign; }
			set {
				if ( !Enum.IsDefined ( typeof(ContentAlignment), value ) )
					throw new InvalidEnumArgumentException( "CheckAlign",
						(int)value,
						typeof(Appearance));

				if ( checkAlign != value ) {
					checkAlign = value;
				}
			}
		}

		public bool Checked {
			get {	return checked_; }
			set {
				if ( checked_ != value ) {
					checked_ = value;
					OnCheckedChanged ( EventArgs.Empty );
					Invalidate ();
				}
			}
		}

		[MonoTODO]
		public override ContentAlignment TextAlign {
			get {	return base.TextAlign;	}
			set {	base.TextAlign = value;	}
		}

		public void PerformClick()
		{
			OnClick ( EventArgs.Empty );
		}

		public override string ToString()
		{
			return GetType().FullName.ToString () + ", Checked: " + Checked.ToString ( );
		}

		public event EventHandler AppearanceChanged;
		public event EventHandler CheckedChanged;

		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
 
				createParams.ClassName = "BUTTON";

				createParams.Style = (int) (
					(int)WindowStyles.WS_CHILD | 
					(int)WindowStyles.WS_VISIBLE );

				createParams.Style |= AutoCheckStyle | AppearanceStyle;
				createParams.Style |= (int)Win32.ContentAlignment2SystemButtonStyle( TextAlign );

				return createParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get {	return ImeMode.Disable;	}
		}

		protected override Size DefaultSize {
			get {	return new Size(104,24); }
		}

		[MonoTODO]
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnCheckedChanged(EventArgs e)
		{
			if ( CheckedChanged != null )
				CheckedChanged ( this, e );
		}

		[MonoTODO]
		protected override void OnClick(EventArgs e)
		{
			if (AutoCheck && !Checked) {
				Checked = true;
				foreach (Control ctr in Parent.Controls) {
					RadioButton rbtn = ctr as RadioButton;
					if (rbtn != null && rbtn != this) {
						rbtn.Checked = false;
					}
				}
			}
			base.OnClick ( e );
		}

		[MonoTODO]
		protected override void OnEnter(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated ( e );
		}

		protected override void OnMouseUp(MouseEventArgs e) 
		{
			OnClick (EventArgs.Empty);
			base.OnMouseUp(e);
		}
		
		[MonoTODO]
		protected override bool ProcessMnemonic(char charCode)
		{
			throw new NotImplementedException ();
		}

		private int AutoCheckStyle
		{
			get { return (int) ( AutoCheck ?  ButtonStyles.BS_AUTORADIOBUTTON : ButtonStyles.BS_RADIOBUTTON ); }
		}

		private int AppearanceStyle
		{
			get { return (int) ( Appearance == Appearance.Normal ? 0 : ButtonStyles.BS_PUSHLIKE ); }
		}

		internal override void ButtonPaint (PaintEventArgs e) 
		{
			Rectangle paintBounds = ClientRectangle;
			Bitmap bmp = new Bitmap( paintBounds.Width, paintBounds.Height,e.Graphics);
			Graphics paintOn = Graphics.FromImage(bmp);
			
			SolidBrush sb = new SolidBrush (BackColor);
			paintOn.FillRectangle (sb, paintBounds);
			sb.Dispose ();
			
			// FIXME: Width/Heigth of radiobutton are not correct
			int CheckSize = 12;
			Rectangle checkRect = new Rectangle (paintBounds.Left, paintBounds.Top, CheckSize, CheckSize);
			Rectangle textRect = new Rectangle (checkRect.Right + 3, paintBounds.Top, paintBounds.Width - checkRect.Width - 4, paintBounds.Height);
			if (0 != (CheckAlign & (ContentAlignment.BottomLeft | ContentAlignment.BottomCenter | ContentAlignment.BottomRight))) {
				checkRect.Y = paintBounds.Bottom - CheckSize;
			}
			else if(0 != (CheckAlign & (ContentAlignment.MiddleLeft | ContentAlignment.MiddleCenter | ContentAlignment.MiddleRight))) {
				checkRect.Y = paintBounds.Top + paintBounds.Height / 2 - CheckSize / 2;
			}
			
			if (0 != (CheckAlign & (ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight))) {
				checkRect.X = paintBounds.Right - CheckSize;
				textRect.X = paintBounds.Left;
			}
			else if(0 != (CheckAlign & (ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter))) {
				checkRect.X = paintBounds.Left + paintBounds.Width / 2 - CheckSize / 2;
				textRect.X = paintBounds.Left;
				textRect.Width = paintBounds.Width;
			}
			
			ButtonState state = ButtonState.Normal;
			if (FlatStyle == FlatStyle.Flat) {
				state |= ButtonState.Flat;
			}
			
			if (Checked) {
				state |= ButtonState.Checked;
			}
			
			ControlPaint.DrawRadioButton (paintOn, checkRect, state);
			
			Win32.DrawText(paintOn, Text, Font, ForeColor, textRect, TextAlign);
			
			if (Focused) {
				ControlPaint.DrawFocusRectangle (paintOn, textRect);
			}
			
			e.Graphics.DrawImage(bmp, 0, 0, paintBounds.Width, paintBounds.Height);
			paintOn.Dispose ();
			bmp.Dispose();
		}
	 }
}
