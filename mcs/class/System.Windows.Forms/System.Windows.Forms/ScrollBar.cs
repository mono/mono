//
// System.Windows.Forms.ScrollBar.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

//public void add_Click(EventHandler value);
//public void add_MouseDown(MouseEventHandler value);
//public void add_MouseMove(MouseEventHandler value);
//public void add_MouseUp(MouseEventHandler value);
//public void add_Paint(PaintEventHandler value);
//
//protected virtual void OnValueChanged(EventArgs e);
//public Font Font {get; set;}


using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

     public class ScrollBar : Control {

		//
		//  --- Constructor
		//

		public ScrollBar() : base()
		{
			//spec says tabstop defaults to false.
			base.TabStop = false;
			//base.window
		}

		//
		//  --- Public Properties
		//

		 //COMPACT FRAMEWORK
		[MonoTODO]
		public override Color BackColor {
			get {
				//FIXME:
				return base.BackColor;
			}
			set {
				//FIXME:
				base.BackColor = value;
			}
		}

		[MonoTODO]
		public override Image BackgroundImage {
			get {
				//FIXME:
				return base.BackgroundImage;
			}
			set {
				//FIXME:
				base.BackgroundImage = value;
			}
		}

		 //COMPACT FRAMEWORK
		 [MonoTODO]
		public override Color ForeColor {
			get {
				//FIXME:
				return base.ForeColor;
			}
			set {
				//FIXME:
				base.ForeColor = value;
			}
		}

		[MonoTODO]
		public new ImeMode ImeMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:		
			}
		}

		 //COMPACT FRAMEWORK
		[MonoTODO]
		public int LargeChange {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:		
			}
		}

		 //COMPACT FRAMEWORK
		 [MonoTODO]
		public int Maximum {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:		
			}
		}

		 //COMPACT FRAMEWORK
		 [MonoTODO]
		public int Minimum {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:		
			}
		}

		 //COMPACT FRAMEWORK
		 [MonoTODO]
		public int SmallChange {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:		
			}
		}

		 //COMPACT FRAMEWORK
		 public override string Text {
			 //Can't imagen what a scroll bar would do with text, so just call base.
			 get {
				 return base.Text;
			 }
			 set {
				 base.Text = value;
			 }
		 }

		 //COMPACT FRAMEWORK
		 [MonoTODO]
		public int Value {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:		
			}
		}

		//
		//  --- Public Methods
		//

		//COMPACT FRAMEWORK
		public override string ToString()
		{	
			 //replace with value, if implmeted as properity.
			return Value.ToString();
		}

		//
		//  --- Public Events
		//

		[MonoTODO]
		public event ScrollEventHandler Scroll;

		//Compact FrameWork
		public event EventHandler ValueChanged;

		//
		//  --- Protected Properties
		//

		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
				createParams.Caption = "";
				createParams.ClassName = "SCROLLBAR";
				createParams.Style = (int) WindowStyles.WS_OVERLAPPEDWINDOW;
				return createParams;
			}
		}

		[MonoTODO]
		protected override ImeMode DefaultImeMode {
			get {
				//FIXME:
				return base.DefaultImeMode;
			}
		}

		//
		//  --- Protected Methods
		//

		 //COMPACT FRAMEWORK
		[MonoTODO]
		protected override void OnEnabledChanged(EventArgs e)
		{
			//FIXME:
			base.OnEnabledChanged(e);
		}

		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			//FIXME:
			base.OnHandleCreated(e);
		}
	 }
}
