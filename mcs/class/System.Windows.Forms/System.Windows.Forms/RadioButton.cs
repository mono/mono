//
// System.Windows.Forms.RadioButton.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class RadioButton : ButtonBase {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public RadioButton()
		{
			
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public Appearance Appearance {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool AutoCheck {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ContentAlignment CheckAlign {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Checked {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override ContentAlignment TextAlign {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
		[MonoTODO]
		public void PerformClick()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Events
		//
		[MonoTODO]
		public event EventHandler AppearanceChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public event EventHandler CheckedChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);
 
				createParams.Caption = Text;
				createParams.ClassName = "RADIOBUTTON";
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
		protected override ImeMode DefaultImeMode {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnCheckedChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnClick(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnEnter(EventArgs e)
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
		[MonoTODO]
		protected override bool ProcessMnemonic(char charCode)
		{
			throw new NotImplementedException ();
		}

	 }
}
