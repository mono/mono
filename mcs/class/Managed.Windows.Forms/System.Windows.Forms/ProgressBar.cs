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
// Copyright (C) 2004 Novell, Inc.
//
// Autors:
//		Jordi Mas i Hernandez	jordi@ximian.com
//
//


using System.Drawing;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	public sealed class ProgressBar : Control
	{
		#region Local Variables
		private int maximum;
		private int minimum;
		internal int step;
		internal int val;
		internal Rectangle paint_area = new Rectangle ();
		internal Rectangle client_area = new Rectangle ();
		#endregion	// Local Variables

		#region Events
		public new event EventHandler BackColorChanged;
		public new event EventHandler BackgroundImageChanged;
		public new event EventHandler CausesValidationChanged;
		public new event EventHandler DoubleClick;
		public new event EventHandler Enter;
		public new event EventHandler FontChanged;
		public new event EventHandler ForeColorChanged;
		public new event EventHandler ImeModeChanged;
		public new event KeyEventHandler KeyDown;
		public new event KeyPressEventHandler KeyPress;
		public new event KeyEventHandler KeyUp;
		public new event EventHandler Leave;
		public new event PaintEventHandler Paint;
		public new event EventHandler RightToLeftChanged;
		public new event EventHandler TabStopChanged;
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

		public override bool AllowDrop
		{
			get { return base.AllowDrop; }
			set {
				base.AllowDrop = value;
			}
		}

		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fires a BackColorChanged event
		public override Color BackColor
		{
			get { return base.BackColor; }
			set { BackColor = value; }
		}

		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fires a BackgroundImageChanged event
		public override Image BackgroundImage
		{
			get { return base.BackgroundImage; }
			set {BackgroundImage = value; }
		}

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
		// does not fires a FontChanged event
		public override Font Font
		{
			get { return base.Font;	}
			set { base.Font = value; }
		}

		// Setting this property in MS .Net 1.1 does not have any visual effect and it
		// does not fires a FontChanged event
		public override Color ForeColor
		{
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

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

		public int Step
		{
			get { return step; }
			set {
				step = value;
				Refresh ();
			}
		}

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
			Draw ();
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
			paint_area.X = paint_area.Y = 0;
			paint_area.Width = Width;
			paint_area.Height = Height;

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

		/* Disable background painting to avoid flickering, since we do our painting*/
		protected override void OnPaintBackground (PaintEventArgs pevent)
    		{
    			// None
    		}

		private void Draw ()
		{
			ThemeEngine.Current.DrawProgressBar (DeviceContext, this.ClientRectangle, this);
		}

		private void OnPaintPB (Object o, PaintEventArgs pevent)
		{
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			/* Copies memory drawing buffer to screen*/
			Draw ();
			pevent.Graphics.DrawImage (ImageBuffer, 0, 0);
		}

		#endregion
	}
}
