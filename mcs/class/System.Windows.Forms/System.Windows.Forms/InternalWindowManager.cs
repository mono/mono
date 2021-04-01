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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
//


using System;
using System.Drawing;
using System.Runtime.InteropServices;


namespace System.Windows.Forms {

	internal abstract class InternalWindowManager {
		private TitleButtons title_buttons;
		internal Form form;

		// moving windows
		internal Point start;
		internal State state;
		protected Point clicked_point;
		private FormPos sizing_edge;
		internal Rectangle virtual_position;

		private Rectangle normal_bounds;
		private Rectangle iconic_bounds;
		

		public enum State {
			Idle,
			Moving,
			Sizing,
		}

		[Flags]
		public enum FormPos {
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

		public InternalWindowManager (Form form)
		{
			this.form = form;

			form.SizeChanged += new EventHandler (FormSizeChangedHandler);

			title_buttons = new TitleButtons (form);
			ThemeEngine.Current.ManagedWindowSetButtonLocations (this);
		}

		public Form Form {
			get { return form; }
		}
		
		public int IconWidth {
			get { return TitleBarHeight - 5; }
		}

		public TitleButtons TitleButtons {
			get {
				return title_buttons;
			}
		}
		internal Rectangle NormalBounds {
			get {
				return normal_bounds;
			}
			set {
				normal_bounds = value;
			}
		}
		internal Size IconicSize {
			get {
				return SystemInformation.MinimizedWindowSize;
			}
		}
		
		internal Rectangle IconicBounds {
			get {
				if (iconic_bounds == Rectangle.Empty)
					return Rectangle.Empty;
				Rectangle result = iconic_bounds;
				result.Y = Form.Parent.ClientRectangle.Bottom - iconic_bounds.Y;
				return result;
			}
			set {
				iconic_bounds = value;
				iconic_bounds.Y = Form.Parent.ClientRectangle.Bottom - iconic_bounds.Y;
			}
		}

		internal virtual Rectangle MaximizedBounds {
			get {
				return Form.Parent.ClientRectangle;
			}
		}
				
		public virtual void UpdateWindowState (FormWindowState old_window_state, FormWindowState new_window_state, bool force)
		{
			if (old_window_state == FormWindowState.Normal) {
				NormalBounds = form.Bounds;
			} else if (old_window_state == FormWindowState.Minimized) {
				IconicBounds = form.Bounds;
			}

			switch (new_window_state) {
			case FormWindowState.Minimized:
				if (IconicBounds == Rectangle.Empty) {
					Size size = IconicSize;
					Point location = new Point (0, Form.Parent.ClientSize.Height - size.Height);
					IconicBounds = new Rectangle (location, size);
				}
				form.Bounds = IconicBounds;
				break;
			case FormWindowState.Maximized:
				form.Bounds = MaximizedBounds;
				break;
			case FormWindowState.Normal:
				form.Bounds = NormalBounds;
				break;
			}

			UpdateWindowDecorations (new_window_state);
			form.ResetCursor ();
		}
		
		public virtual void UpdateWindowDecorations (FormWindowState window_state)
		{
			ThemeEngine.Current.ManagedWindowSetButtonLocations (this);
			if (form.IsHandleCreated)
				XplatUI.RequestNCRecalc (form.Handle);
		}
		
		public virtual bool WndProc (ref Message m)
		{
#if debug
			Console.WriteLine(DateTime.Now.ToLongTimeString () + " " + this.GetType () .Name + " (Handle={0},Text={1}) received message {2}", form.IsHandleCreated ? form.Handle : IntPtr.Zero,  form.Text, m.ToString ());
#endif

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
				return HandleRButtonDown (ref m);
				
			case Msg.WM_LBUTTONDOWN:
				return HandleLButtonDown (ref m);
				
			case Msg.WM_LBUTTONDBLCLK:
				return HandleLButtonDblClick (ref m);
				
			case Msg.WM_PARENTNOTIFY:
				if (Control.LowOrder(m.WParam.ToInt32()) == (int) Msg.WM_LBUTTONDOWN) 
					Activate ();
				break;

			case Msg.WM_NCHITTEST: 
				return HandleNCHitTest (ref m);

				// Return true from these guys, otherwise win32 will mess up z-order
			case Msg.WM_NCLBUTTONUP:
				HandleNCLButtonUp (ref m);
				return true;

			case Msg.WM_NCLBUTTONDOWN:
				HandleNCLButtonDown (ref m);
				return true;

			case Msg.WM_NCMOUSEMOVE:
				HandleNCMouseMove (ref m);
				return true;
				
			case Msg.WM_NCLBUTTONDBLCLK:
				HandleNCLButtonDblClick (ref m);
				break;

			case Msg.WM_NCMOUSELEAVE:
				HandleNCMouseLeave (ref m);
				break;
			
			case Msg.WM_MOUSELEAVE:
				HandleMouseLeave (ref m);
				break;

			case Msg.WM_NCCALCSIZE:
				return HandleNCCalcSize (ref m);

			case Msg.WM_NCPAINT:
				return HandleNCPaint (ref m);
			}

			return false;
		}

		protected virtual bool HandleNCPaint (ref Message m)
		{
			PaintEventArgs pe = XplatUI.PaintEventStart (ref m, form.Handle, false);

			Rectangle clip;
			
			if (form.ActiveMenu != null) {
				Point pnt;

				pnt = GetMenuOrigin ();

				// The entire menu has to be in the clip rectangle because the 
				// control buttons are right-aligned and otherwise they would
				// stay painted when the window gets resized.
				clip = new Rectangle (pnt.X, pnt.Y, form.ClientSize.Width, 0);
				clip = Rectangle.Union (clip, pe.ClipRectangle);
				pe.SetClip (clip);
				pe.Graphics.SetClip (clip);

				form.ActiveMenu.Draw (pe, new Rectangle (pnt.X, pnt.Y, form.ClientSize.Width, 0));
			}
			if (HasBorders || IsMinimized && !(Form.IsMdiChild && IsMaximized)) {
				// clip region is not correct on win32.
				// use the entire form's area.
				clip = new Rectangle (0, 0, form.Width, form.Height);
				ThemeEngine.Current.DrawManagedWindowDecorations (pe.Graphics, clip, this);
			}
			XplatUI.PaintEventEnd (ref m, form.Handle, false, pe);
			return true;
		}

		protected virtual bool HandleNCCalcSize (ref Message m)
		{
			XplatUIWin32.NCCALCSIZE_PARAMS ncp;
			XplatUIWin32.RECT rect;

			if (m.WParam == (IntPtr)1) {
				ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)Marshal.PtrToStructure (m.LParam,
						typeof (XplatUIWin32.NCCALCSIZE_PARAMS));
				
				ncp.rgrc1 = NCCalcSize (ncp.rgrc1);

				Marshal.StructureToPtr (ncp, m.LParam, true);
			} else {
				rect = (XplatUIWin32.RECT) Marshal.PtrToStructure (m.LParam, typeof (XplatUIWin32.RECT));
				
				rect = NCCalcSize (rect);
				
				Marshal.StructureToPtr (rect, m.LParam, true);
			}
			
			return true;
		}

		protected virtual XplatUIWin32.RECT NCCalcSize (XplatUIWin32.RECT proposed_window_rect)
		{
			int bw = ThemeEngine.Current.ManagedWindowBorderWidth (this);

			if (HasBorders) {
				proposed_window_rect.top += TitleBarHeight + bw;
				proposed_window_rect.bottom -= bw;
				proposed_window_rect.left += bw;
				proposed_window_rect.right -= bw;
			}

			if (XplatUI.RequiresPositiveClientAreaSize) {
				// This is necessary for Linux, can't handle <= 0-sized 
				// client areas correctly.
				if (proposed_window_rect.right <= proposed_window_rect.left) {
					proposed_window_rect.right += proposed_window_rect.left - proposed_window_rect.right + 1;
				}
				if (proposed_window_rect.top >= proposed_window_rect.bottom) {
					proposed_window_rect.bottom += proposed_window_rect.top - proposed_window_rect.bottom + 1;
				}
			}

			return proposed_window_rect;
		}

		protected virtual bool HandleNCHitTest (ref Message m)
		{

			int x = Control.LowOrder ((int)m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int)m.LParam.ToInt32 ());

			NCPointToClient (ref x, ref y);

			FormPos pos = FormPosForCoords (x, y);

			if (pos == FormPos.TitleBar) {
				m.Result = new IntPtr ((int)HitTest.HTCAPTION);
				return true;
			}

			if (!IsSizable)
				return false;

			switch (pos) {
			case FormPos.Top:
				m.Result = new IntPtr ((int)HitTest.HTTOP);
				break;
			case FormPos.Left:
				m.Result = new IntPtr ((int)HitTest.HTLEFT);
				break;
			case FormPos.Right:
				m.Result = new IntPtr ((int)HitTest.HTRIGHT);
				break;
			case FormPos.Bottom:
				m.Result = new IntPtr ((int)HitTest.HTBOTTOM);
				break;
			case FormPos.TopLeft:
				m.Result = new IntPtr ((int)HitTest.HTTOPLEFT);
				break;
			case FormPos.TopRight:
				m.Result = new IntPtr ((int)HitTest.HTTOPRIGHT);
				break;
			case FormPos.BottomLeft:
				m.Result = new IntPtr ((int)HitTest.HTBOTTOMLEFT);
				break;
			case FormPos.BottomRight:
				m.Result = new IntPtr ((int)HitTest.HTBOTTOMRIGHT);
				break;
			default:
				// We return false so that DefWndProc handles things
				return false;
			}
			return true;
		}

		public virtual void UpdateBorderStyle (FormBorderStyle border_style)
		{
			if (form.IsHandleCreated) {
				XplatUI.SetBorderStyle (form.Handle, border_style);
			}

			if (ShouldRemoveWindowManager (border_style)) {
				form.RemoveWindowManager ();
				return;
			}
				
			ThemeEngine.Current.ManagedWindowSetButtonLocations (this);
		}

		
		
		public virtual void SetWindowState (FormWindowState old_state, FormWindowState window_state)
		{
			UpdateWindowState (old_state, window_state, false);
		}

		public virtual FormWindowState GetWindowState ()
		{
			return form.window_state;
		}

		public virtual void PointToClient (ref int x, ref int y)
		{
			// toolwindows stay in screencoords we just have to make sure
			// they obey the working area
			Rectangle working = SystemInformation.WorkingArea;

			if (x > working.Right)
				x = working.Right;
			if (x < working.Left)
				x = working.Left;

			if (y < working.Top)
				y = working.Top;
			if (y > working.Bottom)
				y = working.Bottom;
		}

		public virtual void PointToScreen (ref int x, ref int y)
		{
			XplatUI.ClientToScreen (form.Handle, ref x, ref y);
		}

		protected virtual bool ShouldRemoveWindowManager (FormBorderStyle style)
		{
			return style != FormBorderStyle.FixedToolWindow && style != FormBorderStyle.SizableToolWindow;
		}

		public bool IconRectangleContains (int x, int y)
		{
			if (!ShowIcon)
				return false;

			Rectangle icon = ThemeEngine.Current.ManagedWindowGetTitleBarIconArea (this);
			return icon.Contains (x, y);
		}

		public bool ShowIcon {
			get {
				if (!Form.ShowIcon)
					return false;
				if (!Form.ControlBox)
					return false;
				if (!HasBorders)
					return false;
				if (IsMinimized)
					return true;
				if (IsToolWindow || Form.FormBorderStyle == FormBorderStyle.FixedDialog)
					return false;
				return true;
			}
		}

		protected virtual void Activate ()
		{
			form.Invalidate (true);
			form.Update ();
		}

		public virtual bool IsActive {
			get {
				return true;
			}
		}


		private void FormSizeChangedHandler (object sender, EventArgs e)
		{
			if (form.IsHandleCreated) {
				ThemeEngine.Current.ManagedWindowSetButtonLocations (this);
				XplatUI.InvalidateNC (form.Handle);
			}
		}

		protected virtual bool HandleRButtonDown (ref Message m)
		{
			Activate ();
			return false;
		}
		
		protected virtual bool HandleLButtonDown (ref Message m)
		{
			Activate ();
			return false;
		}

		protected virtual bool HandleLButtonDblClick(ref Message m)
		{
			return false;
		}
		
		protected virtual bool HandleNCMouseLeave (ref Message m)
		{
			int x = Control.LowOrder ((int)m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int)m.LParam.ToInt32 ());

			NCPointToClient (ref x, ref y);
			FormPos pos = FormPosForCoords (x, y);

			if (pos != FormPos.TitleBar) {
				HandleTitleBarLeave (x, y);
				return true;
			}

			return true;
		}
		
		protected virtual bool HandleNCMouseMove (ref Message m)
		{
			int x = Control.LowOrder((int)m.LParam.ToInt32( ));
			int y = Control.HighOrder((int)m.LParam.ToInt32( ));

			NCPointToClient (ref x, ref y);
			FormPos pos = FormPosForCoords (x, y);

			if (pos == FormPos.TitleBar) {
				HandleTitleBarMouseMove (x, y);
				return true;
			}

			if (form.ActiveMenu != null && XplatUI.IsEnabled (form.Handle)) {
				MouseEventArgs mea = new MouseEventArgs (Form.FromParamToMouseButtons (m.WParam.ToInt32 ()), form.mouse_clicks, x, y, 0);
				form.ActiveMenu.OnMouseMove (form, mea);
			}

			return true;
			
		}
		
		protected virtual bool HandleNCLButtonDown (ref Message m)
		{
			Activate ();

			start = Cursor.Position;
			virtual_position = form.Bounds;
			
			int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int) m.LParam.ToInt32 ());
			
			// Need to adjust because we are in NC land
			NCPointToClient (ref x, ref y);
			FormPos pos = FormPosForCoords (x, y);
			
			if (form.ActiveMenu != null && XplatUI.IsEnabled (form.Handle)) {
				MouseEventArgs mea = new MouseEventArgs (Form.FromParamToMouseButtons (m.WParam.ToInt32 ()), form.mouse_clicks, x, y - TitleBarHeight, 0);
				form.ActiveMenu.OnMouseDown (form, mea);
			}
			
			if (pos == FormPos.TitleBar) {
				HandleTitleBarDown (x, y);
				return true;
			}

			if (IsSizable) {
				if ((pos & FormPos.AnyEdge) == 0)
					return false;

				virtual_position = form.Bounds;
				state = State.Sizing;
				sizing_edge = pos;
				form.Capture = true;
				return true;
			}

			return false;
		}

		protected virtual void HandleNCLButtonDblClick (ref Message m)
		{
			int x = Control.LowOrder ((int)m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int)m.LParam.ToInt32 ());

			// Need to adjust because we are in NC land
			NCPointToClient (ref x, ref y);

			FormPos pos = FormPosForCoords (x, y);
			if (pos == FormPos.TitleBar || pos == FormPos.Top)
				HandleTitleBarDoubleClick (x, y);

		}
		
		protected virtual void HandleTitleBarDoubleClick (int x, int y)
		{
		
		}
		
		protected virtual void HandleTitleBarLeave (int x, int y)
		{
			title_buttons.MouseLeave (x, y);
		}
		
		protected virtual void HandleTitleBarMouseMove (int x, int y)
		{
			if (title_buttons.MouseMove (x, y))
				XplatUI.InvalidateNC (form.Handle);
		}
		
		protected virtual void HandleTitleBarUp (int x, int y)
		{
			title_buttons.MouseUp (x, y);

			return;
		}
		
		protected virtual void HandleTitleBarDown (int x, int y)
		{
			title_buttons.MouseDown (x, y);

			if (!TitleButtons.AnyPushedTitleButtons && !IsMaximized) {
				state = State.Moving;
				clicked_point = new Point (x, y);
				if (form.Parent != null) {
					form.CaptureWithConfine (form.Parent);
				} else {
					form.Capture = true;
				}
			}
			
			XplatUI.InvalidateNC (form.Handle);
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
			
			return false;
		}

		private void HandleMouseLeave (ref Message m)
		{
			form.ResetCursor ();
		}
	
		protected virtual void HandleWindowMove (Message m)
		{
			Point move = MouseMove (Cursor.Position);

			UpdateVP (virtual_position.X + move.X, virtual_position.Y + move.Y,
					virtual_position.Width, virtual_position.Height);
		}

		private void HandleSizing (Message m)
		{
			Rectangle pos = virtual_position;
			int mw;
			int mh;
			if (IsToolWindow) {
				int border_width = BorderWidth;
				mw = 2 * (border_width + Theme.ManagedWindowSpacingAfterLastTitleButton) + ThemeEngine.Current.ManagedWindowButtonSize (this).Width;
				mh = 2 * border_width + TitleBarHeight;
			} else {
				Size minimum_size = SystemInformation.MinWindowTrackSize;
				mw = minimum_size.Width;
				mh = minimum_size.Height;
			}
			int x = Cursor.Position.X;
			int y = Cursor.Position.Y;

			PointToClient (ref x, ref y);

			if ((sizing_edge & FormPos.Top) != 0) {
				if (pos.Bottom - y < mh)
					y = pos.Bottom - mh;
				pos.Height = pos.Bottom - y;
				pos.Y = y;
			} else if ((sizing_edge & FormPos.Bottom) != 0) {
				int height = y - pos.Top;
				if (height <= mh)
					height = mh;
				pos.Height = height;
			}

			if ((sizing_edge & FormPos.Left) != 0) {
				if (pos.Right - x < mw)
					x = pos.Right - mw;
				pos.Width = pos.Right - x;
				pos.X = x;
			} else if ((sizing_edge & FormPos.Right) != 0) {
				int width = x - form.Left;
				if (width <= mw)
					width = mw;
				pos.Width = width;
			}

			UpdateVP (pos);
		}

		public bool IsMaximized {
			get { return GetWindowState () == FormWindowState.Maximized; }
		}

		public bool IsMinimized {
			get { return GetWindowState () == FormWindowState.Minimized; }
		}

		public bool IsSizable {
			get {
				switch (form.FormBorderStyle) {
				case FormBorderStyle.Sizable:
				case FormBorderStyle.SizableToolWindow:
					return (form.window_state != FormWindowState.Minimized);
				default:
					return false;
				}
			}
		}

		public bool HasBorders {
			get {
				return form.FormBorderStyle != FormBorderStyle.None;
			}
		}

		public bool IsToolWindow {
			get {
				if (form.FormBorderStyle == FormBorderStyle.SizableToolWindow ||
				    form.FormBorderStyle == FormBorderStyle.FixedToolWindow || 
				    form.GetCreateParams().IsSet (WindowExStyles.WS_EX_TOOLWINDOW))
					return true;
				return false;
			}
		}

		public int TitleBarHeight {
			get {
				return ThemeEngine.Current.ManagedWindowTitleBarHeight (this);
			}
		}

		public int BorderWidth {
			get {
				return ThemeEngine.Current.ManagedWindowBorderWidth (this);
			}
		}
		
		public virtual int MenuHeight {
			get {
				return (form.Menu != null ? ThemeEngine.Current.MenuHeight : 0);
			}
		}

		protected void UpdateVP (Rectangle r)
		{
			UpdateVP (r.X, r.Y, r.Width, r.Height);
		}

		protected void UpdateVP (Point loc, int w, int h)
		{
			UpdateVP (loc.X, loc.Y, w, h);
		}

		protected void UpdateVP (int x, int y, int w, int h)
		{
			virtual_position.X = x;
			virtual_position.Y = y;
			virtual_position.Width = w;
			virtual_position.Height = h;

			DrawVirtualPosition (virtual_position);
		}

		protected virtual void HandleLButtonUp (ref Message m)
		{
			if (state == State.Idle)
				return;

			ClearVirtualPosition ();

			form.Capture = false;
			if (state == State.Moving && form.Location != virtual_position.Location) 
				form.Location = virtual_position.Location;
			else if (state == State.Sizing && form.Bounds != virtual_position)
				form.Bounds = virtual_position;
			state = State.Idle;

			OnWindowFinishedMoving ();
		}

		private bool HandleNCLButtonUp (ref Message m)
		{
			if (form.Capture) {
				ClearVirtualPosition ();

				form.Capture = false;
				state = State.Idle;
				if (form.MdiContainer != null)
					form.MdiContainer.SizeScrollBars();
			}
				
			int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int) m.LParam.ToInt32 ());

			NCPointToClient (ref x, ref y);
			FormPos pos = FormPosForCoords (x, y);

			if (pos == FormPos.TitleBar) {
				HandleTitleBarUp (x, y);
				return true;
			}
			
			return true;
		}
		
		protected void DrawTitleButton (Graphics dc, TitleButton button, Rectangle clip)
		{
			if (!button.Rectangle.IntersectsWith (clip))
				return;

			ThemeEngine.Current.ManagedWindowDrawMenuButton (dc, button, clip, this);
		}

		public virtual void DrawMaximizedButtons (object sender, PaintEventArgs pe)
		{
		}

		protected Point MouseMove (Point pos)
		{
			return new Point (pos.X - start.X, pos.Y - start.Y);
		}

		protected virtual void DrawVirtualPosition (Rectangle virtual_position)
		{
			form.Bounds = virtual_position;
			start = Cursor.Position;
		}

		protected virtual void ClearVirtualPosition ()
		{
			
		}

		protected virtual void OnWindowFinishedMoving ()
		{
		}

		protected virtual void NCPointToClient(ref int x, ref int y) {
			form.PointToClient(ref x, ref y);
			NCClientToNC (ref x, ref y);
		}

		protected virtual void NCClientToNC (ref int x, ref int y) {
			y += TitleBarHeight;
			y += BorderWidth;
			y += MenuHeight;
		}
		
		internal Point GetMenuOrigin ()
		{
			return new Point (BorderWidth, BorderWidth + TitleBarHeight);
		}
		
		protected FormPos FormPosForCoords (int x, int y)
		{
			int bw = BorderWidth;
			if (y < TitleBarHeight + bw) {
				//	Console.WriteLine ("A");
				if (y > bw && x > bw &&
						x < form.Width - bw)
					return FormPos.TitleBar;

				if (x < bw || (x < 20 && y < bw))
					return FormPos.TopLeft;

				if (x > form.Width - bw ||
					(x > form.Width - 20 && y < bw))
					return FormPos.TopRight;

				if (y < bw)
					return FormPos.Top;

			} else if (y > form.Height - 20) {
				//	Console.WriteLine ("B");
				if (x < bw ||
						(x < 20 && y > form.Height - bw))
					return FormPos.BottomLeft;

				if (x > form.Width - (bw * 2) ||
						(x > form.Width - 20 &&
						 y > form.Height - bw))
					return FormPos.BottomRight;

				if (y > form.Height - (bw * 2))
					return FormPos.Bottom;


			} else if (x < bw) {
				//	Console.WriteLine ("C");
				return FormPos.Left;
			} else if (x > form.Width - (bw * 2)) {
//				Console.WriteLine ("D");
				return FormPos.Right;
			} else {
				//			Console.WriteLine ("E   {0}", form.Width - bw);
			}
			
			return FormPos.None;
		}
	}
	internal class TitleButton
	{
		public Rectangle Rectangle;
		public ButtonState State;
		public CaptionButton Caption;
		private EventHandler Clicked;
		public bool Visible;
		bool entered;

		public TitleButton (CaptionButton caption, EventHandler clicked)
		{
			Caption = caption;
			Clicked = clicked;
		}
		
		public void OnClick ()
		{
			if (Clicked != null) {
				Clicked (this, EventArgs.Empty);
			}
		}

		public bool Entered {
			get { return entered; }
			set { entered = value; }
		}
	}

	internal class TitleButtons : System.Collections.IEnumerable
	{
		public TitleButton MinimizeButton;
		public TitleButton MaximizeButton;
		public TitleButton RestoreButton;
		public TitleButton CloseButton;
		public TitleButton HelpButton;

		public TitleButton [] AllButtons;
		public bool Visible;

		private ToolTip.ToolTipWindow tooltip;
		private Timer tooltip_timer;
		private TitleButton tooltip_hovered_button;
		private TitleButton tooltip_hidden_button;
		private const int tooltip_hide_interval = 3000;
		private const int tooltip_show_interval = 1000;
		private Form form;
		
		public TitleButtons (Form frm)
		{
			this.form = frm;
			this.Visible = true;
			
			MinimizeButton = new TitleButton (CaptionButton.Minimize, new EventHandler (ClickHandler));
			MaximizeButton = new TitleButton (CaptionButton.Maximize, new EventHandler (ClickHandler));
			RestoreButton = new TitleButton (CaptionButton.Restore, new EventHandler (ClickHandler));
			CloseButton = new TitleButton (CaptionButton.Close, new EventHandler (ClickHandler));
			HelpButton = new TitleButton (CaptionButton.Help, new EventHandler (ClickHandler));

			AllButtons = new TitleButton [] { MinimizeButton, MaximizeButton, RestoreButton, CloseButton, HelpButton };
		}
		
		private void ClickHandler (object sender, EventArgs e)
		{
			if (!Visible) {
				return;
			}
			
			TitleButton button = (TitleButton) sender;
			
			switch (button.Caption) {
				case CaptionButton.Close: 
					form.Close ();
					break;
				case CaptionButton.Help:
					Console.WriteLine ("Help not implemented.");
					break;
				case CaptionButton.Maximize:
					form.WindowState = FormWindowState.Maximized;
					break;
				case CaptionButton.Minimize:
					form.WindowState = FormWindowState.Minimized;
					break;
				case CaptionButton.Restore:
					form.WindowState = FormWindowState.Normal;
					break;
			}
		}
		
		public TitleButton FindButton (int x, int y)
		{
			if (!Visible) {
				return null;
			}
			
			foreach (TitleButton button in AllButtons) {
				if (button.Visible && button.Rectangle.Contains (x, y)) {
					return button;
				}
			}
			return null;
		}
		
		public bool AnyPushedTitleButtons {
			get {
				if (!Visible) {
					return false;
				}
				
				foreach (TitleButton button in AllButtons) {
					if (button.Visible && button.State == ButtonState.Pushed) {
						return true;
					}
				}
				return false;
			}
		}

		#region IEnumerable Members

		public System.Collections.IEnumerator GetEnumerator ()
		{
			return AllButtons.GetEnumerator ();
		}
		#endregion

		#region ToolTip helpers
		// Called from MouseMove if mouse is over a button
		public void ToolTipStart (TitleButton button)
		{
			tooltip_hovered_button = button;

			if (tooltip_hovered_button == tooltip_hidden_button)
				return;
			tooltip_hidden_button = null;

			if (tooltip != null && tooltip.Visible)
				ToolTipShow (true);

			if (tooltip_timer == null) {

				tooltip_timer = new Timer ();
				tooltip_timer.Tick += new EventHandler (ToolTipTimerTick);
			}

			tooltip_timer.Interval = tooltip_show_interval;
			tooltip_timer.Start ();
			tooltip_hovered_button = button;
		}

		public void ToolTipTimerTick (object sender, EventArgs e)
		{
			if (tooltip_timer.Interval == tooltip_hide_interval) {
				tooltip_hidden_button = tooltip_hovered_button;
				ToolTipHide (false);
			} else {
				ToolTipShow (false);
			}
		}
		// Called from timer (with only_refresh = false)
		// Called from ToolTipStart if tooltip is already shown (with only_refresh = true)
		public void ToolTipShow (bool only_refresh)
		{
			if (!form.Visible)
				return;

			string text = Locale.GetText (tooltip_hovered_button.Caption.ToString ());

			tooltip_timer.Interval = tooltip_hide_interval;
			tooltip_timer.Enabled = true;

			if (only_refresh && (tooltip == null || !tooltip.Visible)) {
				return;
			}

			if (tooltip == null)
				tooltip = new ToolTip.ToolTipWindow ();
			else if (tooltip.Text == text && tooltip.Visible)
				return;
			else if (tooltip.Visible)
				tooltip.Visible = false;

			if (form.WindowState == FormWindowState.Maximized && form.MdiParent != null)
				tooltip.Present (form.MdiParent, text);
			else
				tooltip.Present (form, text);
			
		}
		
		// Called from MouseLeave (with reset_hidden_button = true)
		// Called from MouseDown  (with reset_hidden_button = false)
		// Called from MouseMove if mouse isn't over any button (with reset_hidden_button = false)
		// Called from Timer if hiding (with reset_hidden_button = false)
		public void ToolTipHide (bool reset_hidden_button)
		{
			if (tooltip_timer != null)
				tooltip_timer.Enabled = false;
			if (tooltip != null && tooltip.Visible)
				tooltip.Visible = false;
			if (reset_hidden_button)
				tooltip_hidden_button = null;
		}
		#endregion
		
		public bool MouseMove (int x, int y)
		{
			if (!Visible) {
				return false;
			}

			bool any_change = false;
			bool any_pushed_buttons = AnyPushedTitleButtons;
			bool any_tooltip = false;
			TitleButton over_button = FindButton (x, y);

			foreach (TitleButton button in this) {
				if (button == null)
					continue;
				
				if (button.State == ButtonState.Inactive)
					continue;
					
				if (button == over_button) {
					if (any_pushed_buttons) {
						any_change |= button.State != ButtonState.Pushed;
						button.State = ButtonState.Pushed;
					}
					ToolTipStart (button);
					any_tooltip = true;
					if (!button.Entered) {
						button.Entered = true;
						if (ThemeEngine.Current.ManagedWindowTitleButtonHasHotElementStyle (button, form))
							any_change = true;
					}
				} else {
					if (any_pushed_buttons) {
						any_change |= button.State != ButtonState.Normal;
						button.State = ButtonState.Normal;
					}
					if (button.Entered) {
						button.Entered = false;
						if (ThemeEngine.Current.ManagedWindowTitleButtonHasHotElementStyle (button, form))
							any_change = true;
					}
				}
			}

			if (!any_tooltip)
				ToolTipHide (false);

			return any_change;
		}

		public void MouseDown (int x, int y)
		{
			if (!Visible) {
				return;
			}

			ToolTipHide (false);

			foreach (TitleButton button in this) {
				if (button != null && button.State != ButtonState.Inactive) {
					button.State = ButtonState.Normal;
				}
			}
			TitleButton clicked_button = FindButton (x, y);
			if (clicked_button != null && clicked_button.State != ButtonState.Inactive) {
				clicked_button.State = ButtonState.Pushed;
			}
		}

		public void MouseUp (int x, int y)
		{
			if (!Visible) {
				return;
			}
			
			TitleButton clicked_button = FindButton (x, y);
			if (clicked_button != null && clicked_button.State != ButtonState.Inactive) {
				clicked_button.OnClick ();
			}

			foreach (TitleButton button in this) {
				if (button == null || button.State == ButtonState.Inactive)
					continue;

				button.State = ButtonState.Normal;
			}

			if (clicked_button == CloseButton && !form.closing)
				XplatUI.InvalidateNC (form.Handle);
				
			ToolTipHide (true);
		}

		internal void MouseLeave (int x, int y)
		{
			if (!Visible) {
				return;
			}
			
			foreach (TitleButton button in this) {
				if (button == null || button.State == ButtonState.Inactive)
					continue;

				button.State = ButtonState.Normal;
			}
			
			ToolTipHide (true);
		}
	}
}


