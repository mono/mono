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

					updateCheck ( );
					OnCheckedChanged ( EventArgs.Empty );
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
			Checked = !Checked;
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
			int res = Win32.SendMessage ( Handle, (int)ButtonMessages.BM_GETCHECK, 0, 0);

			bool check = Checked;

			if ( res == (int) NativeButtonState.BST_CHECKED ) 
				check = true;
			else if ( res == (int) NativeButtonState.BST_UNCHECKED ) 
				check = false;

			if ( checked_ != check ) {
				checked_ = check;
				OnCheckedChanged ( EventArgs.Empty );
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
			updateCheck ( );
		}

		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			throw new NotImplementedException ();
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

		private void updateCheck ( ) {
			if ( IsHandleCreated ) 
				Win32.SendMessage ( Handle, (int) ButtonMessages.BM_SETCHECK, 
							Checked ? (int) NativeButtonState.BST_CHECKED :
								  ( int ) NativeButtonState.BST_UNCHECKED, 0 );
		}
	 }
}
