//
// System.Windows.Forms.CheckBox.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows check box.
	/// </summary>

	[MonoTODO]
	public class CheckBox : ButtonBase {

		// private fields
		Appearance appearance;
		bool autoCheck;
		ContentAlignment checkAlign;
		bool _checked;
		CheckState checkState;
		bool threeState;
		ContentAlignment textAlign;
		
		// --- Constructor ---
		public CheckBox() : base() 
		{
			appearance = Appearance.Normal;
			autoCheck = true;
			checkAlign = ContentAlignment.MiddleLeft;
			_checked = false;
			checkState = CheckState.Unchecked;
			threeState = false;
			textAlign = ContentAlignment.MiddleCenter;

			SubClassWndProc_ = true;
		}
		
		// --- CheckBox Properties ---
		public Appearance Appearance {
			get { return appearance; }
			set { appearance=value; }
		}
		
		public bool AutoCheck {
			get { return autoCheck; }
			set { autoCheck = value; }
		}
		
		public ContentAlignment CheckAlign {
			get { return checkAlign; }
			set { checkAlign=value; }
		}
		
		public bool Checked {
			get { return _checked; }
			set { 
				if( _checked != value) {
					CheckState = (value) ? CheckState.Checked : CheckState.Unchecked;
				}
			}
		}
		
		public CheckState CheckState {
			get { return checkState; }
			set { 
				if( checkState != value) {

					checkState = value; 
					bool oldChecked = _checked;

					if( checkState == CheckState.Unchecked) {
						_checked = false;
					}
					else {
						_checked = true;
					}

					if( oldChecked != _checked) { 
						OnCheckedChanged(new EventArgs());
					}

					OnCheckStateChanged(new EventArgs());
				}
			}
		}
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
	
				createParams.ClassName = "BUTTON";

				createParams.Style = (int) (
					(int)WindowStyles.WS_CHILD | 
					(int)WindowStyles.WS_VISIBLE | 
					(int)ButtonStyles.BS_CHECKBOX |
					(int)WindowStyles.WS_CLIPSIBLINGS |
					(int)SS_Static_Control_Types.SS_LEFT );

				if( autoCheck) {
					createParams.Style |= (int)ButtonStyles.BS_AUTOCHECKBOX;
				}
				return createParams;
			}
		}
		
		[MonoTODO]
		protected override Size DefaultSize {
			get { return new Size(100,20); }
		}
		
		[MonoTODO]
		public override ContentAlignment TextAlign {
			get {
				return textAlign;
			}
			set {
				textAlign = value;
			}
		}
		
		public bool ThreeState {
			get { return threeState; }
			set { threeState = value; }
		}
		
		
		
		
		// --- CheckBox methods ---

		// I do not think this is part of the spec
		//protected override AccessibleObject CreateAccessibilityInstance() 
		//{
		//	throw new NotImplementedException ();
		//}
		
		
		// [event methods]
		[MonoTODO]
		protected virtual void OnAppearanceChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected virtual void OnCheckedChanged(EventArgs e) 
		{
			//FIXME:
			if(CheckedChanged != null) {
				CheckedChanged( this, e);
			}
		}
		
		[MonoTODO]
		protected virtual void OnCheckStateChanged(EventArgs e) 
		{
			//FIXME:
			if(CheckStateChanged != null) {
				CheckStateChanged( this, e);
			}
		}
		
		[MonoTODO]
		protected override void OnClick(EventArgs e) 
		{
			CheckState = (CheckState)Win32.SendMessage(Handle, (int)ButtonMessages.BM_GETCHECK, 0, 0);
			base.OnClick(e);
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			//FIXME:
			base.OnHandleCreated(e);
			Win32.SendMessage(Handle, (int)ButtonMessages.BM_SETCHECK, (int)checkState, 0);
		}
		
		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs e) 
		{
			//FIXME:
			base.OnMouseUp(e);
		}
		// end of [event methods]
		
		
		[MonoTODO]
		protected override bool ProcessMnemonic(char charCode) 
		{
			//FIXME:
			return base.ProcessMnemonic(charCode);
		}
		
		[MonoTODO]
		//FIXME: do a better tostring
		public override string ToString() 
		{
			if(Checked){
				return "CheckBox" + " Checked";
			}
			else{
				return "CheckBox" +  " Unchecked";
			}
		}
		
		/// --- CheckBox events ---
		public event EventHandler AppearanceChanged;
		public event EventHandler CheckedChanged;
		public event EventHandler CheckStateChanged;
		
		/// --- public class CheckBox.CheckBoxAccessibleObject : ButtonBase.ButtonBaseAccessibleObject ---
		/// the class is not stubbed, cause it's only used for .NET framework
	}
}
