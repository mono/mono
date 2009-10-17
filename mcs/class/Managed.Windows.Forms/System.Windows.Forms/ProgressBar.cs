// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (C) 2004-2006 Novell, Inc.
//
// Authors:
//		Jordi Mas i Hernandez	jordi@ximian.com
//		Peter Dennis Bartok	pbartok@novell.com
//
//

using System.Drawing;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[DefaultProperty ("Value")]
#if NET_2_0
	[DefaultBindingProperty ("Value")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
#endif
	public
#if !NET_2_0
	sealed
#endif
	class ProgressBar : Control
	{
		#region Local Variables
		private int maximum;
		private int minimum;
		internal int step;
		internal int val;
		internal DateTime start = DateTime.Now;
		internal Rectangle client_area = new Rectangle ();
#if NET_2_0
		internal ProgressBarStyle style;
		Timer marquee_timer;
		bool right_to_left_layout;
#endif

#if NET_2_0
		private static readonly Color defaultForeColor = SystemColors.Highlight;
#endif
		#endregion	// Local Variables

		#region events

#if NET_2_0
		static object RightToLeftLayoutChangedEvent = new object ();
#endif
			
#if ONLY_1_1
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add { base.BackColorChanged += value; }
			remove { base.BackColorChanged -= value; }
		}
#endif
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}
		
#if NET_2_0
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add	{ base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}
#endif
			
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler CausesValidationChanged {
			add { base.CausesValidationChanged += value; }
			remove { base.CausesValidationChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick {
			add { base.DoubleClick += value; }
			remove { base.DoubleClick -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Enter {
			add { base.Enter += value; }
			remove { base.Enter -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler FontChanged {
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}
		
#if ONLY_1_1
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}
#endif
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Leave {
			add { base.Leave += value; }
			remove { base.Leave -= value; }
		}

#if NET_2_0
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new event MouseEventHandler MouseDoubleClick {
			add { base.MouseDoubleClick += value; }
			remove { base.MouseDoubleClick -= value; }
		}
			
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged {
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}
#endif
			
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}
		
#if ONLY_1_1
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler RightToLeftChanged {
			add { base.RightToLeftChanged += value; }
			remove { base.RightToLeftChanged -= value; }
		}
#endif
#if NET_2_0
		public event EventHandler RightToLeftLayoutChanged {
			add { Events.AddHandler (RightToLeftLayoutChangedEvent, value); }
			remove { Events.RemoveHandler (RightToLeftLayoutChangedEvent, value); }
		}
#endif
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion Events

		#region Public Constructors
		public ProgressBar()
		{
			maximum = 100;
			minimum = 0;
			step = 10;
			val = 0;

			base.Resize += new EventHandler (OnResizeTB);

			SetStyle (ControlStyles.UserPaint | 
				ControlStyles.Selectable | 
				ControlStyles.ResizeRedraw | 
				ControlStyles.Opaque
#if NET_2_0
				| ControlStyles.UseTextForAccessibility
#endif
				, false);

			force_double_buffer = true;
			
#if NET_2_0
			ForeColor = defaultForeColor;
#endif
		}
		#endregion	// Public Constructors

		#region Public Instance Properties

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool AllowDrop
		{
			get { return base.AllowDrop; }
			set {
				base.AllowDrop = value;
			}
		}

#if ONLY_1_1
		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fire a BackColorChanged event
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color BackColor
		{
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}
#endif

		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fire a BackgroundImageChanged event
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage
		{
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

#if NET_2_0
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
				get	{ return base.BackgroundImageLayout; }
				set { base.BackgroundImageLayout = value; }
		}
#endif
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool CausesValidation
		{
			get { return base.CausesValidation; }
			set { base.CausesValidation = value; }
		}

		protected override CreateParams CreateParams
		{
			get { return base.CreateParams;	}
		}

		protected override ImeMode DefaultImeMode
		{
			get { return base.DefaultImeMode; }
		}

		protected override Size DefaultSize
		{
			get { return ThemeEngine.Current.ProgressBarDefaultSize; }
		}

#if NET_2_0
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool DoubleBuffered {
				get { return base.DoubleBuffered; }
				set { base.DoubleBuffered = value; }
		}
#endif
		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fire a FontChanged event
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Font Font
		{
			get { return base.Font; }
			set { base.Font = value; }
		}

#if ONLY_1_1
		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fire a FontChanged event
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color ForeColor
		{
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}
#endif

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ImeMode ImeMode
		{
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}

		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue (100)]
		public int Maximum
		{
			get {
				return maximum;
			}
			set {
				if (value < 0)
#if NET_2_0
					throw new ArgumentOutOfRangeException ("Maximum", 
#else
					throw new ArgumentException(
#endif
						string.Format("Value '{0}' must be greater than or equal to 0.", value ));

				maximum = value;
				minimum = Math.Min (minimum, maximum);
				val = Math.Min (val, maximum);
				Refresh ();
			}
		}

		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue (0)]
		public int Minimum {
			get {
				return minimum;
			}
			set {
				if (value < 0)
#if NET_2_0
					throw new ArgumentOutOfRangeException ("Minimum", 
#else
					throw new ArgumentException(
#endif
						string.Format("Value '{0}' must be greater than or equal to 0.", value ));

				minimum = value;
				maximum = Math.Max (maximum, minimum);
				val = Math.Max (val, minimum);
				Refresh ();
			}
		}

#if NET_2_0
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}
			
		[Localizable(true)]
		[DefaultValue(false)]
		[MonoTODO ("RTL is not supported")]
		public virtual bool RightToLeftLayout {
				get { return right_to_left_layout;}
				set	{ 
					if (right_to_left_layout != value) {
						right_to_left_layout = value;
						OnRightToLeftLayoutChanged (EventArgs.Empty);
					}
				}		
		}
#endif
			
#if ONLY_1_1
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override RightToLeft RightToLeft
		{
			get { return base.RightToLeft; }
			set { base.RightToLeft = value; }
		}
#endif

		[DefaultValue (10)]
		public int Step
		{
			get { return step; }
			set {
				step = value;
				Refresh ();
			}
		}

#if NET_2_0
		[Browsable (true)]
		[DefaultValue (ProgressBarStyle.Blocks)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public ProgressBarStyle Style {
			get {
				return style;
			}

			set {
				if (value != ProgressBarStyle.Blocks && value != ProgressBarStyle.Continuous
						&& value != ProgressBarStyle.Marquee)
					throw new InvalidEnumArgumentException ("value", unchecked((int)value), typeof (ProgressBarStyle));
				if (style != value) {
					style = value;

					if (style == ProgressBarStyle.Marquee) {
						if (marquee_timer == null) {
							marquee_timer = new Timer ();
							marquee_timer.Interval = 10;
							marquee_timer.Tick += new EventHandler (marquee_timer_Tick);
						}
						marquee_timer.Start ();
					} else {
						if (marquee_timer != null) {
							marquee_timer.Stop ();
						}
						Refresh ();
					}
				}
			}
		}

		void marquee_timer_Tick (object sender, EventArgs e)
		{
			Invalidate ();
		}
		
		int marquee_animation_speed = 100;
		[DefaultValue (100)]
		public int MarqueeAnimationSpeed {
			get {
				return marquee_animation_speed;
			}

			set {
				marquee_animation_speed = value;
			}
		}
#endif

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool TabStop
		{
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Bindable(false)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Bindable(true)]
		[DefaultValue (0)]
		public int Value
		{
			get {
				return val;
			}
			set {
				if (value < Minimum || value > Maximum)
#if NET_2_0
					throw new ArgumentOutOfRangeException ("Value", string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));
#else
					throw new ArgumentException(string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));
#endif
				val = value;
				Refresh ();
			}
		}


		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		public void Increment (int value)
		{
#if NET_2_0
			if (Style == ProgressBarStyle.Marquee)
				throw new InvalidOperationException ("Increment should not be called if the style is Marquee.");
#endif
			int newValue = Value + value;

			if (newValue < Minimum)
				newValue = Minimum;

			if (newValue > Maximum)
				newValue = Maximum;

			Value = newValue;
			Refresh ();
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);

			UpdateAreas ();
		}

#if NET_2_0
		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}
			
		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged (e);
		}
			
		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}
			
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftLayoutChanged(EventArgs e)
		{
				EventHandler eh = (EventHandler) Events [RightToLeftLayoutChangedEvent];
				if (eh != null)
					eh (this, e);
		}
#endif
			
		public void PerformStep ()
		{
#if NET_2_0
			if (Style == ProgressBarStyle.Marquee)
				throw new InvalidOperationException ("PerformStep should not be called if the style is Marquee.");
#endif
			Increment (Step);
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void ResetForeColor ()
		{
			ForeColor = defaultForeColor;
		}
#endif

		public override string ToString()
		{
			return string.Format ("{0}, Minimum: {1}, Maximum: {2}, Value: {3}",
				GetType().FullName,
				Minimum.ToString (),
				Maximum.ToString (),
				Value.ToString () );
		}

		#endregion	// Public Instance Methods

		#region Private Instance Methods

		private void UpdateAreas ()
		{
			client_area.X = client_area.Y = 2;
			client_area.Width = Width - 4;
			client_area.Height = Height - 4;
		}

		private void OnResizeTB (Object o, EventArgs e)
		{
			if (Width <= 0 || Height <= 0)
				return;

			UpdateAreas ();
			Invalidate();	// Invalidate the full surface, blocks will not match
		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{
			ThemeEngine.Current.DrawProgressBar (pevent.Graphics, pevent.ClipRectangle, this);
		}

		#endregion
	}
}
