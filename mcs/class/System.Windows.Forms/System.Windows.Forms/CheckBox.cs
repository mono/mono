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
	/// ToDo note:
	///  - nothing is implemented
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
			set { _checked=value; }
		}
		
		public CheckState CheckState {
			get { return checkState; }
			set { checkState = value; }
		}
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);
 
				createParams.Caption = Text;
				createParams.ClassName = "CHECKBOX";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				//		createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					(int)WindowStyles.WS_CHILD | 
					(int)WindowStyles.WS_VISIBLE | (int)SS_Static_Control_Types.SS_LEFT );
				window.CreateHandle (createParams);
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
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnCheckedChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnCheckStateChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnClick(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs mevent) 
		{
			throw new NotImplementedException ();
		}
		// end of [event methods]
		
		
		[MonoTODO]
		protected override bool ProcessMnemonic(char charCode) 
		{
			throw new NotImplementedException ();
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
