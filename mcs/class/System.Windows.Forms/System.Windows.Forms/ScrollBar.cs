//
// System.Windows.Forms.ScrollBar.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
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
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

     public class ScrollBar : Control {

		int value_;
		int minimum;
		int maximum;
		int largeChange;
		int smallChange;

		public ScrollBar() : base()
		{
			//spec says tabstop defaults to false.
			base.TabStop = false;
			value_ = 0;
			minimum = 0;
			maximum = 100;
			largeChange = 10;
			smallChange = 1;	
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Color BackColor {
			get { return base.BackColor; }
			set { base.BackColor = value;}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Image BackgroundImage {
			get { return base.BackgroundImage;  }
			set { base.BackgroundImage = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Color ForeColor {
			get { return base.ForeColor;  }
			set { base.ForeColor = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = ImeMode.NoControl; }
		}

		[MonoTODO]
		public int LargeChange {
			get { return largeChange; }
			set {
				if ( value < 0 )
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));

				largeChange = value;	
			}
		}

		[MonoTODO]
		public int Maximum {
			get { return maximum; }
			set {
				maximum = value;

				if ( maximum < minimum )
					minimum = maximum;
			}
		}

		[MonoTODO]
		public int Minimum {
			get { return minimum; }
			set {
				minimum = value;

				if ( minimum > maximum )
					maximum = minimum;
			}
		}

		[MonoTODO]
		public int SmallChange {
			get { return smallChange; }
			set {
				if ( value < 0 )
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));

				smallChange = value;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override string Text {
			 get { return base.Text;  }
			 set { base.Text = value; }
		 }

		[MonoTODO]
		public int Value {
			get { return value_; }
			set {
				if ( value < Minimum || value > Maximum )
					throw new ArgumentException(
						string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));

				bool raiseEvent = ( value_ != value ) && ( ValueChanged != null );

				value_ = value;
			}
		}

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

		public event EventHandler ValueChanged;

		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
				createParams.ClassName = "SCROLLBAR";
				createParams.Style |= (int) (WindowStyles.WS_VISIBLE | WindowStyles.WS_CHILD);
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
