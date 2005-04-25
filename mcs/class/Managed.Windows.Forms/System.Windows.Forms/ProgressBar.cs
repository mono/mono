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
// Copyright (C) 2004-2005 Novell, Inc.
//
// Authors:
//		Jordi Mas i Hernandez	jordi@ximian.com
//
//


using System.Drawing;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	[DefaultProperty ("Value")]
	public sealed class ProgressBar : Control
	{
		#region Local Variables
		private int maximum;
		private int minimum;
		internal int step;
		internal int val;		
		internal Rectangle client_area = new Rectangle ();
		#endregion	// Local Variables

		#region events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]		
		public new event EventHandler BackColorChanged;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler CausesValidationChanged;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Enter;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler FontChanged;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Leave;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler RightToLeftChanged;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged;
		#endregion Events

		#region Public Constructors
		public ProgressBar()
		{
			maximum = 100;
			minimum = 0;
			step = 10;
			val = 0;

			base.Paint += new PaintEventHandler (OnPaintPB);
			base.Resize += new EventHandler (OnResizeTB);

			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
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

		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fire a BackColorChanged event
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color BackColor
		{
			get { return base.BackColor; }
			set { BackColor = value; }
		}

		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fire a BackgroundImageChanged event
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage
		{
			get { return base.BackgroundImage; }
			set {BackgroundImage = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool CausesValidation
		{
			get { return base.CausesValidation; }
			set {
				if (base.CausesValidation == value)
					return;

				CausesValidation = value;
				if (CausesValidationChanged != null)
					CausesValidationChanged (this, new EventArgs ());
			}
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

		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fire a FontChanged event
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Font Font
		{
			get { return base.Font;	}
			set { base.Font = value; }
		}

		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fire a FontChanged event
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color ForeColor
		{
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ImeMode ImeMode
		{
			get { return base.ImeMode; }
			set
			{
				if (value == base.ImeMode)
					return;

				base.ImeMode = value;
				if (ImeModeChanged != null)
					ImeModeChanged (this, EventArgs.Empty);
			}
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
					throw new ArgumentException(
						string.Format("Value '{0}' must be greater than or equal to 0.", value ));

				maximum = value;
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
					throw new ArgumentException(
						string.Format("Value '{0}' must be greater than or equal to 0.", value ));

				minimum = value;
				Refresh ();
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override RightToLeft RightToLeft
		{
			get { return base.RightToLeft; }
			set {
				if (base.RightToLeft == value)
					return;

				base.RightToLeft = value;

				if (RightToLeftChanged != null)
					RightToLeftChanged (this, EventArgs.Empty);

			}
		}

		[DefaultValue (10)]
		public int Step
		{
			get { return step; }
			set {
				step = value;
				Refresh ();
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool TabStop
		{
			get { return base.TabStop; }
			set {
				if (base.TabStop == value)
					return;

				base.TabStop = value;

				if (TabStopChanged != null)
					TabStopChanged (this, EventArgs.Empty);

			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Bindable(false)]
		public override string Text
		{
			get { return base.Text; }
			set
			{
				if (value == base.Text)
					return;

				if (TextChanged != null)
					TextChanged (this, EventArgs.Empty);

				Refresh ();
			}
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
					throw new ArgumentException(
						string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));

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

			CreateBuffers (Width, Height);
			
		}

		public void PerformStep ()
		{
			if (Value >= Maximum)
				return;

			Value = Value + Step;
			Refresh ();
		}

		public override string ToString()
		{
			return string.Format ("{0}, Minimum: {1}, Maximum: {2}, Value: {3}",
				GetType().FullName.ToString (),
				Maximum.ToString (),
				Minimum.ToString (),
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
    		}

		private void OnPaintPB (Object o, PaintEventArgs pevent)
		{
                        ThemeEngine.Current.DrawProgressBar (pevent.Graphics, pevent.ClipRectangle, this);
		}		
		
		#endregion
	}
}
