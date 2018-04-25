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
// Copyright (c) 2005-2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// NOT COMPLETE

using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;

// NOTE: Possible optimization:
// Several properties calculate dimensions on the fly; instead; they can 
// be stored in a field and only be recalculated when a style is changed (DefaultClientRect, for example)

namespace System.Windows.Forms {
	internal class Hwnd : IDisposable {
		#region Local Variables
		private static Hashtable	windows	= new Hashtable(100, 0.5f);
		//private const int	menu_height = 14;			// FIXME - Read this value from somewhere
		
		private IntPtr		handle;
		internal IntPtr		client_window;
		internal IntPtr		whole_window;
		internal IntPtr		cursor;
		internal Menu		menu;
		internal TitleStyle	title_style;
		internal FormBorderStyle	border_style;
		internal bool		border_static;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal bool		allow_drop;
		internal Hwnd		parent;
		internal Hwnd		owner;
		internal bool		visible;
		internal bool		mapped;
		internal uint		opacity;
		internal bool		enabled;
		internal bool		zero_sized;
		internal ArrayList	invalid_list;
		internal Rectangle	nc_invalid;
		internal bool		expose_pending;
		internal bool		nc_expose_pending;
		internal bool		configure_pending;
		internal bool		resizing_or_moving; // Used by the X11 backend to track form resize/move
		internal bool		reparented;
		internal object		user_data;
		internal Rectangle	client_rectangle;
		internal int		caption_height;
		internal int		tool_caption_height;
		internal bool		whacky_wm;
		internal bool		fixed_size;
		internal bool		zombie; /* X11 only flag.  true if the X windows have been destroyed but we haven't been Disposed */
		internal bool		topmost; /* X11 only. */
		internal Region		user_clip;
		internal XEventQueue	queue;
		internal WindowExStyles	initial_ex_style;
		internal WindowStyles	initial_style;
		internal FormWindowState cached_window_state = (FormWindowState)(-1);  /* X11 only field */
		internal Point		previous_child_startup_location = new Point (int.MinValue, int.MinValue);
		static internal Point	previous_main_startup_location = new Point (int.MinValue, int.MinValue);
		internal ArrayList children;

		[ThreadStatic]
		private static Bitmap bmp;
		[ThreadStatic]
		private static Graphics bmp_g;
		#endregion	// Local Variables

		// locks for some operations (used in XplatUIX11.cs)
		internal object configure_lock = new object ();
		internal object expose_lock = new object ();

		#region Constructors and destructors
		public Hwnd() {
			x = 0;
			y = 0;
			width = 0;
			height = 0;
			visible = false;
			menu = null;
			border_style = FormBorderStyle.None;
			client_window = IntPtr.Zero;
			whole_window = IntPtr.Zero;
			cursor = IntPtr.Zero;
			handle = IntPtr.Zero;
			parent = null;
			invalid_list = new ArrayList();
			expose_pending = false;
			nc_expose_pending = false;
			enabled = true;
			reparented = false;
			client_rectangle = Rectangle.Empty;
			opacity = 0xffffffff;
			fixed_size = false;
			children = new ArrayList ();
			resizing_or_moving = false;
			whacky_wm = false;
			topmost = false;
		}

		public void Dispose() {
			expose_pending = false;
			nc_expose_pending = false;
			Parent = null;
			lock (windows) {
				windows.Remove(client_window);
				windows.Remove(whole_window);
			}
			client_window = IntPtr.Zero;
			whole_window = IntPtr.Zero;
			zombie = true;
		}
		#endregion

		#region	Static Methods
		public static Hwnd ObjectFromWindow(IntPtr window) {
			Hwnd rv;
			lock (windows) {
				rv = (Hwnd)windows[window];
			}
			return rv;
		}

		public static Hwnd ObjectFromHandle(IntPtr handle) {
			//return (Hwnd)(((GCHandle)handle).Target);
			Hwnd rv;
			lock (windows) {
				rv = (Hwnd)windows[handle];
			}
			return rv;
		}

		public static IntPtr HandleFromObject(Hwnd obj) {
			return obj.handle;
		}

		public static Hwnd GetObjectFromWindow(IntPtr window) {
			Hwnd rv;
			lock (windows) {
				rv = (Hwnd)windows[window];
			}
			return rv;
		}

		public static IntPtr GetHandleFromWindow(IntPtr window) {
			Hwnd	hwnd;

			lock (windows) {
				hwnd = (Hwnd)windows[window];
			}
			if (hwnd != null) {
				return hwnd.handle;
			} else {
				return IntPtr.Zero;
			}
		}

		public static Borders GetBorderWidth (CreateParams cp)
		{
			Borders border_size = new Borders ();

			Size windowborder = ThemeEngine.Current.BorderSize; /*new Size (1, 1);*/ // This is the only one that can be changed from the display properties in windows.
			Size border = ThemeEngine.Current.BorderStaticSize; /*new Size (1, 1);*/
			Size clientedge = ThemeEngine.Current.Border3DSize; /*new Size (2, 2);*/
			Size thickframe = new Size (2 + windowborder.Width, 2 + windowborder.Height);
			Size dialogframe = ThemeEngine.Current.BorderSizableSize; /* new Size (3, 3);*/
			
			if (cp.IsSet (WindowStyles.WS_CAPTION)) {
				border_size.Inflate (dialogframe);
			} else if (cp.IsSet (WindowStyles.WS_BORDER)) {
				if (cp.IsSet (WindowExStyles.WS_EX_DLGMODALFRAME)) {
					if (cp.IsSet (WindowStyles.WS_THICKFRAME) && (cp.IsSet (WindowExStyles.WS_EX_STATICEDGE) || cp.IsSet (WindowExStyles.WS_EX_CLIENTEDGE))) {
						border_size.Inflate (border);
					}
				} else {
					border_size.Inflate (border);
				}
			} else if (cp.IsSet (WindowStyles.WS_DLGFRAME)) {
				border_size.Inflate (dialogframe);
			}

			if (cp.IsSet (WindowStyles.WS_THICKFRAME)) {
				if (cp.IsSet (WindowStyles.WS_DLGFRAME)) {
					border_size.Inflate (border);
				} else {
					border_size.Inflate (thickframe);
				}
			}

			bool only_small_border;
			Size small_border = Size.Empty;

			only_small_border = cp.IsSet (WindowStyles.WS_THICKFRAME) || cp.IsSet (WindowStyles.WS_DLGFRAME);
			if (only_small_border && cp.IsSet (WindowStyles.WS_THICKFRAME) && !cp.IsSet (WindowStyles.WS_BORDER) && !cp.IsSet (WindowStyles.WS_DLGFRAME)) {
				small_border = border;
			}

			if (cp.IsSet (WindowExStyles.WS_EX_CLIENTEDGE | WindowExStyles.WS_EX_DLGMODALFRAME)) {
				border_size.Inflate (clientedge + (only_small_border ? small_border : dialogframe));
			} else if (cp.IsSet (WindowExStyles.WS_EX_STATICEDGE | WindowExStyles.WS_EX_DLGMODALFRAME)) {
				border_size.Inflate (only_small_border ? small_border : dialogframe);
			} else if (cp.IsSet (WindowExStyles.WS_EX_STATICEDGE | WindowExStyles.WS_EX_CLIENTEDGE)) {
				border_size.Inflate (border + (only_small_border ? Size.Empty : clientedge));
			} else {
				if (cp.IsSet (WindowExStyles.WS_EX_CLIENTEDGE)) {
					border_size.Inflate (clientedge);
				}
				if (cp.IsSet (WindowExStyles.WS_EX_DLGMODALFRAME) && !cp.IsSet (WindowStyles.WS_DLGFRAME)) {
					border_size.Inflate (cp.IsSet (WindowStyles.WS_THICKFRAME) ? border : dialogframe);
				}
				if (cp.IsSet (WindowExStyles.WS_EX_STATICEDGE)) {
					if (cp.IsSet (WindowStyles.WS_THICKFRAME) || cp.IsSet (WindowStyles.WS_DLGFRAME)) {
						border_size.Inflate (new Size (-border.Width, -border.Height));
					} else {
						border_size.Inflate (border);
					}
				}
			}
			
			return border_size;
		}

		public static Rectangle	GetWindowRectangle (CreateParams cp, Menu menu)
		{
			return GetWindowRectangle (cp, menu, Rectangle.Empty);
		}

		public static Rectangle GetWindowRectangle (CreateParams cp, Menu menu, Rectangle client_rect)
		{
			Rectangle rect;
			Borders borders;

			borders = GetBorders (cp, menu);

			rect = new Rectangle (Point.Empty, client_rect.Size);
			rect.Y -= borders.top;
			rect.Height += borders.top + borders.bottom;
			rect.X -= borders.left;
			rect.Width += borders.left + borders.right;

#if debug
			Console.WriteLine ("GetWindowRectangle ({0}, {1}, {2}): {3}", cp, menu != null, client_rect, rect);
#endif
			return rect;
		}
		
		public Rectangle GetClientRectangle (int width, int height)
		{
			CreateParams cp = new CreateParams ();
			cp.WindowStyle = initial_style;
			cp.WindowExStyle = initial_ex_style;
			return GetClientRectangle (cp, menu, width, height);
		}

		// This could be greatly optimized by caching the outputs and only updating when something is moved
		// in the parent planar space.  To do that we need to track z-order in the parent space as well
		public ArrayList GetClippingRectangles ()
		{
			ArrayList masks = new ArrayList ();

			if (x < 0) {
				masks.Add (new Rectangle (0, 0, x*-1, Height));
				if (y < 0) {
					masks.Add (new Rectangle (x*-1, 0, Width, y*-1));
				}
			} else if (y < 0) {
				masks.Add (new Rectangle (0, 0, Width, y*-1));
			}

			foreach (Hwnd child in children) {
				if (child.visible)
					masks.Add (new Rectangle (child.X, child.Y, child.Width, child.Height));
			}

			if (parent == null) {
				return masks;
			}

			ArrayList siblings = parent.children;

			foreach (Hwnd sibling in siblings) {
				IntPtr sibling_handle = whole_window;

				if (sibling == this)
					continue;
				
				// This entire method should be cached to find all higher views at the time of query
				do {
					sibling_handle = XplatUI.GetPreviousWindow (sibling_handle);

					if (sibling_handle == sibling.WholeWindow && sibling.visible) {

						Rectangle intersect = Rectangle.Intersect (new Rectangle (X, Y, Width, Height), new Rectangle (sibling.X, sibling.Y, sibling.Width, sibling.Height));
				
						if (intersect == Rectangle.Empty)
							continue;
					
						intersect.X -= X;
						intersect.Y -= Y;

						masks.Add (intersect);
					}
				} while (sibling_handle != IntPtr.Zero);
			}

			return masks;
		}

		public static Borders GetBorders (CreateParams cp, Menu menu)
		{

			Borders borders = new Borders ();

			if (menu != null) {
				int menu_height = menu.Rect.Height;
				if (menu_height == 0)
					menu_height = ThemeEngine.Current.CalcMenuBarSize (GraphicsContext, menu, cp.Width);
				borders.top += menu_height;
			}
			
			if (cp.IsSet (WindowStyles.WS_CAPTION)) {
				int caption_height;
				if (cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW)) {
					caption_height = ThemeEngine.Current.ToolWindowCaptionHeight;
				} else {
					caption_height = ThemeEngine.Current.CaptionHeight;
				}
				borders.top += caption_height;
			}

			Borders border_width = GetBorderWidth (cp);

			borders.left += border_width.left;
			borders.right += border_width.right;
			borders.top += border_width.top;
			borders.bottom += border_width.bottom;
			
			return borders;
		}

		public static Rectangle GetClientRectangle(CreateParams cp, Menu menu, int width, int height) {
			Rectangle rect;
			Borders borders;

			borders = GetBorders (cp, menu); 
			
			rect = new Rectangle(0, 0, width, height);
			rect.Y += borders.top;
			rect.Height -= borders.top + borders.bottom;
			rect.X += borders.left;
			rect.Width -= borders.left + borders.right;
			
#if debug
			Console.WriteLine ("GetClientRectangle ({0}, {1}, {2}, {3}): {4}", cp, menu != null, width, height, rect);
#endif
			
			return rect;
		}
		
		public static Graphics GraphicsContext {
			get {
				if (bmp_g == null) {
					bmp = new Bitmap (1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					bmp_g = Graphics.FromImage (bmp);
				}
			
				return bmp_g;
			}
		}
		#endregion	// Static Methods

		#region Instance Properties
		public FormBorderStyle BorderStyle {
			get {
				return border_style;
			}

			set {
				border_style = value;
			}
		}

		public Rectangle ClientRect {
			get {
				if (client_rectangle == Rectangle.Empty) {
					return DefaultClientRect;
				}
				return client_rectangle;
			}

			set {
				client_rectangle = value;
			}
		}

		public IntPtr Cursor {
			get {
				return cursor;
			}

			set {
				cursor = value;
			}
		}

		public IntPtr ClientWindow {
			get {
				return client_window;
			}

			set {
				client_window = value;
				handle = value;

				zombie = false;

				if (client_window != IntPtr.Zero) {
					lock (windows) {
						if (windows[client_window] == null) {
							windows[client_window] = this;
						}
					}
				}
			}
		}

		public Region UserClip {
			get {
				return user_clip;
			}

			set {
				user_clip = value;
			}
		}

		public Rectangle DefaultClientRect {
			get {
				// We pass a Zero for the menu handle so the menu size is
				// not computed this is done via an WM_NCCALC
				CreateParams cp = new CreateParams ();
				Rectangle rect;
				
				cp.WindowStyle = initial_style;
				cp.WindowExStyle = initial_ex_style;

				rect = GetClientRectangle (cp, null, width, height);

				return rect;
			}
		}

		public bool ExposePending {
			get {
				return expose_pending;
			}
		}

		public IntPtr Handle {
			get {
				if (handle == IntPtr.Zero) {
					throw new ArgumentNullException("Handle", "Handle is not yet assigned, need a ClientWindow");
				}
				return handle;
			}
		}

		public int Height {
			get {
				return height;
			}

			set {
				height = value;
			}
		}

		public Menu Menu {
			get {
				return menu;
			}

			set {
				menu = value;
			}
		}

		public bool Reparented {
			get {
				return reparented;
			}

			set {
				reparented = value;
			}
		}

		public uint Opacity {
			get {
				return opacity;
			}

			set {
				opacity = value;
			}
		}

		public XEventQueue Queue {
			get {
				return queue;
			}

			set {
				queue = value;
			}
		}

		public bool Enabled {
			get {
				if (!enabled) {
					return false;
				}

				if (parent != null) {
					return parent.Enabled;
				}

				return true;
			}

			set {
				enabled = value;
			}
		}

		public IntPtr EnabledHwnd {
			get {
				if (Enabled || parent == null) {
					return Handle;
				}

				return parent.EnabledHwnd;
			}
		}

		public Point MenuOrigin {
			get {
				Form frm = Control.FromHandle (handle) as Form;
				if (frm != null && frm.window_manager != null)		
					return frm.window_manager.GetMenuOrigin ();

				Point	pt;
				Size	border_3D_size = ThemeEngine.Current.Border3DSize;

				pt = new Point(0, 0);

				if (border_style == FormBorderStyle.Fixed3D) {
					pt.X += border_3D_size.Width;
					pt.Y += border_3D_size.Height;
				} else if (border_style == FormBorderStyle.FixedSingle) {
					pt.X += 1;
					pt.Y += 1;
				}

				if (this.title_style == TitleStyle.Normal)  {
					pt.Y += caption_height;
				} else if (this.title_style == TitleStyle.Tool)  {
					pt.Y += tool_caption_height;
				}

				return pt;
			}
		}

		public Rectangle Invalid {
			get {
				if (invalid_list.Count == 0)
					return Rectangle.Empty;

				Rectangle result = (Rectangle)invalid_list[0];
				for (int i = 1; i < invalid_list.Count; i ++) {
					result = Rectangle.Union (result, (Rectangle)invalid_list[i]);
				}
				return result;
			}
		}

		public Rectangle[] ClipRectangles {
			get {
				return (Rectangle[]) invalid_list.ToArray (typeof (Rectangle));
 			}
 		}

		public Rectangle NCInvalid {
			get { return nc_invalid; }
			set { nc_invalid = value; }

		}

		public bool NCExposePending {
			get {
				return nc_expose_pending;
			}
		}

		public Hwnd Parent {
			get {
				return parent;
			}

			set {
				if (parent != null)
					parent.children.Remove (this);
				parent = value;
				if (parent != null)
					parent.children.Add (this);
			}
		}

		public bool Mapped {
			get {
				if (!mapped) {
					return false;
				}

				if (parent != null) {
					return parent.Mapped;
				}

				return true;
			}

			set {
				mapped = value;
			}
		}

		public int CaptionHeight {
			get { return caption_height; }
			set { caption_height = value; }
		}

		public int ToolCaptionHeight {
			get { return tool_caption_height; }
			set { tool_caption_height = value; }
		}

		public TitleStyle TitleStyle {
			get {
				return title_style;
			}

			set {
				title_style = value;
			}
		}

		public object UserData {
			get {
				return user_data;
			}

			set {
				user_data = value;
			}
		}

		public IntPtr WholeWindow {
			get {
				return whole_window;
			}

			set {
				whole_window = value;

				zombie = false;

				if (whole_window != IntPtr.Zero) {
					lock (windows) {
						if (windows[whole_window] == null) {
							windows[whole_window] = this;
						}
					}
				}
			}
		}

		public int Width {
			get {
				return width;
			}

			set {
				width = value;
			}
		}

		public bool Visible {
			get {
				return visible;
			}

			set {
				visible = value;
			}
		}

		public int X {
			get {
				return x;
			}

			set {
				x = value;
			}
		}

		public int Y {
			get {
				return y;
			}

			set {
				y = value;
			}
		}

		#endregion	// Instance properties

		#region Methods
		public void AddInvalidArea(int x, int y, int width, int height) {
			AddInvalidArea(new Rectangle(x, y, width, height));
		}

		public void AddInvalidArea(Rectangle rect) {
			ArrayList tmp = new ArrayList ();
			foreach (Rectangle r in invalid_list) {
				if (!rect.Contains (r)) {
					tmp.Add (r);
				}
			}
			tmp.Add (rect);
			invalid_list = tmp;
		}

		public void ClearInvalidArea() {
			invalid_list.Clear();
			expose_pending = false;
		}

		public void AddNcInvalidArea(int x, int y, int width, int height) {
			if (nc_invalid == Rectangle.Empty) {
				nc_invalid = new Rectangle (x, y, width, height);
				return;
			}

			int right, bottom;
			right = Math.Max (nc_invalid.Right, x + width);
			bottom = Math.Max (nc_invalid.Bottom, y + height);
			nc_invalid.X = Math.Min (nc_invalid.X, x);
			nc_invalid.Y = Math.Min (nc_invalid.Y, y);

			nc_invalid.Width = right - nc_invalid.X;
			nc_invalid.Height = bottom - nc_invalid.Y;
		}

		public void AddNcInvalidArea(Rectangle rect) {
			if (nc_invalid == Rectangle.Empty) {
				nc_invalid = rect;
				return;
			}
			nc_invalid = Rectangle.Union (nc_invalid, rect);
		}

		public void ClearNcInvalidArea() {
			nc_invalid = Rectangle.Empty;
			nc_expose_pending = false;
		}

		public override string ToString() {
			return String.Format("Hwnd, Mapped:{3} ClientWindow:0x{0:X}, WholeWindow:0x{1:X}, Zombie={4}, Parent:[{2:X}]", client_window.ToInt32(), whole_window.ToInt32(), parent != null ? parent.ToString() : "<null>", Mapped, zombie);
		}

		public static Point GetNextStackedFormLocation (CreateParams cp)
		{
			if (cp.control == null)
				return Point.Empty;
		
			MdiClient parent = cp.control.Parent as MdiClient;
			if (parent != null)
				return parent.GetNextStackedFormLocation (cp);

			int X = cp.X;
			int Y = cp.Y;
			Point previous, next;
			Rectangle within;

			previous = Hwnd.previous_main_startup_location;
			within = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;

			if (previous.X == int.MinValue || previous.Y == int.MinValue) {
				next = Point.Empty;
			} else {
				next = new Point (previous.X + 22, previous.Y + 22);
			}

			if (!within.Contains (next.X * 3, next.Y * 3)) {
				next = Point.Empty;
			}

			if (next == Point.Empty && cp.Parent == IntPtr.Zero) {
				next = new Point (22, 22);
			}

			Hwnd.previous_main_startup_location = next;

			return next;
		}

		#endregion	// Methods
		
		internal struct Borders
		{
			public int top;
			public int bottom;
			public int left;
			public int right;

			public void Inflate (Size size)
			{
				left += size.Width;
				right += size.Width;
				top += size.Height;
				bottom += size.Height;
			}

			public override string ToString ()
			{
				return string.Format("{{top={0}, bottom={1}, left={2}, right={3}}}", top, bottom, left, right);
			}
			
			public static bool operator == (Borders a, Borders b)
			{
				return (a.left == b.left && a.right == b.right && a.top == b.top && a.bottom == b.bottom);
			}
			
			public static bool operator != (Borders a, Borders b)
			{
				return !(a == b);
			}

			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}

			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}
		}
	}
}
