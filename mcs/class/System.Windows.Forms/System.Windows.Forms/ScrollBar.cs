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
//public Font Font {get; set;}


using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	// Implements the basic functionality of a scroll bar control.
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
			set { base.ImeMode = value; }
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

				if ( IsHandleCreated )
					setScrollRange ( Minimum, maximum );
			}
		}

		[MonoTODO]
		public int Minimum {
			get { return minimum; }
			set {
				minimum = value;

				if ( minimum > maximum )
					maximum = minimum;

				if ( IsHandleCreated )
					setScrollRange ( minimum, Maximum );
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

				bool raiseEvent = ( value_ != value );

				value_ = value;
					
				if ( IsHandleCreated )
					setScrollPos ( value_ );

				if ( raiseEvent )
					OnValueChanged ( EventArgs.Empty );
			}
		}

		public override string ToString()
		{	
			return string.Format("{0}, Minimum: {1}, Maximum: {2}, Value: {3}",
						GetType( ).FullName.ToString( ), Minimum, Maximum, Value);
		}

		public event ScrollEventHandler Scroll;
		public event EventHandler ValueChanged;

		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
				createParams.ClassName = "SCROLLBAR";
				createParams.Style |= (int) (WindowStyles.WS_CHILD);
				return createParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get { return ImeMode.Disable; }
		}

		[MonoTODO]
		protected override void OnEnabledChanged(EventArgs e)
		{
			//FIXME:
			base.OnEnabledChanged(e);
		}

		protected virtual void OnValueChanged( EventArgs e )
		{
			if ( ValueChanged != null )
				ValueChanged ( this, e );
		}

		protected virtual void OnScroll( ScrollEventArgs se ) 
		{
			Value = se.NewValue;
			if ( Scroll != null )
				Scroll ( this, se );
		}

		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			//FIXME:
			base.OnHandleCreated(e);
			setScrollRange ( Minimum, Maximum );
			setScrollPos ( Value );
		}

		[MonoTODO]
		protected override void WndProc(ref Message m) {
			switch ( m.Msg ) {
				case Msg.WM_HSCROLL:
				case Msg.WM_VSCROLL:
					switch ( (ScrollBarRequests) Win32.LOW_ORDER ( m.WParam.ToInt32 ( ) ) ) {
					case ScrollBarRequests.SB_LEFT /*SB_TOP*/:
						fireScroll ( ScrollEventType.First, Minimum );
					break;
					case ScrollBarRequests.SB_RIGHT /*SB_BOTTOM*/:
						fireScroll ( ScrollEventType.Last, Minimum  );
					break;
					case ScrollBarRequests.SB_LINELEFT /*SB_LINEUP*/:
						fireScroll ( ScrollEventType.SmallDecrement, Value - SmallChange );
					break;
					case ScrollBarRequests.SB_LINERIGHT /*SB_LINEDOWN*/:
						fireScroll ( ScrollEventType.SmallIncrement, Value + SmallChange );
					break;
					case ScrollBarRequests.SB_PAGELEFT /*SB_PAGEUP*/:
						fireScroll ( ScrollEventType.LargeDecrement, Value - LargeChange );
					break;
					case ScrollBarRequests.SB_PAGERIGHT /*SB_PAGEDOWN*/:
						fireScroll ( ScrollEventType.LargeIncrement, Value + LargeChange );
					break;
					case ScrollBarRequests.SB_THUMBTRACK:
						fireScroll ( ScrollEventType.ThumbTrack, getTrackPos ( ) );
					break;
					case ScrollBarRequests.SB_THUMBPOSITION:
						fireScroll ( ScrollEventType.ThumbPosition, getScrollPos ( ) );
					break;
					case ScrollBarRequests.SB_ENDSCROLL:
						fireScroll ( ScrollEventType.EndScroll, getScrollPos ( ) );
					break;
					}
				break;
				default:
					CallControlWndProc( ref m );
				break;
			}
		}

		private void setScrollRange ( int minimum, int maximum )
		{
			SCROLLINFO scrinfo = new SCROLLINFO ( );
			scrinfo.cbSize = Marshal.SizeOf ( scrinfo );
			scrinfo.fMask = (int) ScrollBarInfoFlags.SIF_RANGE;
			scrinfo.nMin = minimum;
			scrinfo.nMax = maximum;
			Win32.SetScrollInfo ( Handle, (int) ScrollBarTypes.SB_CTL, ref scrinfo, 1 );
		}

		private void setScrollPos ( int val )
		{
			SCROLLINFO scrinfo = new SCROLLINFO ( );
			scrinfo.cbSize = Marshal.SizeOf ( scrinfo );
			scrinfo.fMask = (int) ScrollBarInfoFlags.SIF_POS;
			scrinfo.nPos = val;
			Win32.SetScrollInfo ( Handle, (int) ScrollBarTypes.SB_CTL, ref scrinfo, 1 );
		}

		private int getScrollPos ( )
		{
			SCROLLINFO scrinfo = new SCROLLINFO ( );
			scrinfo.cbSize = Marshal.SizeOf ( scrinfo );
			scrinfo.fMask = (int) ScrollBarInfoFlags.SIF_POS;
			Win32.GetScrollInfo ( Handle, (int) ScrollBarTypes.SB_CTL, ref scrinfo);
			return scrinfo.nPos;
		}

		private int getTrackPos (  )
		{
			SCROLLINFO scrinfo = new SCROLLINFO ( );
			scrinfo.cbSize = Marshal.SizeOf ( scrinfo );
			scrinfo.fMask = (int) ScrollBarInfoFlags.SIF_TRACKPOS;
			Win32.GetScrollInfo ( Handle, (int) ScrollBarTypes.SB_CTL, ref scrinfo);
			return scrinfo.nTrackPos;
		}

		private void fireScroll ( ScrollEventType type, int Val )
		{
			OnScroll ( new ScrollEventArgs ( type, clip ( Val ) ) );
		}

		private int clip ( int val )
		{
			if ( val < Minimum )
				return Minimum;
			if ( val > Maximum )
				return Maximum;
			return val;
		}
	 }
}
