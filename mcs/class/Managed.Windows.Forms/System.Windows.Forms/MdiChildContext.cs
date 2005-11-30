using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms {

	internal class MdiChildContext {

		private static readonly int MdiBorderStyle = 0xFFFF;
		private static Color titlebar_color;

		
		private MainMenu merged_menu;
		private int BorderWidth = 3;
//		private int TitleBarHeight = 26;
//		private int ToolTitleBarHeight = 19;
		private Size MinTitleBarSize = new Size (115, 25);

		private Form form;
		private MdiClient mdi_container;

		private TitleButton close_button;
		private TitleButton maximize_button;
		private TitleButton minimize_button;

		private TitleButton [] title_buttons = new TitleButton [3];
		
		// moving windows
		private Point start;
		private State state;
		private FormPos sizing_edge;
		private Rectangle virtual_position;
		private Rectangle prev_virtual_position;
		private Rectangle prev_bounds;
		private bool maximized;

		private class TitleButton {
			public Rectangle Rectangle;
			public ButtonState State;
			public CaptionButton Caption;
			public EventHandler Clicked;
			
			public TitleButton (CaptionButton caption, EventHandler clicked)
			{
				Caption = caption;
				Clicked = clicked;
			}
		}

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

		public MdiChildContext (Form form, MdiClient mdi_container)
		{
			titlebar_color = Color.FromArgb (255, 0, 0, 255);
			this.form = form;
			this.mdi_container = mdi_container;

			/*
			minimize_button = new Button ();
			minimize_button.Bounds = new Rectangle (form.Width - 62,
					-26, 18, 22);
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
			*/

			CreateButtons ();
		}

		public bool Maximized {
			get { return maximized; }
		}

		public MainMenu MergedMenu {
			get {
				if (merged_menu == null)
					merged_menu = CreateMergedMenu ();
				return merged_menu;
			}
		}

		private MainMenu CreateMergedMenu ()
		{
			Form parent = (Form) mdi_container.Parent;
			MainMenu clone = (MainMenu) parent.Menu.CloneMenu ();
			clone.MergeMenu (form.Menu);
			clone.MenuChanged += new EventHandler (MenuChangedHandler);
			clone.SetForm (parent);
			return clone;
		}

		private void MenuChangedHandler (object sender, EventArgs e)
		{
			CreateMergedMenu ();
		}

		public bool HandleMessage (ref Message m)
		{
			switch ((Msg)m.Msg) {


				// The mouse handling messages are actually
				// not WM_NC* messages except for the first button and NCMOVEs
				// down because we capture on the form

			case Msg.WM_MOUSEMOVE:
				return HandleMouseMove (form, ref m);

			case Msg.WM_LBUTTONUP:
				HandleLButtonUp (ref m);
				break;

			case Msg.WM_RBUTTONDOWN:
			case Msg.WM_LBUTTONDOWN:
				return HandleButtonDown (ref m);

			case Msg.WM_NCMOUSEMOVE:
				return HandleNCMouseMove (form, ref m);

			case Msg.WM_NCLBUTTONUP:
				return HandleNCLButtonUp (ref m);

			case Msg.WM_NCLBUTTONDOWN:
				return HandleNCLButtonDown (ref m);

			case Msg.WM_NCPAINT:
//				form.UpdateStyles ();
				PaintWindowDecorations ();
				// Graphics g = XplatUI.GetMenuDC (form.Handle, IntPtr.Zero);
				// g.Clear (Color.Red);
				break;
			}
			return false;
		}

		public void UpdateBorderStyle (FormBorderStyle border_style)
		{
			if (border_style != FormBorderStyle.None)
				XplatUI.SetBorderStyle (form.Handle, (FormBorderStyle) MdiBorderStyle);
			else
				XplatUI.SetBorderStyle (form.Handle, FormBorderStyle.None);

			CreateButtons ();
		}

		private void CreateButtons ()
		{
			switch (form.FormBorderStyle) {
			case FormBorderStyle.None:
				close_button = null;
				minimize_button = null;
				maximize_button = null;
				break;
			case FormBorderStyle.FixedSingle:
			case FormBorderStyle.Fixed3D:
			case FormBorderStyle.FixedDialog:
			case FormBorderStyle.Sizable:
				close_button = new TitleButton (CaptionButton.Close, new EventHandler (CloseClicked));
				minimize_button = new TitleButton (CaptionButton.Minimize, new EventHandler (MinimizeClicked));
				maximize_button = new TitleButton (CaptionButton.Maximize, new EventHandler (MaximizeClicked));
				break;
			case FormBorderStyle.FixedToolWindow:
			case FormBorderStyle.SizableToolWindow:
				close_button = new TitleButton (CaptionButton.Close, new EventHandler (CloseClicked));
				break;
			}

			title_buttons [0] = close_button;
			title_buttons [1] = minimize_button;
			title_buttons [2] = maximize_button;

		}

		private bool HandleButtonDown (ref Message m)
		{
			form.BringToFront ();
			mdi_container.ActiveMdiChild = form;
			return false;
		}

		private bool HandleNCLButtonDown (ref Message m)
		{
			form.BringToFront ();
			mdi_container.ActiveMdiChild = form;

			int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int) m.LParam.ToInt32 ());

			form.PointToClient (ref x, ref y);

			start = new Point (x, y);
			virtual_position = form.Bounds;

			// Need to adjust because we are in NC land
			y += TitleBarHeight;
			FormPos pos = FormPosForCoords (x, y);

			if (pos == FormPos.TitleBar) {
				HandleTitleBarDown (x, y);
				return true;
			}

			/*
			if (IsSizable) {
				SetCursorForPos (pos);
			
				if ((pos & FormPos.AnyEdge) == 0)
					return false;

				state = State.Sizing;
				sizing_edge = pos;
				form.Capture = true;
				return true;
			}
			*/

			return false;
		}

		private void HandleTitleBarDown (int x, int y)
		{
			foreach (TitleButton button in title_buttons) {
				if (button != null && button.Rectangle.Contains (x, y)) {
					button.State = ButtonState.Pushed;
					return;
				}
			}

			if (maximized)
				return;

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

			/*
			if (IsSizable) {
				int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
				int y = Control.HighOrder ((int) m.LParam.ToInt32 ());
				FormPos pos = FormPosForCoords (x, y);
				Console.WriteLine ("position:   " + pos);
				SetCursorForPos (pos);

				ClearVirtualPosition ();
				state = State.Idle;
			}
			*/
			
			return false;
		}

		private bool HandleNCMouseMove (Form form, ref Message m)
		{
			if (IsSizable) {
				int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
				int y = Control.HighOrder ((int) m.LParam.ToInt32 ());
				FormPos pos = FormPosForCoords (x, y);
				Console.WriteLine ("position:   " + pos);
				SetCursorForPos (pos);

				ClearVirtualPosition ();
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

			mdi_container.EnsureScrollBars (virtual_position.Right, virtual_position.Bottom);

			DrawVirtualPosition ();
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

		private bool HasBorders {
			get {
				return form.FormBorderStyle != FormBorderStyle.None;
			}
		}

		private bool IsToolWindow {
			get {
				if (form.FormBorderStyle == FormBorderStyle.SizableToolWindow ||
						form.FormBorderStyle == FormBorderStyle.FixedToolWindow)
					return true;
				return false;
			}
		}

		private int TitleBarHeight {
			get {
				if (IsToolWindow)
					return 19;
				if (form.FormBorderStyle == FormBorderStyle.None)
					return 0;
				return 26;
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

			DrawVirtualPosition ();
		}

		private void HandleLButtonUp (ref Message m)
		{
			if (state == State.Idle)
				return;

			ClearVirtualPosition ();

			form.Capture = false;
			form.Bounds = virtual_position;
			state = State.Idle;
		}

		private bool HandleNCLButtonUp (ref Message m)
		{
			int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int) m.LParam.ToInt32 ());

			form.PointToClient (ref x, ref y);

			// Need to adjust because we are in NC land
			y += TitleBarHeight;

			foreach (TitleButton button in title_buttons) {
				if (button != null && button.Rectangle.Contains (x, y)) {
					button.Clicked (this, EventArgs.Empty);
					return true;
				}
			}

			return true;
		}

		private void PaintWindowDecorations ()
		{
			Graphics dc = XplatUI.GetMenuDC (form.Handle, IntPtr.Zero);

			if (HasBorders) {
				Rectangle borders = new Rectangle (0, 0, form.Width, form.Height);
//			dc.FillRectangle (new SolidBrush (Color.Black), borders);
				
/*			
			dc.DrawRectangle (new Pen (SystemColors.ControlLight, 1), borders);
			borders.Inflate (-2, -2);
			dc.DrawRectangle (new Pen (SystemColors.ControlDark, 1), borders);
			borders.X++;
			borders.Width -= 2;
			dc.DrawRectangle (new Pen (SystemColors.ControlLight, 1), borders);
*/
			
				ControlPaint.DrawBorder3D (dc, borders,	Border3DStyle.Raised);

				if (IsSizable) {
					borders.Inflate (-1, -1);
					ControlPaint.DrawFocusRectangle (dc, borders);
				}
			}

			Color color = ThemeEngine.Current.ColorControlDark;
			if (form == mdi_container.ActiveMdiChild && !maximized)
				color = titlebar_color;

			Rectangle tb = new Rectangle (BorderWidth, BorderWidth,
					form.Width - (BorderWidth * 2), TitleBarHeight - 1);

			dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (color), tb);

			dc.DrawLine (new Pen (Color.White, 1), BorderWidth,
					TitleBarHeight + BorderWidth, form.Width - BorderWidth,
					TitleBarHeight + BorderWidth);

			if (!IsToolWindow) {
				tb.X += 18; // Room for the icon and the buttons
				tb.Width = (form.Width - 62) - tb.X;
			}

			if (form.Text != null) {
				StringFormat format = new StringFormat ();
				format.FormatFlags = StringFormatFlags.NoWrap;
				format.Trimming = StringTrimming.EllipsisCharacter;
				format.LineAlignment = StringAlignment.Center;
				dc.DrawString (form.Text, form.Font,
						ThemeEngine.Current.ResPool.GetSolidBrush (Color.White),
						tb, format);
			}

			if (!IsToolWindow && HasBorders) {
				if (form.Icon != null) {
					dc.DrawIcon (form.Icon, new Rectangle (BorderWidth + 3,
								     BorderWidth + 3, 16, 16));
				}
					
				minimize_button.Rectangle = new Rectangle (form.Width - 62,
						BorderWidth + 2, 18, 22);

				maximize_button.Rectangle = new Rectangle (form.Width - 44,
						BorderWidth + 2, 18, 22);
				
				close_button.Rectangle = new Rectangle (form.Width - 24,
						BorderWidth + 2, 18, 22);

				DrawTitleButton (dc, minimize_button);
				DrawTitleButton (dc, maximize_button);
				DrawTitleButton (dc, close_button);
			} else {
				close_button.Rectangle = new Rectangle (form.Width - BorderWidth - 2 - 13,
						BorderWidth + 2, 13, 13);
				DrawTitleButton (dc, close_button);
			}
		}

		private void DrawTitleButton (Graphics dc, TitleButton button)
		{
			dc.FillRectangle (SystemBrushes.Control, button.Rectangle);

			ControlPaint.DrawCaptionButton (dc, button.Rectangle,
					button.Caption, ButtonState.Normal);
		}

		private void CloseClicked (object sender, EventArgs e)
		{
			form.Close ();
			// form.Close should set visibility to false somewhere
			// in it's closing chain but currently does not.
			form.Visible = false;
		}

		private void MinimizeClicked (object sender, EventArgs e)
		{
			form.SuspendLayout ();
			form.Width = MinTitleBarSize.Width + (BorderWidth * 2);
			form.Height = MinTitleBarSize.Height + (BorderWidth * 2);
			form.ResumeLayout ();
		}

		private void MaximizeClicked (object sender, EventArgs e)
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

		private void DrawVirtualPosition ()
		{
			ClearVirtualPosition ();

			XplatUI.DrawReversibleRectangle (mdi_container.Handle, virtual_position, 2);
			prev_virtual_position = virtual_position;
		}

		private void ClearVirtualPosition ()
		{
			if (prev_virtual_position != Rectangle.Empty)
				XplatUI.DrawReversibleRectangle (mdi_container.Handle,
						prev_virtual_position, 2);
			prev_virtual_position = Rectangle.Empty;
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

