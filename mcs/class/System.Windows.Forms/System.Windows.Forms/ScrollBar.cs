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
	//	This is only a template.  Nothing is implemented yet.
	//
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
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override Image BackgroundImage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		 //COMPACT FRAMEWORK
		 [MonoTODO]
		public override Color ForeColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public new ImeMode ImeMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		 //COMPACT FRAMEWORK
		[MonoTODO]
		public int LargeChange {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		 //COMPACT FRAMEWORK
		 [MonoTODO]
		public int Maximum {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		 //COMPACT FRAMEWORK
		 [MonoTODO]
		public int Minimum {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		 //COMPACT FRAMEWORK
		 [MonoTODO]
		public int SmallChange {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
		public event ScrollEventHandler Scroll {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}

		//Compact FrameWork
		public event EventHandler ValueChanged;

		//
		//  --- Protected Properties
		//

		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				createParams.Caption = "";
				createParams.ClassName = "mono_scrollable_control";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
  				
				//if (parent != null)
				//	createParams.Parent = parent.Handle;
				//else 
				createParams.Parent = (IntPtr) 0;
	  
				createParams.Style = (int) WindowStyles.WS_OVERLAPPEDWINDOW;
	  
				return createParams;
			}
		}

		[MonoTODO]
		protected override ImeMode DefaultImeMode {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Protected Methods
		//

		 //COMPACT FRAMEWORK
		[MonoTODO]
		protected override void OnEnabledChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			throw new NotImplementedException ();
		}
	 }
}
