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

		public ScrollBar()
		{
			//
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
		 [MonoTODO]
		public override string Text {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
		 [MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
