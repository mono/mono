
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms {

	internal class MdiChildContext {

		private static Color titlebar_color;

		private int BorderWidth = 3;
		private int TitleBarHeight = 25;
		private Size MinTitleBarSize = new Size (115, 25);
		
		private Form form;
		private Button close_button;
		private Button maximize_button;
		private Button minimize_button;
		
		// moving windows
		private Point start;
		private State state;
		private FormPos sizing_edge;
		private Rectangle virtual_position;
		private Rectangle prev_bounds;
		private bool maximized;
		
		
		private enum State {
			Idle,
			Moving,
			Sizing,
		}

		[Flags]
		private enum FormPos {
			None,

			TitleBar = 1,

			Top = 2,
			Left = 4,
			Right = 8,
			Bottom = 16,

			TopLeft = Top | Left,
			TopRight = Top | Right,

			BottomLeft = Bottom | Left,
			BottomRight = Bottom | Right,

			AnyEdge = Top | Left | Right | Bottom,
		}

		public MdiChildContext (Form form)
		{
			titlebar_color = Color.FromArgb (255, 0, 0, 255);
			this.form = form;

			form.Paint += new PaintEventHandler (PaintWindowDecorations);

			minimize_button = new Button ();
			minimize_button.Bounds = new Rectangle (form.Width - 62,
					BorderWidth + 2, 18, 22);
			minimize_button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			minimize_button.Paint += new PaintEventHandler (PaintButtonHandler);
			minimize_button.Click += new EventHandler (OnMinimizeHandler);
			
			maximize_button = new Button ();
			maximize_button.Bounds = new Rectangle (form.Width - 44,
					BorderWidth + 2, 18, 22);
			maximize_button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			maximize_button.Paint += new PaintEventHandler (PaintButtonHandler);
			maximize_button.Click += new EventHandler (OnMaximizeHandler);
			
			close_button = new Button ();
			close_button.Bounds = new Rectangle (form.Width - 24,
					BorderWidth + 2, 18, 22);
			close_button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			close_button.Paint += new PaintEventHandler (PaintButtonHandler);
			close_button.Click += new EventHandler (CloseButtonClicked);

			form.Controls.AddImplicit (close_button);
			form.Controls.AddImplicit (maximize_button);
			form.Controls.AddImplicit (minimize_button);
		}

		public bool HandleMessage (ref Message m)
		{
			switch ((Msg)m.Msg) {
				//case Msg.WM_PAINT:
				//DrawWindowDecorations (form.CreateGraphics ());
				//break;

			case Msg.WM_LBUTTONDOWN:
				return HandleLButtonDown (form, ref m);

			case Msg.WM_MOUSEMOVE:
				return HandleMouseMove (form, ref m);
				 
			case Msg.WM_LBUTTONUP:
				HandleLButtonUp (ref m);
				break;
			}
			return false;
		}

		
		private bool HandleLButtonDown (Form form, ref Message m)
		{
			form.BringToFront ();

			int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int) m.LParam.ToInt32 ());
			FormPos pos = FormPosForCoords (x, y);

			start = new Point (x, y);
			virtual_position = form.Bounds;

			if (pos == FormPos.TitleBar) {
				HandleTitleBarDown (x, y);
				return true;
			}

			if (IsSizable) {
				SetCursorForPos (pos);
			
				if ((pos & FormPos.AnyEdge) == 0)
					return false;

				state = State.Sizing;
				sizing_edge = pos;
				form.Capture = true;
				return true;
			}

			return false;
		}

		private void HandleTitleBarDown (int x, int y)
		{
			state = State.Moving;			     
			form.Capture = true;
		}

		private bool HandleMouseMove (Form form, ref Message m)
		{
			switch (state) {
			case State.Moving:
				HandleWindowMove (m);
				return true;
			case State.Sizing:
				HandleSizing (m);
				return true;
			}

			if (IsSizable) {
				int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
				int y = Control.HighOrder ((int) m.LParam.ToInt32 ());
				FormPos pos = FormPosForCoords (x, y);

				SetCursorForPos (pos);

				state = State.Idle;

			}

			return false;
		}
	
		private void SetCursorForPos (FormPos pos)
		{
			switch (pos) {
			case FormPos.TopLeft:
			case FormPos.BottomRight:
				form.Cursor = Cursors.SizeNWSE;
				break;
			case FormPos.TopRight:
			case FormPos.BottomLeft:
				form.Cursor = Cursors.SizeNESW;
				break;
			case FormPos.Top:
			case FormPos.Bottom:
				form.Cursor = Cursors.SizeNS;
				break;
			case FormPos.Left:
			case FormPos.Right:
				form.Cursor = Cursors.SizeWE;
				break;
			default:
				form.Cursor = Cursors.Default;
				break;
			}
		}
	
		private void HandleWindowMove (Message m)
		{
			Point move = MouseMove (m);

			virtual_position.X = form.Left + move.X;
			virtual_position.Y = form.Top + move.Y;
			virtual_position.Width = form.Width;
			virtual_position.Height = form.Height;

			Graphics g = form.Parent.CreateGraphics ();
			DrawVirtualPosition (g);
		}

		private void HandleSizing (Message m)
		{
			Point move = MouseMove (m);
			Rectangle pos = virtual_position;
			int mw = MinTitleBarSize.Width + (BorderWidth * 2);
			int mh = MinTitleBarSize.Height + (BorderWidth * 2);
			
			if ((sizing_edge & FormPos.Top) != 0) {
				int height = form.Height - move.Y;
				if (height <= mh) {
					move.Y += height - mh;
					height = mh;
				}
				pos.Y = form.Top + move.Y;
				pos.Height = height;
			} else if ((sizing_edge & FormPos.Bottom) != 0) {
				int height = form.Height + move.Y;
				if (height <= mh)
					move.Y -= height - mh;
				pos.Height = form.Height + move.Y;
			}

			if ((sizing_edge & FormPos.Left) != 0) {
				int width = form.Width - move.X;
				if (width <= mw) {
					move.X += width - mw;
					width = mw;
				}
				pos.X = form.Left + move.X;
				pos.Width = width;
			} else if ((sizing_edge & FormPos.Right) != 0) {
				int width = form.Width + move.X;
				if (width <= mw)
					move.X -= width - mw;
				pos.Width = form.Width + move.X;
			}

			UpdateVP (pos);
		}

		private bool IsSizable {
			get {
				switch (form.FormBorderStyle) {
				case FormBorderStyle.Sizable:
				case FormBorderStyle.SizableToolWindow:
					return true;
				default:
					return false;
				}
			}
		}

		private void UpdateVP (Rectangle r)
		{
			UpdateVP (r.X, r.Y, r.Width, r.Height);
		}

		private void UpdateVP (Point loc, int w, int h)
		{
			UpdateVP (loc.X, loc.Y, w, h);
		}

		private void UpdateVP (int x, int y, int w, int h)
		{
			virtual_position.X = x;
			virtual_position.Y = y;
			virtual_position.Width = w;
			virtual_position.Height = h;

			Graphics g = form.Parent.CreateGraphics ();
			DrawVirtualPosition (g);
		}

		private void HandleLButtonUp (ref Message m)
		{
			if (state == State.Idle)
				return;

			form.Capture = false;

			// Clear the virtual position
			Graphics g = form.Parent.CreateGraphics ();
			g.Clear (form.Parent.BackColor);

			form.Bounds = virtual_position;
			state = State.Idle;
		}

		private void PaintWindowDecorations (object sender, PaintEventArgs pe)
		{
			Color color = titlebar_color;
			if (maximized)
				color = ThemeEngine.Current.ColorButtonFace;
			Rectangle tb = new Rectangle (BorderWidth, BorderWidth,
					form.Width - BorderWidth, TitleBarHeight);

			pe.Graphics.FillRectangle (new SolidBrush (color),
						BorderWidth, BorderWidth,
						form.Width - BorderWidth, TitleBarHeight);

			if (form.Text != null) {
				StringFormat format = new StringFormat ();
				format.LineAlignment = StringAlignment.Center;
				pe.Graphics.DrawString (form.Text, form.Font,
						new SolidBrush (form.ForeColor),
						tb, format);
			}

			if (form.Icon != null) {
				pe.Graphics.DrawIcon (form.Icon, BorderWidth, BorderWidth);
			}

			Pen bp = new Pen (ThemeEngine.Current.ColorButtonFace,
					BorderWidth);

			// HACK: kludge the borders around
			Rectangle border = form.ClientRectangle;
			border.X++;
			border.Y++;
			border.Width -= 4;
			border.Height -= 4;
			pe.Graphics.DrawRectangle (bp, border);

			Border3DStyle style = Border3DStyle.Raised | Border3DStyle.Bump;
			border = form.ClientRectangle;

			if (maximized) {
				style = Border3DStyle.SunkenInner;
				border.Y = TitleBarHeight + BorderWidth * 2;
				border.Height -= TitleBarHeight;
			}
			
			ControlPaint.DrawBorder3D (pe.Graphics, border,
					style,
					Border3DSide.Left | Border3DSide.Right |
					Border3DSide.Top | Border3DSide.Bottom);

		}

		private void PaintButtonHandler (object sender, PaintEventArgs pe)
		{
			if (sender == close_button) {
				ControlPaint.DrawCaptionButton (pe.Graphics,
						close_button.ClientRectangle,
						CaptionButton.Close,
						close_button.ButtonState);
			} else if (sender == maximize_button) {
				ControlPaint.DrawCaptionButton (pe.Graphics,
						maximize_button.ClientRectangle,
						CaptionButton.Maximize,
						maximize_button.ButtonState);
			} else if (sender == minimize_button) {
				ControlPaint.DrawCaptionButton (pe.Graphics,
						minimize_button.ClientRectangle,
						CaptionButton.Minimize,
						minimize_button.ButtonState);
			}
		}

		private void CloseButtonClicked (object sender, EventArgs e)
		{
			form.Close ();
			// form.Close should set visibility to false somewhere
			// in it's closing chain but currently does not.
			form.Visible = false;
		}

		private void OnMinimizeHandler (object sender, EventArgs e)
		{
			form.SuspendLayout ();
			form.Width = MinTitleBarSize.Width + (BorderWidth * 2);
			form.Height = MinTitleBarSize.Height + (BorderWidth * 2);
			form.ResumeLayout ();
		}

		private void OnMaximizeHandler (object sender, EventArgs e)
		{
			if (maximized) {
				form.Bounds = prev_bounds;
				maximized = false;
			} else {
				prev_bounds = form.Bounds;
				form.Bounds = form.Parent.Bounds;
				maximized = true;
			}
		}

		private Point NewLocation (Message m)
		{
			int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int) m.LParam.ToInt32 ());
			int x_move = x - start.X;
			int y_move = y - start.Y;

			return new Point (form.Left + x_move, form.Top + y_move);
		}

		private Point MouseMove (Message m)
		{
			int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int) m.LParam.ToInt32 ());
			int x_move = x - start.X;
			int y_move = y - start.Y;

			return new Point (x_move, y_move);
		}

		// For now just use a solid pen as it is 10 billion times
		// faster then using the hatch, and what we really need is invert
		private void DrawVirtualPosition (Graphics graphics)
		{			
			Pen pen = new Pen (Color.Black, 3);

			graphics.Clear (form.Parent.BackColor);
			graphics.DrawRectangle (pen, virtual_position);
			pen.Dispose ();
		}

		private FormPos FormPosForCoords (int x, int y)
		{
			if (y < TitleBarHeight + BorderWidth) {

				if (y > BorderWidth && x > BorderWidth &&
						x < form.Width - BorderWidth)
					return FormPos.TitleBar;

				if (x < BorderWidth || (x < 20 && y < BorderWidth))
					return FormPos.TopLeft;

				if (x > form.Width - BorderWidth ||
					(x > form.Width - 20 && y < BorderWidth))
					return FormPos.TopRight;

				if (y < BorderWidth)
					return FormPos.Top;

			} else if (y > form.Height - 20) {

				if (x < BorderWidth ||
						(x < 20 && y > form.Height - BorderWidth))
					return FormPos.BottomLeft;

				if (x > form.Width - BorderWidth ||
						(x > form.Width - 20 &&
						 y > form.Height - BorderWidth))
					return FormPos.BottomRight;

				if (y > form.Height - BorderWidth)
					return FormPos.Bottom;


			} else if (x < BorderWidth) {
				return FormPos.Left;
			} else if (x > form.Width - BorderWidth) {
				return FormPos.Right;
			}

			return FormPos.None;
		}
	}

}

