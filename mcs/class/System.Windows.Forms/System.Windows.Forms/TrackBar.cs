//
// System.Windows.Forms.TrackBar
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class TrackBar : Control, ISupportInitialize {

		Orientation orientation = Orientation.Horizontal;
		int minimum = 0;
		int maximum = 10;
		int tickFrequency = 1;
		bool autosize = true;
		int val = 0;
		TickStyle tickStyle = TickStyle.BottomRight;
		int smallChange = 1;
		int largeChange = 5;

		//
		//  --- Public Constructors
		//
		[MonoTODO]
		public TrackBar()
		{
			Size = DefaultSize;
		}
		//
		// --- Public Properties
		//
		[MonoTODO]
		public bool AutoSize {
			get {
				return autosize;
			}
			set {
				autosize = value;
			}
		}

		public override Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
			}
		}

		public override Font Font {
			get {
				return base.Font;
			}
			set {
			}
		}

		public override Color ForeColor {
			get {
				return base.ForeColor;
			}
			set {
			}
		}

		[MonoTODO]
		public int LargeChange {
			get {
				return largeChange;
			}
			set {
				if ( value < 0 )
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));

				largeChange = value;

				if ( IsHandleCreated ) 
					Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETPAGESIZE, 0, value);
			}
		}
		[MonoTODO]
		public int Maximum {
			get {
				return maximum;
			}
			set {
				maximum = value;

				if ( maximum < minimum )
					minimum = maximum;

				if ( IsHandleCreated ) 
					Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETRANGEMAX, 1, value);
			}
		}
		[MonoTODO]
		public int Minimum {
			get {
				return minimum;
			}
			set {
				minimum = value;

				if ( minimum > maximum )
					maximum = minimum;

				if ( IsHandleCreated ) 
					Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETRANGEMIN, 1, value);
			}
		}
		[MonoTODO]
		public Orientation Orientation {
			get {
				return orientation;
			}
			set {
				int oldOrient = GetOrientation();

				orientation = value;

				ChangeWindowStyle( oldOrient, GetOrientation() );
				if( oldOrient != GetOrientation() )
					Size = new Size(Height, Width);
			}
		}
		[MonoTODO]
		public int SmallChange {
			get {
				return smallChange;
			}
			set {
				if ( value < 0 )
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));

				smallChange = value;

				if ( IsHandleCreated ) 
					Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETLINESIZE, 0, value);
			}
		}

		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}

		[MonoTODO]
		public int TickFrequency {
			get {
				return tickFrequency;
			}
			set {
				if ( value > 0 ) {
					tickFrequency = value;
					if ( IsHandleCreated ) 
						Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETTICFREQ, value, 0);
				}
			}
		}
		[MonoTODO]
		public TickStyle TickStyle {
			get {
				return tickStyle;
			}
			set {
				int OldStyle = GetTickStyle();
				tickStyle = value;
				ChangeWindowStyle( OldStyle, GetTickStyle() );
			}
		}

		[MonoTODO]
		public int Value {
			get {
				if ( IsHandleCreated ) {
					return Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_GETPOS, 0, 0);
				}
				return val;
			}
			set {
				if ( value < Minimum || value > Maximum )
					throw new ArgumentException(
						string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));

				bool raiseEvent = ( val != value ) && ( ValueChanged != null );

				val = value;
				if ( IsHandleCreated )
					Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETPOS, -1, val);

				if( raiseEvent )
					ValueChanged ( this, new EventArgs() );
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public void SetRange(int minValue, int maxValue) 
		{
			Minimum = minValue;
			Maximum = maxValue;

			if ( IsHandleCreated )
				Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETRANGE, 1,
							MakeLong(Minimum, Maximum));
		}
		[MonoTODO]
		public override string ToString() 
		{
			return string.Format("System.Windows.Forms.Trackbar, Minimum: {0}, Maximum: {1}, Value: {2}",
						Minimum, Maximum, Value);
		}
		
		// --- Public Events
		
		public event EventHandler Scroll;
		public event EventHandler ValueChanged;
        
        // --- Protected Properties
        //
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();

				createParams.Caption = Text;
				createParams.ClassName = "msctls_trackbar32";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE) | GetTickStyle() | 
					GetOrientation() |
					(int)TrackbarControlStyles.TBS_AUTOTICKS;

				if( TabStop ) 
					createParams.Style |= (int)WindowStyles.WS_TABSTOP;

				return createParams;
			}		
		}

		protected override ImeMode DefaultImeMode {
			get {
				return ImeMode.Disable;
			}
		}

		protected override Size DefaultSize {
			get {
				return new System.Drawing.Size(100,34);
			}
		}
		//
		// --- Protected Methods
		//
		[MonoTODO]
		protected override void CreateHandle() 
		{
			//FIXME: just to get it to run
			base.CreateHandle();
		}
		[MonoTODO]
		protected override bool IsInputKey(Keys keyData) 
		{
			if (	keyData == Keys.Left   || keyData == Keys.Right ||
				keyData == Keys.Up     || keyData == Keys.Down ||
				keyData == Keys.Home   || keyData == Keys.End ||
				keyData == Keys.PageUp || keyData == Keys.PageDown )
			return true;

			return IsInputKey(keyData);
		}
		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e) 
		{
			//FIXME:
			base.OnBackColorChanged(e);
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			base.OnHandleCreated(e);
			Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETRANGE, 1, MakeLong(Minimum, Maximum));
			Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETPOS, 1, val);
			Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETTICFREQ, TickFrequency, 0);
			Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETLINESIZE, 0, SmallChange);
			Win32.SendMessage(Handle, (int)TrackbarMessages.TBM_SETPAGESIZE, 0, LargeChange);
		}

		protected virtual void OnScroll(EventArgs e) 
		{
			if ( Scroll != null)
				Scroll ( this, e );
		}

		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) 
		{
			//FIXME:
			base.SetBoundsCore(x, y, width, height, specified);
		}

		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			switch ( m.Msg ) {
			case Msg.WM_HSCROLL:
			case Msg.WM_VSCROLL:
				OnScroll( new EventArgs() );
				if ( ValueChanged != null )
					ValueChanged (this, new EventArgs() );
			break;
			}
			base.WndProc(ref m);
		}

		[MonoTODO]
		void ISupportInitialize.BeginInit()
		{
			//FIXME:
		}

		[MonoTODO]
		void ISupportInitialize.EndInit(){
			//FIXME:
		}

		private int MakeLong(int lo, int hi)
		{
			return (hi << 16) | (lo & 0x0000ffff);
		}

		private int GetTickStyle()
		{
			int style = 0;

			switch ( tickStyle ) {
			case TickStyle.Both:
				style = (int)TrackbarControlStyles.TBS_BOTH;
			break;
			case TickStyle.BottomRight:
				style = (int)TrackbarControlStyles.TBS_BOTTOM | (int)TrackbarControlStyles.TBS_RIGHT;
			break;
			case TickStyle.TopLeft:
				style = (int)TrackbarControlStyles.TBS_TOP | (int)TrackbarControlStyles.TBS_LEFT;
			break;
			default:
				style = (int)TrackbarControlStyles.TBS_NOTICKS;
			break;
			};

			return style;
		}

		private int GetOrientation()
		{
			if ( Orientation == Orientation.Horizontal )
				return (int)TrackbarControlStyles.TBS_HORZ;
			else
				return (int)TrackbarControlStyles.TBS_VERT;
		}

		private bool ChangeWindowStyle(int Remove, int Add)
		{
			if( IsHandleCreated ) {
				int style = Win32.GetWindowLong( Handle, GetWindowLongFlag.GWL_STYLE ).ToInt32();
				int newStyle = (style & ~Remove) | Add;
				if (style != newStyle) {
					Win32.SetWindowLong(Handle, GetWindowLongFlag.GWL_STYLE, newStyle);
					return true;
				}
			}
			return false;
		}
	}
}
