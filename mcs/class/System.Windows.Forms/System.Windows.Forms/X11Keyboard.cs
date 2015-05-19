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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
//


//
// TODO:
//  - dead chars are not translated properly
//  - There is a lot of potential for optimmization in here
// 
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	public enum XLookupStatus
	{
		XBufferOverflow = -1,
		XLookupNone = 1,
		XLookupChars = 2,
		XLookupKeySym = 3,
		XLookupBoth = 4
	}

	internal class X11Keyboard : IDisposable {
		internal static object XlibLock;

		private IntPtr display;
		private IntPtr client_window;
		private IntPtr xim;
		private Hashtable xic_table = new Hashtable ();
		private XIMPositionContext positionContext;
		private XIMCallbackContext callbackContext;
		private XIMProperties ximStyle;
		private EventMask xic_event_mask = EventMask.NoEventMask;
		private StringBuilder lookup_buffer;
		private byte [] lookup_byte_buffer = new byte [100];
		private int min_keycode, max_keycode, keysyms_per_keycode, syms;
		private int [] keyc2vkey = new int [256];
		private int [] keyc2scan = new int [256];
		private byte [] key_state_table = new byte [256];
		private int lcid;
		private bool num_state, cap_state;
		private bool initialized;
		private bool menu_state = false;
		private Encoding encoding;

		private int NumLockMask;
		private int AltGrMask;

		public X11Keyboard (IntPtr display, IntPtr clientWindow)
		{
			this.display = display;
			lookup_buffer = new StringBuilder (24);
			EnsureLayoutInitialized ();
		}

		private Encoding AnsiEncoding
		{
			get
			{
				if (encoding == null)
					encoding = Encoding.GetEncoding(new CultureInfo(lcid).TextInfo.ANSICodePage);
				return encoding;
			}
		}

		public IntPtr ClientWindow {
			get { return client_window; }
		}

		void IDisposable.Dispose ()
		{
			if (xim != IntPtr.Zero) {
				foreach (IntPtr xic in xic_table.Values)
					XDestroyIC (xic);
				xic_table.Clear ();

				XCloseIM (xim);
				xim = IntPtr.Zero;
			}
		}

		public void DestroyICForWindow (IntPtr window)
		{
			IntPtr xic = GetXic (window);
			if (xic != IntPtr.Zero) {
				xic_table.Remove ((long) window);
				XDestroyIC (xic);
			}
		}

		public void EnsureLayoutInitialized ()
		{
			if (initialized)
				return;

			KeyboardLayouts layouts = new KeyboardLayouts ();
			KeyboardLayout layout = DetectLayout (layouts);
			lcid = layout.Lcid;
			CreateConversionArray (layouts, layout);
			SetupXIM ();
			initialized = true;
		}

		private void SetupXIM ()
		{
			xim = IntPtr.Zero;

			if (!XSupportsLocale ()) {
				Console.Error.WriteLine ("X does not support your locale");
				return;
			}

			if (!XSetLocaleModifiers (String.Empty)) {
				Console.Error.WriteLine ("Could not set X locale modifiers");
				return;
			}

			if (Environment.GetEnvironmentVariable (ENV_NAME_XIM_STYLE) == "disabled") {
				return;
			}

			xim = XOpenIM (display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			if (xim == IntPtr.Zero) 
				Console.Error.WriteLine ("Could not get XIM");

			initialized = true;
		}

		void CreateXicForWindow (IntPtr window)
		{
			IntPtr xic = CreateXic (window, xim);
			xic_table [(long) window] = xic;
			if (xic == IntPtr.Zero)
				Console.Error.WriteLine ("Could not get XIC");
			else {
				if (XGetICValues (xic, "filterEvents", out xic_event_mask, IntPtr.Zero) != null)
					Console.Error.WriteLine ("Could not get XIC values");
				EventMask mask = EventMask.ExposureMask | EventMask.KeyPressMask | EventMask.FocusChangeMask;
				if ((xic_event_mask | mask) == xic_event_mask) {
					xic_event_mask |= mask;
					lock (XlibLock) {
						XplatUIX11.XSelectInput(display, window, new IntPtr ((int) xic_event_mask));
					}
				}
			}
		}

		public EventMask KeyEventMask {
			get { return xic_event_mask; }
		}
		
		public Keys ModifierKeys {
			get {
				Keys keys = Keys.None;
				if ((key_state_table [(int) VirtualKeys.VK_SHIFT] & 0x80) != 0)
					keys |= Keys.Shift;
				if ((key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) != 0)
					keys |= Keys.Control;
				if ((key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0)
					keys |= Keys.Alt;
				return keys;
			}
		}

		private IntPtr GetXic (IntPtr window)
		{
			if (xim != IntPtr.Zero && xic_table.ContainsKey ((long) window))
				return (IntPtr) xic_table [(long) window];
			else
				return IntPtr.Zero;
		}

		private bool FilterKey (XEvent e, int vkey)
		{
			if (XplatUI.key_filters.Count == 0)
				return false;
			XLookupStatus status;
			XKeySym ks;
			KeyFilterData data;
			data.Down = (e.type == XEventName.KeyPress);
			data.ModifierKeys = ModifierKeys;
			LookupString (ref e, 0, out ks, out status);
			data.keysym = (int)ks;
			data.keycode = e.KeyEvent.keycode;
			data.str = lookup_buffer.ToString (0, lookup_buffer.Length);
			return XplatUI.FilterKey (data);
		}

		public void FocusIn (IntPtr window)
		{
			this.client_window = window;
			if (xim == IntPtr.Zero)
				return;

			if (!xic_table.ContainsKey ((long) window))
				CreateXicForWindow (window);
			IntPtr xic = GetXic (window);
			if (xic != IntPtr.Zero)
				XSetICFocus (xic);
		}

		private bool have_Xutf8ResetIC = true;

		public void FocusOut (IntPtr window)
		{
			this.client_window = IntPtr.Zero;
			if (xim == IntPtr.Zero)
				return;

			IntPtr xic = GetXic (window);
			if (xic != IntPtr.Zero) {
				if (have_Xutf8ResetIC) {
					try {
						Xutf8ResetIC (xic);
					} catch (EntryPointNotFoundException) {
						have_Xutf8ResetIC = false;
					}
				}
				XUnsetICFocus (xic);
			}
		}

		public bool ResetKeyState(IntPtr hwnd, ref MSG msg) {
			// FIXME - keep defining events/msg and return true until we've 'restored' all
			// pending keypresses
			if ((key_state_table [(int) VirtualKeys.VK_SHIFT] & 0x80) != 0) {
				key_state_table [(int) VirtualKeys.VK_SHIFT] &=  unchecked((byte)~0x80);
			}

			if ((key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) != 0) {
				key_state_table [(int) VirtualKeys.VK_CONTROL] &=  unchecked((byte)~0x80);
			}

			if ((key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0) {
				key_state_table [(int) VirtualKeys.VK_MENU] &=  unchecked((byte)~0x80);
			}
			return false;
		}

		// Almost identical to UpdateKeyState() but does not call LookupString().
		// The actual purpose is to handle shift state correctly.
		public void PreFilter (XEvent xevent)
		{
			// It is still possible that some keyboards could have some shift
			// keys outside this range, but let's think about such cases only
			// if it actually happened.
			if (xevent.KeyEvent.keycode >= keyc2vkey.Length)
				return;

			int vkey = keyc2vkey [xevent.KeyEvent.keycode];

			switch (xevent.type) {
			case XEventName.KeyRelease:
				key_state_table [vkey & 0xff] &= unchecked ((byte) ~0x80);
				break;
			case XEventName.KeyPress:
				if ((key_state_table [vkey & 0xff] & 0x80) == 0) {
					key_state_table [vkey & 0xff] ^= 0x01;
				}
				key_state_table [vkey & 0xff] |= 0x80;
				break;
			}
		}

		public void KeyEvent (IntPtr hwnd, XEvent xevent, ref MSG msg)
		{
			XKeySym keysym;
			int ascii_chars;

			XLookupStatus status;
			ascii_chars = LookupString (ref xevent, 24, out keysym, out status);

			if (((int) keysym >= (int) MiscKeys.XK_ISO_Lock && 
				(int) keysym <= (int) MiscKeys.XK_ISO_Last_Group_Lock) ||
				(int) keysym == (int) MiscKeys.XK_Mode_switch) {
				UpdateKeyState (xevent);
				return;
			}

			if ((xevent.KeyEvent.keycode >> 8) == 0x10)
				xevent.KeyEvent.keycode = xevent.KeyEvent.keycode & 0xFF;

			int event_time = (int)xevent.KeyEvent.time;

			if (status == XLookupStatus.XLookupChars) {
				// do not ignore those inputs. They are mostly from XIM.
				msg = SendImeComposition (lookup_buffer.ToString (0, lookup_buffer.Length));
				msg.hwnd = hwnd;
				return;
			}

			AltGrMask = xevent.KeyEvent.state & (0x6000 | (int) KeyMasks.ModMasks);
			int vkey = EventToVkey (xevent);
			if (vkey == 0 && ascii_chars != 0) {
				vkey = (int) VirtualKeys.VK_NONAME;
			}

			if (FilterKey (xevent, vkey))
				return;
			switch ((VirtualKeys) (vkey & 0xFF)) {
			case VirtualKeys.VK_NUMLOCK:
				GenerateMessage (VirtualKeys.VK_NUMLOCK, 0x45, xevent.KeyEvent.keycode, xevent.type, event_time);
				break;
			case VirtualKeys.VK_CAPITAL:
				GenerateMessage (VirtualKeys.VK_CAPITAL, 0x3A, xevent.KeyEvent.keycode, xevent.type, event_time);
				break;
			default:
				if (((key_state_table [(int) VirtualKeys.VK_NUMLOCK] & 0x01) == 0) != ((xevent.KeyEvent.state & NumLockMask) == 0)) {
					GenerateMessage (VirtualKeys.VK_NUMLOCK, 0x45, xevent.KeyEvent.keycode, XEventName.KeyPress, event_time);
					GenerateMessage (VirtualKeys.VK_NUMLOCK, 0x45, xevent.KeyEvent.keycode, XEventName.KeyRelease, event_time);
				}

				if (((key_state_table [(int) VirtualKeys.VK_CAPITAL] & 0x01) == 0) != ((xevent.KeyEvent.state & (int) KeyMasks.LockMask) == 0)) {
					GenerateMessage (VirtualKeys.VK_CAPITAL, 0x3A, xevent.KeyEvent.keycode, XEventName.KeyPress, event_time);
					GenerateMessage (VirtualKeys.VK_CAPITAL, 0x3A, xevent.KeyEvent.keycode, XEventName.KeyRelease, event_time);
				}

				num_state = false;
				cap_state = false;

				int bscan = (keyc2scan [xevent.KeyEvent.keycode] & 0xFF);
				KeybdEventFlags dw_flags = KeybdEventFlags.None;
				if (xevent.type == XEventName.KeyRelease)
					dw_flags |= KeybdEventFlags.KeyUp;
				if ((vkey & 0x100) != 0)
					dw_flags |= KeybdEventFlags.ExtendedKey;
				msg = SendKeyboardInput ((VirtualKeys) (vkey & 0xFF), bscan, xevent.KeyEvent.keycode, dw_flags, event_time);
				msg.hwnd = hwnd;
				break;
			}
		}

		public bool TranslateMessage (ref MSG msg)
		{
			bool res = false;

			if (msg.message >= Msg.WM_KEYFIRST && msg.message <= Msg.WM_KEYLAST)
				res = true;

			if (msg.message == Msg.WM_SYSKEYUP && msg.wParam == (IntPtr) 0x12 && menu_state) {
				msg.message = Msg.WM_KEYUP;
				menu_state = false;
			}

			if (msg.message != Msg.WM_KEYDOWN && msg.message != Msg.WM_SYSKEYDOWN)
				return res;

			if ((key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0 && msg.wParam != (IntPtr) 0x12)
				menu_state = true;

			EnsureLayoutInitialized ();

			string buffer;
			Msg message;
			int tu = ToUnicode ((int) msg.wParam, Control.HighOrder ((int) msg.lParam), out buffer);
			switch (tu) {
			case 1:
				message = (msg.message == Msg.WM_KEYDOWN ? Msg.WM_CHAR : Msg.WM_SYSCHAR);
				XplatUI.PostMessage (msg.hwnd, message, (IntPtr) buffer [0], msg.lParam);
				break;
			case -1:
				message = (msg.message == Msg.WM_KEYDOWN ? Msg.WM_DEADCHAR : Msg.WM_SYSDEADCHAR);
				XplatUI.PostMessage (msg.hwnd, message, (IntPtr) buffer [0], msg.lParam);
				return true;
			}
			
			return res;
		}

		public int ToKeycode(int key) 
		{
			int keycode = 0;
			
			if (nonchar_vkey_key[key] > 0)
				keycode = XKeysymToKeycode (display, nonchar_vkey_key[key]);
			
			if (keycode == 0)
				keycode = XKeysymToKeycode (display, key);

			return keycode;		
		}

		public int ToUnicode (int vkey, int scan, out string buffer)
		{
			if ((scan & 0x8000) != 0) {
				buffer = String.Empty;
				return 0;
			}

			XEvent e = new XEvent ();
			e.AnyEvent.type = XEventName.KeyPress;
			e.KeyEvent.display = display;
			e.KeyEvent.keycode = 0;
			e.KeyEvent.state = 0;

			if ((key_state_table [(int) VirtualKeys.VK_SHIFT] & 0x80) != 0) {
				e.KeyEvent.state |= (int) KeyMasks.ShiftMask;
			}

			if ((key_state_table [(int) VirtualKeys.VK_CAPITAL] & 0x01) != 0) {
				e.KeyEvent.state |= (int) KeyMasks.LockMask;
			}

			if ((key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) != 0) {
				e.KeyEvent.state |= (int) KeyMasks.ControlMask;
			}

			if ((key_state_table [(int) VirtualKeys.VK_NUMLOCK] & 0x01) != 0) {
				e.KeyEvent.state |= NumLockMask;
			}

			e.KeyEvent.state |= AltGrMask;

			for (int keyc = min_keycode; (keyc <= max_keycode) && (e.KeyEvent.keycode == 0); keyc++) {
				// find keycode that could have generated this vkey
				if ((keyc2vkey [keyc] & 0xFF) == vkey) {
					// filter extended bit because it is not known
					e.KeyEvent.keycode = keyc;
					if ((EventToVkey (e) & 0xFF) != vkey) {
						// Wrong one (ex: because of num,lock state)
						e.KeyEvent.keycode = 0;
					}
				}
			}

			if ((vkey >= (int) VirtualKeys.VK_NUMPAD0) && (vkey <= (int) VirtualKeys.VK_NUMPAD9))
				e.KeyEvent.keycode = XKeysymToKeycode (display, vkey - (int) VirtualKeys.VK_NUMPAD0 + (int) KeypadKeys.XK_KP_0);

			if (vkey == (int) VirtualKeys.VK_DECIMAL)
				e.KeyEvent.keycode = XKeysymToKeycode (display, (int) KeypadKeys.XK_KP_Decimal);
			
			if (vkey == (int) VirtualKeys.VK_SEPARATOR)
				e.KeyEvent.keycode = XKeysymToKeycode(display, (int) KeypadKeys.XK_KP_Separator);

			if (e.KeyEvent.keycode == 0 && vkey != (int) VirtualKeys.VK_NONAME) {
				// And I couldn't find the keycode so i returned the vkey and was like whatever
				Console.Error.WriteLine ("unknown virtual key {0:X}", vkey);
				buffer = String.Empty;
				return vkey; 
			}

			XKeySym t;
			XLookupStatus status;
			int res = LookupString (ref e, 24, out t, out status);
			int keysym = (int) t;

			buffer = String.Empty;
			if (res == 0) {
				int dead_char = MapDeadKeySym (keysym);
				if (dead_char != 0) {
					byte [] bytes = new byte [1];
					bytes [0] = (byte) dead_char;
					buffer = new string (AnsiEncoding.GetChars (bytes));
					res = -1;
				}
			} else {
				// Shift + arrow, shift + home, ....
				// X returns a char for it, but windows doesn't
				if (((e.KeyEvent.state & NumLockMask) == 0) && ((e.KeyEvent.state & (int) KeyMasks.ShiftMask) != 0) &&
						(keysym >= (int) KeypadKeys.XK_KP_0) && (keysym <= (int) KeypadKeys.XK_KP_9)) {
					buffer = String.Empty;
					res = 0;
				}

				// CTRL + number, X returns chars, windows does not
				if ((e.KeyEvent.state & (int) KeyMasks.ControlMask) != 0) {
					if (((keysym >= 33) && (keysym < 'A')) || ((keysym > 'Z') && (keysym < 'a'))) {
						buffer = String.Empty;
						res = 0;
					}
				}

				// X returns a char for delete key on extended keyboards, windows does not
				if (keysym == (int) TtyKeys.XK_Delete) {
					buffer = String.Empty;
					res = 0;
				}

				if (keysym == (int) TtyKeys.XK_BackSpace && (key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) != 0) {
					buffer = new string (new char [] { (char) 127 });

					return 1;
				}

				// For some special chars, such backspace and enter, looking up for a string
				// can randomly fail to properly fill the buffer (either marshaling or X11), so we use
				// the keysysm value to fill the gap
				if (keysym == (int) XKeySym.XK_BackSpace) {
					buffer = new string (new char [] { (char) 8 });
					return 1;
				}
				if (keysym == (int) XKeySym.XK_Return) {
					buffer = new string (new char [] { (char)13 });
					return 1;
				}

				if (res != 0) {
					buffer = lookup_buffer.ToString ();
					res = buffer.Length;
				}
			}

			return res;
		}

		string stored_keyevent_string;

		internal string GetCompositionString ()
		{
			return stored_keyevent_string;
		}

		private MSG SendImeComposition (string s)
		{
			MSG msg = new MSG ();
			msg.message = Msg.WM_IME_COMPOSITION;
			msg.refobject = s;
			stored_keyevent_string = s;
			return msg;
		}

		private MSG SendKeyboardInput (VirtualKeys vkey, int scan, int keycode, KeybdEventFlags dw_flags, int time)
		{
			Msg message;

			if ((dw_flags & KeybdEventFlags.KeyUp) != 0) {
				bool sys_key = (key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0 &&
					      ((key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) == 0);
				key_state_table [(int) vkey] &= unchecked ((byte) ~0x80);
				message = (sys_key ? Msg.WM_SYSKEYUP : Msg.WM_KEYUP);
			} else {
				if ((key_state_table [(int) vkey] & 0x80) == 0) {
					key_state_table [(int) vkey] ^= 0x01;
				}
				key_state_table [(int) vkey] |= 0x80;
				bool sys_key = (key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0 &&
					      ((key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) == 0);
				message = (sys_key ? Msg.WM_SYSKEYDOWN : Msg.WM_KEYDOWN);
			}

			MSG msg = new MSG ();
			msg.message = message;
			msg.wParam = (IntPtr) vkey;
			msg.lParam = GenerateLParam (msg, keycode);
			return msg;
		}

		private IntPtr GenerateLParam (MSG m, int keyCode)
		{
			// http://msdn.microsoft.com/en-us/library/ms646267(VS.85).aspx
			//
			byte flags = 0;

			if (m.message == Msg.WM_SYSKEYUP || m.message == Msg.WM_KEYUP)
				flags |= 0x80; // transition state flag = 1

			flags |= 0x40; // previous key state flag = 1

			if ((key_state_table [(int) VirtualKeys.VK_RMENU] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_LMENU] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0)
				flags |= 0x20; // context code flag = 1

			if ((key_state_table [(int) VirtualKeys.VK_INSERT] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_DELETE] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_HOME] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_END] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_UP] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_DOWN] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_LEFT] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_RIGHT] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_NUMLOCK] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_PRINT] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_RETURN] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_DIVIDE] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_PRIOR] & 0x80) != 0 ||
			    (key_state_table [(int) VirtualKeys.VK_NEXT] & 0x80) != 0)
				flags |= 0x01; // extended key flag = 1

			int lparam = ((((int)flags) & 0x000000FF) << 3*8); // message flags
			lparam |= ((keyCode & 0x000000FF) << 2*8); // scan code
			lparam |= 0x00000001; // repeat count = 1

			return (IntPtr)lparam;
		}

		private void GenerateMessage (VirtualKeys vkey, int scan, int key_code, XEventName type, int event_time)
		{
			bool state = (vkey == VirtualKeys.VK_NUMLOCK ? num_state : cap_state);
			KeybdEventFlags up, down;

			if (state) {
				// The INTERMEDIARY state means : just after a 'press' event, if a 'release' event comes,
				// don't treat it. It's from the same key press. Then the state goes to ON.
				// And from there, a 'release' event will switch off the toggle key.
				SetState (vkey, false);
			} else {
				down = (vkey == VirtualKeys.VK_NUMLOCK ? KeybdEventFlags.ExtendedKey : KeybdEventFlags.None);
				up = (vkey == VirtualKeys.VK_NUMLOCK ? KeybdEventFlags.ExtendedKey :
						KeybdEventFlags.None) | KeybdEventFlags.KeyUp;
				if ((key_state_table [(int) vkey] & 0x1) != 0) { // it was on
					if (type != XEventName.KeyPress) {
						SendKeyboardInput (vkey, scan, key_code, down, event_time);
						SendKeyboardInput (vkey, scan, key_code, up, event_time);
						SetState (vkey, false);
						key_state_table [(int) vkey] &= unchecked ((byte) ~0x01);
					}
				} else {
					if (type == XEventName.KeyPress) {
						SendKeyboardInput (vkey, scan, key_code, down, event_time);
						SendKeyboardInput (vkey, scan, key_code, up, event_time);
						SetState (vkey, true);
						key_state_table [(int) vkey] |= 0x01;
					}
				}
			}
		}

		private void UpdateKeyState (XEvent xevent)
		{
			int vkey = EventToVkey (xevent);

			switch (xevent.type) {
			case XEventName.KeyRelease:
				key_state_table [vkey & 0xff] &= unchecked ((byte) ~0x80);
				break;
			case XEventName.KeyPress:
				if ((key_state_table [vkey & 0xff] & 0x80) == 0) {
					key_state_table [vkey & 0xff] ^= 0x01;
				}
				key_state_table [vkey & 0xff] |= 0x80;
				break;
			}
		}

		private void SetState (VirtualKeys key, bool state)
		{
			if (VirtualKeys.VK_NUMLOCK == key)
				num_state = state;
			else
				cap_state = state;
		}

		public int EventToVkey (XEvent e)
		{
			XLookupStatus status;
			XKeySym ks;

			LookupString (ref e, 0, out ks, out status);
			int keysym = (int) ks;

			if (((e.KeyEvent.state & NumLockMask) != 0) &&
			    (keysym == (int)KeypadKeys.XK_KP_Separator || keysym == (int)KeypadKeys.XK_KP_Decimal ||
			    (keysym >= (int)KeypadKeys.XK_KP_0 && keysym <= (int)KeypadKeys.XK_KP_9))) {
				// Only the Keypad keys 0-9 and . send different keysyms
				// depending on the NumLock state
				return nonchar_key_vkey [keysym & 0xFF];
			}

			return keyc2vkey [e.KeyEvent.keycode];
		}

		private void CreateConversionArray (KeyboardLayouts layouts, KeyboardLayout layout)
		{
			XEvent e2 = new XEvent ();
			uint keysym = 0;
			int [] ckey = new int [] { 0, 0, 0, 0 };

			e2.KeyEvent.display = display;
			e2.KeyEvent.state = 0;

			for (int keyc = min_keycode; keyc <= max_keycode; keyc++) {
				int vkey = 0;
				int scan = 0;

				e2.KeyEvent.keycode = keyc;
				XKeySym t;

				XLookupStatus status;
				LookupString (ref e2, 0, out t, out status);

				keysym = (uint) t;
				if (keysym != 0) {
					if ((keysym >> 8) == 0xFF) {
						vkey = nonchar_key_vkey [keysym & 0xFF];
						scan = nonchar_key_scan [keysym & 0xFF];
						// Set extended bit
						if ((scan & 0x100) != 0)
							vkey |= 0x100;
					} else if (keysym == 0x20) { // spacebar
						vkey = (int) VirtualKeys.VK_SPACE;
						scan = 0x39;
					} else {
						// Search layout dependent scancodes
						int maxlen = 0;
						int maxval = -1;;
						
						for (int i = 0; i < syms; i++) {
							keysym = XKeycodeToKeysym (display, keyc, i);
							if ((keysym < 0x800) && (keysym != ' '))
								ckey [i] = (sbyte) (keysym & 0xFF);
							else
								ckey [i] = (sbyte) MapDeadKeySym ((int) keysym);
						}
						
						for (int keyn = 0; keyn < layout.Keys.Length; keyn++) {
							int ml = Math.Min (layout.Keys [keyn].Length, 4);
							int ok = -1;
							for (int i = 0; (ok != 0) && (i < ml); i++) {
								sbyte ck = (sbyte) layout.Keys [keyn][i];
								if (ck != ckey [i])
									ok = 0;
								if ((ok != 0) || (i > maxlen)) {
									maxlen = i;
									maxval = keyn;
								}
								if (ok != 0)
									break;
							}
						}
						if (maxval >= 0) {
							if (maxval < layouts.scan_table [(int) layout.ScanIndex].Length)
								scan = layouts.scan_table [(int) layout.ScanIndex][maxval];
							if (maxval < layouts.vkey_table [(int) layout.VKeyIndex].Length)
								vkey = layouts.vkey_table [(int) layout.VKeyIndex][maxval];
						}
					}
				}
				keyc2vkey [e2.KeyEvent.keycode] = vkey;
				keyc2scan [e2.KeyEvent.keycode] = scan;
			}
			
			
		}

		private KeyboardLayout DetectLayout (KeyboardLayouts layouts)
		{
			XDisplayKeycodes (display, out min_keycode, out max_keycode);
			IntPtr ksp = XGetKeyboardMapping (display, (byte) min_keycode,
					max_keycode + 1 - min_keycode, out keysyms_per_keycode);
			lock (XlibLock) {
				XplatUIX11.XFree (ksp);
			}

			syms = keysyms_per_keycode;
			if (syms > 4) {
				//Console.Error.WriteLine ("{0} keysymbols per a keycode is not supported, setting to 4", syms);
				syms = 2;
			}

			IntPtr	modmap_unmanaged;
			XModifierKeymap xmk = new XModifierKeymap ();

			modmap_unmanaged = XGetModifierMapping (display);
			xmk = (XModifierKeymap) Marshal.PtrToStructure (modmap_unmanaged, typeof (XModifierKeymap));

			int mmp = 0;
			for (int i = 0; i < 8; i++) {
				for (int j = 0; j < xmk.max_keypermod; j++, mmp++) {
					byte b = Marshal.ReadByte (xmk.modifiermap, mmp);
					if (b != 0) {
						for (int k = 0; k < keysyms_per_keycode; k++) {
							if ((int) XKeycodeToKeysym (display, b, k) == (int) MiscKeys.XK_Num_Lock)
								NumLockMask = 1 << i;
						}
					}
				}
			}
			XFreeModifiermap (modmap_unmanaged);

			int [] ckey = new int [4];
			KeyboardLayout layout = null;
			int max_score = 0;
			int max_seq = 0;
			
			foreach (KeyboardLayout current in layouts.Layouts) {
				int ok = 0;
				int score = 0;
				int match = 0;
				int mismatch = 0;
				int seq = 0;
				int pkey = -1;
				int key = min_keycode;
				int i;

				for (int keyc = min_keycode; keyc <= max_keycode; keyc++) {
					for (i = 0; i < syms; i++) {
						uint keysym = XKeycodeToKeysym (display, keyc, i);
						
						if ((keysym < 0x800) && (keysym != ' ')) {
							ckey [i] = (sbyte) (keysym & 0xFF);
						} else {
							ckey [i] = (sbyte) MapDeadKeySym ((int) keysym);
						}
					}
					if (ckey [0] != 0) {
						for (key = 0; key < current.Keys.Length; key++) {
							int ml = Math.Min (syms, current.Keys [key].Length);
							for (ok = 0, i = 0; (ok >= 0) && (i < ml); i++) {
								sbyte ck = (sbyte) current.Keys [key][i];
								if (ck != 0 && ck == ckey[i])
									ok++;
								if (ck != 0 && ck != ckey[i])
									ok = -1;
							}
							if (ok > 0) {
								score += ok;
								break;
							}
						}
						if (ok > 0) {
							match++;
							/* and how much the keycode order matches */
							if (key > pkey)
								seq++;
							pkey = key;
						} else {
							/* print spaces instead of \0's */
							mismatch++;
							score -= syms;
						}
					}
				}

				if ((score > max_score) || ((score == max_score) && (seq > max_seq))) {
					// best match so far
					layout = current;
					max_score = score;
					max_seq = seq;
				}
			}

			if (layout != null)  {
                                return layout;
			} else {
				Console.WriteLine (Locale.GetText("Keyboard layout not recognized, using default layout: " +
								   layouts.Layouts [0].Name));
			}

			return layouts.Layouts [0];
		}

		// TODO
		private int MapDeadKeySym (int val)
		{
			switch (val) {
			case (int) DeadKeys.XK_dead_tilde :
			case 0x1000FE7E : // Xfree's Dtilde
				return '~';
			case (int) DeadKeys.XK_dead_acute :
			case 0x1000FE27 : // Xfree's XK_Dacute_accent
				return 0xb4;
			case (int) DeadKeys.XK_dead_circumflex:
			case 0x1000FE5E : // Xfree's XK_.Dcircumflex_accent
				return '^';
			case (int) DeadKeys.XK_dead_grave :
			case 0x1000FE60 : // Xfree's XK_.Dgrave_accent
				return '`';
			case (int) DeadKeys.XK_dead_diaeresis :
			case 0x1000FE22 : // Xfree's XK_.Ddiaeresis
				return 0xa8;
			case (int) DeadKeys.XK_dead_cedilla :
				return 0xb8;
			case (int) DeadKeys.XK_dead_macron :
				return '-';
			case (int) DeadKeys.XK_dead_breve :
				return 0xa2;
			case (int) DeadKeys.XK_dead_abovedot :
				return 0xff;
			case (int) DeadKeys.XK_dead_abovering :
				return '0';
			case (int) DeadKeys.XK_dead_doubleacute :
				return 0xbd;
			case (int) DeadKeys.XK_dead_caron :
				return 0xb7;
			case (int) DeadKeys.XK_dead_ogonek :
				return 0xb2;
			}

			return 0;
		}

		private XIMProperties [] GetSupportedInputStyles (IntPtr xim)
		{
			IntPtr stylesPtr;
			string ret = XGetIMValues (xim, XNames.XNQueryInputStyle, out stylesPtr, IntPtr.Zero);
			if (ret != null || stylesPtr == IntPtr.Zero)
				return new XIMProperties [0];
			XIMStyles styles = (XIMStyles) Marshal.PtrToStructure (stylesPtr, typeof (XIMStyles));
			XIMProperties [] supportedStyles = new XIMProperties [styles.count_styles];
			for (int i = 0; i < styles.count_styles; i++)
				supportedStyles [i] = (XIMProperties) Marshal.PtrToStructure (new IntPtr ((long) styles.supported_styles + i * Marshal.SizeOf (typeof (IntPtr))), typeof (XIMProperties));
			lock (XlibLock) {
				XplatUIX11.XFree (stylesPtr);
			}
			return supportedStyles;
		}

		const XIMProperties styleRoot = XIMProperties.XIMPreeditNothing | XIMProperties.XIMStatusNothing;
		const XIMProperties styleOverTheSpot = XIMProperties.XIMPreeditPosition | XIMProperties.XIMStatusNothing;
		const XIMProperties styleOnTheSpot = XIMProperties.XIMPreeditCallbacks | XIMProperties.XIMStatusNothing;
		const string ENV_NAME_XIM_STYLE = "MONO_WINFORMS_XIM_STYLE";

		private XIMProperties [] GetPreferredStyles ()
		{
			string env = Environment.GetEnvironmentVariable (ENV_NAME_XIM_STYLE);
			if (env == null)
				env = "over-the-spot";
			string [] list = env.Split (' ');
			XIMProperties [] ret = new XIMProperties [list.Length];
			for (int i = 0; i < list.Length; i++) {
				string s = list [i];
				switch (s) {
				case "over-the-spot":
					ret [i] = styleOverTheSpot;
					break;
				case "on-the-spot":
					ret [i] = styleOnTheSpot;
					break;
				case "root":
					ret [i] = styleRoot;
					break;
				}
			}
			return ret;
		}

		private IEnumerable GetMatchingStylesInPreferredOrder (IntPtr xim)
		{
			XIMProperties [] supportedStyles = GetSupportedInputStyles (xim);
			foreach (XIMProperties p in GetPreferredStyles ())
				if (Array.IndexOf (supportedStyles, p) >= 0)
					yield return p;
		}

		private IntPtr CreateXic (IntPtr window, IntPtr xim)
		{
			IntPtr xic = IntPtr.Zero;
			foreach (XIMProperties targetStyle in GetMatchingStylesInPreferredOrder (xim)) {
				ximStyle = targetStyle;
				// FIXME: use __arglist when it gets working. See bug #321686
				switch (targetStyle) {
				case styleOverTheSpot:
					xic = CreateOverTheSpotXic (window, xim);
					if (xic != IntPtr.Zero)
						break;
					//Console.WriteLine ("failed to create XIC in over-the-spot mode.");
					continue;
				case styleOnTheSpot:
					// Since .NET/Winforms seems to support only over-the-spot mode,,
					// I'm not likely to continue on-the-spot implementation. But in
					// case we need it, this code will be still useful.
					xic = CreateOnTheSpotXic (window, xim);
					if (xic != IntPtr.Zero)
						break;
					//Console.WriteLine ("failed to create XIC in on-the-spot mode.");
					continue;
				case styleRoot:
					xic = XCreateIC (xim,
						XNames.XNInputStyle, styleRoot,
						XNames.XNClientWindow, window,
						IntPtr.Zero);
					break;
				}
			}
			// fall back to root mode if all modes failed
			if (xic == IntPtr.Zero) {
				ximStyle = styleRoot;
				xic = XCreateIC (xim,
					XNames.XNInputStyle, styleRoot,
					XNames.XNClientWindow, window,
					XNames.XNFocusWindow, window,
					IntPtr.Zero);
			}
			return xic;
		}

		private IntPtr CreateOverTheSpotXic (IntPtr window, IntPtr xim)
		{
			IntPtr list;
			int count;
			Control c = Control.FromHandle (window);
			string xlfd = String.Format ("-*-*-*-*-*-*-{0}-*-*-*-*-*-*-*", (int) c.Font.Size);
			IntPtr fontSet = XCreateFontSet (display, xlfd, out list, out count, IntPtr.Zero);
			XPoint spot = new XPoint ();
			spot.X = 0;
			spot.Y = 0;
			IntPtr pSL = IntPtr.Zero, pFS = IntPtr.Zero;
			try {
				pSL = Marshal.StringToHGlobalAnsi (XNames.XNSpotLocation);
				pFS = Marshal.StringToHGlobalAnsi (XNames.XNFontSet);
				IntPtr preedit = XVaCreateNestedList (0,
					pSL, spot,
					pFS, fontSet,
					IntPtr.Zero);
				return XCreateIC (xim,
					XNames.XNInputStyle, styleOverTheSpot,
					XNames.XNClientWindow, window,
					XNames.XNPreeditAttributes, preedit,
					IntPtr.Zero);
			} finally {
				if (pSL != IntPtr.Zero)
					Marshal.FreeHGlobal (pSL);
				if (pFS != IntPtr.Zero)
					Marshal.FreeHGlobal (pFS);
				XFreeStringList (list);
				//XplatUIX11.XFree (preedit);
				//XFreeFontSet (fontSet);
			}
		}

		private IntPtr CreateOnTheSpotXic (IntPtr window, IntPtr xim)
		{
			callbackContext = new XIMCallbackContext (window);
			return callbackContext.CreateXic (window, xim);
		}

		class XIMCallbackContext
		{
			XIMCallback startCB, doneCB, drawCB, caretCB;
			IntPtr pStartCB = IntPtr.Zero, pDoneCB = IntPtr.Zero, pDrawCB = IntPtr.Zero, pCaretCB = IntPtr.Zero;
			IntPtr pStartCBN = IntPtr.Zero, pDoneCBN = IntPtr.Zero, pDrawCBN = IntPtr.Zero, pCaretCBN = IntPtr.Zero;

			public XIMCallbackContext (IntPtr clientWindow)
			{
				startCB = new XIMCallback (clientWindow, DoPreeditStart);
				doneCB = new XIMCallback (clientWindow, DoPreeditDone);
				drawCB = new XIMCallback (clientWindow, DoPreeditDraw);
				caretCB = new XIMCallback (clientWindow, DoPreeditCaret);
				pStartCB = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (XIMCallback)));
				pDoneCB = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (XIMCallback)));
				pDrawCB = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (XIMCallback)));
				pCaretCB = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (XIMCallback)));
				pStartCBN = Marshal.StringToHGlobalAnsi (XNames.XNPreeditStartCallback);
				pDoneCBN = Marshal.StringToHGlobalAnsi (XNames.XNPreeditDoneCallback);
				pDrawCBN = Marshal.StringToHGlobalAnsi (XNames.XNPreeditDrawCallback);
				pCaretCBN = Marshal.StringToHGlobalAnsi (XNames.XNPreeditCaretCallback);
			}

			~XIMCallbackContext ()
			{
				if (pStartCBN != IntPtr.Zero)
					Marshal.FreeHGlobal (pStartCBN);
				if (pDoneCBN != IntPtr.Zero)
					Marshal.FreeHGlobal (pDoneCBN);
				if (pDrawCBN != IntPtr.Zero)
					Marshal.FreeHGlobal (pDrawCBN);
				if (pCaretCBN != IntPtr.Zero)
					Marshal.FreeHGlobal (pCaretCBN);

				if (pStartCB != IntPtr.Zero)
					Marshal.FreeHGlobal (pStartCB);
				if (pDoneCB != IntPtr.Zero)
					Marshal.FreeHGlobal (pDoneCB);
				if (pDrawCB != IntPtr.Zero)
					Marshal.FreeHGlobal (pDrawCB);
				if (pCaretCB != IntPtr.Zero)
					Marshal.FreeHGlobal (pCaretCB);
			}

			int DoPreeditStart (IntPtr xic, IntPtr clientData, IntPtr callData)
			{
				Debug.WriteLine ("DoPreeditStart");
				XplatUI.SendMessage(clientData, Msg.WM_XIM_PREEDITSTART, clientData, callData);
				return 100;
			}

			int DoPreeditDone (IntPtr xic, IntPtr clientData, IntPtr callData)
			{
				Debug.WriteLine ("DoPreeditDone");
				XplatUI.SendMessage(clientData, Msg.WM_XIM_PREEDITDONE, clientData, callData);
				return 0;
			}

			int DoPreeditDraw (IntPtr xic, IntPtr clientData, IntPtr callData)
			{
				Debug.WriteLine ("DoPreeditDraw");
				XplatUI.SendMessage(clientData, Msg.WM_XIM_PREEDITDRAW, clientData, callData);
				return 0;
			}

			int DoPreeditCaret (IntPtr xic, IntPtr clientData, IntPtr callData)
			{
				Debug.WriteLine ("DoPreeditCaret");
				XplatUI.SendMessage(clientData, Msg.WM_XIM_PREEDITCARET, clientData, callData);
				return 0;
			}

			public IntPtr CreateXic (IntPtr window, IntPtr xim)
			{
				Marshal.StructureToPtr (startCB, pStartCB, false);
				Marshal.StructureToPtr (doneCB, pDoneCB, false);
				Marshal.StructureToPtr (drawCB, pDrawCB, false);
				Marshal.StructureToPtr (caretCB, pCaretCB, false);
				IntPtr preedit = XVaCreateNestedList (0,
					pStartCBN, pStartCB,
					pDoneCBN, pDoneCB,
					pDrawCBN, pDrawCB,
					pCaretCBN, pCaretCB,
					IntPtr.Zero);
				return XCreateIC (xim,
					XNames.XNInputStyle, styleOnTheSpot,
					XNames.XNClientWindow, window,
					XNames.XNPreeditAttributes, preedit,
					IntPtr.Zero);
			}
		}

		class XIMPositionContext
		{
			public CaretStruct Caret;
			public int X;
			public int Y;
		}

		internal void SetCaretPos (CaretStruct caret, IntPtr handle, int x, int y)
		{
			if (ximStyle != styleOverTheSpot)
				return;

			if (positionContext == null)
				this.positionContext = new XIMPositionContext ();

			positionContext.Caret = caret;
			positionContext.X = x;
			positionContext.Y = y + caret.Height;

			MoveCurrentCaretPos ();
		}

		internal void MoveCurrentCaretPos ()
		{
			if (positionContext == null || ximStyle != styleOverTheSpot || client_window == IntPtr.Zero)
				return;

			int x = positionContext.X;
			int y = positionContext.Y;
			CaretStruct caret = positionContext.Caret;
			IntPtr xic = GetXic (client_window);
			if (xic == IntPtr.Zero)
				return;
			Control control = Control.FromHandle (client_window);
			if (control == null || !control.IsHandleCreated)
				return;
			control = Control.FromHandle (caret.Hwnd);
			if (control == null || !control.IsHandleCreated)
				return;
			Hwnd hwnd = Hwnd.ObjectFromHandle (client_window);
			if (!hwnd.mapped)
				return;

			int dx, dy;
			IntPtr child;
			lock (XlibLock) {
				XplatUIX11.XTranslateCoordinates (display, client_window, client_window, x, y, out dx, out dy, out child);
			}

			XPoint spot = new XPoint ();
			spot.X = (short) dx;
			spot.Y = (short) dy;

			IntPtr pSL = IntPtr.Zero;
			try {
				pSL = Marshal.StringToHGlobalAnsi (XNames.XNSpotLocation);
				IntPtr preedit = XVaCreateNestedList (0, pSL, spot, IntPtr.Zero);
				XSetICValues (xic, XNames.XNPreeditAttributes, preedit, IntPtr.Zero);
			} finally {
				if (pSL != IntPtr.Zero)
					Marshal.FreeHGlobal (pSL);
			}
		}

		private bool have_Xutf8LookupString = true;

		private int LookupString (ref XEvent xevent, int len, out XKeySym keysym, out XLookupStatus status)
		{
			IntPtr keysym_res;
			int res;

			status = XLookupStatus.XLookupNone;
			IntPtr xic = GetXic (client_window);
			if (xic != IntPtr.Zero && have_Xutf8LookupString && xevent.type == XEventName.KeyPress) {
				do {
					try {
						res = Xutf8LookupString (xic, ref xevent, lookup_byte_buffer, 100, out keysym_res,  out status);
					} catch (EntryPointNotFoundException) {
						have_Xutf8LookupString = false;

						// call again, this time we'll go through the non-xic clause
						return LookupString (ref xevent, len, out keysym, out status);
					}
					if (status != XLookupStatus.XBufferOverflow)
						break;
					lookup_byte_buffer = new byte [lookup_byte_buffer.Length << 1];
				} while (true);
				lookup_buffer.Length = 0;
				string s = Encoding.UTF8.GetString (lookup_byte_buffer, 0, res);
				lookup_buffer.Append (s);
				keysym = (XKeySym) keysym_res.ToInt32 ();
				return s.Length;
			} else {
				IntPtr statusPtr = IntPtr.Zero;
				lookup_buffer.Length = 0;
				res = XLookupString (ref xevent, lookup_buffer, len, out keysym_res, out statusPtr);
				keysym = (XKeySym) keysym_res.ToInt32 ();
				return res;
			}
		}

		[DllImport ("libX11")]
		private static extern IntPtr XOpenIM (IntPtr display, IntPtr rdb, IntPtr res_name, IntPtr res_class);

		[DllImport ("libX11", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr XCreateIC (IntPtr xim, string name, XIMProperties im_style, string name2, IntPtr value2, IntPtr terminator);
		[DllImport ("libX11", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr XCreateIC (IntPtr xim, string name, XIMProperties im_style, string name2, IntPtr value2, string name3, IntPtr value3, IntPtr terminator);
//		[DllImport ("libX11", CallingConvention = CallingConvention.Cdecl)]
//		private static extern IntPtr XCreateIC (IntPtr xim, string name, XIMProperties im_style, string name2, IntPtr value2, string name3, IntPtr value3, string name4, IntPtr value4, IntPtr terminator);

		[DllImport ("libX11", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr XVaCreateNestedList (int dummy, IntPtr name0, XPoint value0, IntPtr terminator);
		[DllImport ("libX11", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr XVaCreateNestedList (int dummy, IntPtr name0, XPoint value0, IntPtr name1, IntPtr value1, IntPtr terminator);
		[DllImport ("libX11", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr XVaCreateNestedList (int dummy, IntPtr name0, IntPtr value0, IntPtr name1, IntPtr value1, IntPtr name2, IntPtr value2, IntPtr name3, IntPtr value3, IntPtr terminator);

		[DllImport ("libX11")]
		private static extern IntPtr XCreateFontSet (IntPtr display, string name, out IntPtr list, out int count, IntPtr terminator);

		[DllImport ("libX11")]
		internal extern static void XFreeFontSet (IntPtr data);

		[DllImport ("libX11")]
		private static extern void XFreeStringList (IntPtr ptr);

		//[DllImport ("libX11")]
		//private static extern IntPtr XIMOfIC (IntPtr xic);

		[DllImport ("libX11")]
		private static extern void XCloseIM (IntPtr xim);

		[DllImport ("libX11")]
		private static extern void XDestroyIC (IntPtr xic);

		[DllImport ("libX11")]
		private static extern string XGetIMValues (IntPtr xim, string name, out IntPtr value, IntPtr terminator);

		[DllImport ("libX11")]
		private static extern string XGetICValues (IntPtr xic, string name, out EventMask value, IntPtr terminator);

		[DllImport ("libX11", CallingConvention = CallingConvention.Cdecl)]
		private static extern void XSetICValues (IntPtr xic, string name, IntPtr value, IntPtr terminator);

		[DllImport ("libX11")]
		private static extern void XSetICFocus (IntPtr xic);

		[DllImport ("libX11")]
		private static extern void XUnsetICFocus (IntPtr xic);

		[DllImport ("libX11")]
		private static extern string Xutf8ResetIC (IntPtr xic);

		[DllImport ("libX11")]
		private static extern bool XSupportsLocale ();

		[DllImport ("libX11")]
		private static extern bool XSetLocaleModifiers (string mods);

		[DllImport ("libX11")]
		internal extern static int XLookupString(ref XEvent xevent, StringBuilder buffer, int num_bytes, out IntPtr keysym, out IntPtr status);
		[DllImport ("libX11")]
		internal extern static int Xutf8LookupString(IntPtr xic, ref XEvent xevent, byte [] buffer, int num_bytes, out IntPtr keysym, out XLookupStatus status);

		[DllImport ("libX11")]
		private static extern IntPtr XGetKeyboardMapping (IntPtr display, byte first_keycode, int keycode_count, 
				out int keysyms_per_keycode_return);

		[DllImport ("libX11")]
		private static extern void XDisplayKeycodes (IntPtr display, out int min, out int max);

		[DllImport ("libX11", EntryPoint="XKeycodeToKeysym")]
		private static extern uint XKeycodeToKeysym (IntPtr display, int keycode, int index);

		[DllImport ("libX11")]
		private static extern int XKeysymToKeycode (IntPtr display, IntPtr keysym);
		private static int XKeysymToKeycode (IntPtr display, int keysym) {
			return XKeysymToKeycode(display, (IntPtr)keysym);
		}

		[DllImport ("libX11")]
		internal extern static IntPtr XGetModifierMapping (IntPtr display);

		[DllImport ("libX11")]
		internal extern static int XFreeModifiermap (IntPtr modmap);


		private readonly static int [] nonchar_key_vkey = new int []
		{
			/* unused */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF00 */
			/* special keys */
			(int) VirtualKeys.VK_BACK, (int) VirtualKeys.VK_TAB, 0, (int) VirtualKeys.VK_CLEAR, 0, (int) VirtualKeys.VK_RETURN, 0, 0,	    /* FF08 */
			0, 0, 0, (int) VirtualKeys.VK_PAUSE, (int) VirtualKeys.VK_SCROLL, 0, 0, 0,			     /* FF10 */
			0, 0, 0, (int) VirtualKeys.VK_ESCAPE, 0, 0, 0, 0,			      /* FF18 */
			0, 0, (int) VirtualKeys.VK_NONCONVERT, (int) VirtualKeys.VK_CONVERT, 0, 0, 0, 0,					    /* FF20 */
			0, 0, (int) VirtualKeys.VK_OEM_AUTO, 0, 0, 0, 0, 0,					    /* FF28 */
			/* unused */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF30 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF38 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF40 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF48 */
			/* cursor keys */
			(int) VirtualKeys.VK_HOME, (int) VirtualKeys.VK_LEFT, (int) VirtualKeys.VK_UP, (int) VirtualKeys.VK_RIGHT,			    /* FF50 */
			(int) VirtualKeys.VK_DOWN, (int) VirtualKeys.VK_PRIOR, (int) VirtualKeys.VK_NEXT, (int) VirtualKeys.VK_END,
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF58 */
			/* misc keys */
			(int) VirtualKeys.VK_SELECT, (int) VirtualKeys.VK_SNAPSHOT, (int) VirtualKeys.VK_EXECUTE, (int) VirtualKeys.VK_INSERT, 0, 0, 0, 0,  /* FF60 */
			(int) VirtualKeys.VK_CANCEL, (int) VirtualKeys.VK_HELP, (int) VirtualKeys.VK_CANCEL, (int) VirtualKeys.VK_CANCEL, 0, 0, 0, 0,	    /* FF68 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF70 */
			/* keypad keys */
			0, 0, 0, 0, 0, 0, 0, (int) VirtualKeys.VK_NUMLOCK,			      /* FF78 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF80 */
			0, 0, 0, 0, 0, (int) VirtualKeys.VK_RETURN, 0, 0,			      /* FF88 */
			0, 0, 0, 0, 0, (int) VirtualKeys.VK_HOME, (int) VirtualKeys.VK_LEFT, (int) VirtualKeys.VK_UP,			  /* FF90 */
			(int) VirtualKeys.VK_RIGHT, (int) VirtualKeys.VK_DOWN, (int) VirtualKeys.VK_PRIOR, (int) VirtualKeys.VK_NEXT,			    /* FF98 */
			(int) VirtualKeys.VK_END, 0, (int) VirtualKeys.VK_INSERT, (int) VirtualKeys.VK_DELETE,
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFA0 */
			0, 0, (int) VirtualKeys.VK_MULTIPLY, (int) VirtualKeys.VK_ADD,					/* FFA8 */
			(int) VirtualKeys.VK_SEPARATOR, (int) VirtualKeys.VK_SUBTRACT, (int) VirtualKeys.VK_DECIMAL, (int) VirtualKeys.VK_DIVIDE,
			(int) VirtualKeys.VK_NUMPAD0, (int) VirtualKeys.VK_NUMPAD1, (int) VirtualKeys.VK_NUMPAD2, (int) VirtualKeys.VK_NUMPAD3,		    /* FFB0 */
			(int) VirtualKeys.VK_NUMPAD4, (int) VirtualKeys.VK_NUMPAD5, (int) VirtualKeys.VK_NUMPAD6, (int) VirtualKeys.VK_NUMPAD7,
			(int) VirtualKeys.VK_NUMPAD8, (int) VirtualKeys.VK_NUMPAD9, 0, 0, 0, 0,				/* FFB8 */
			/* function keys */
			(int) VirtualKeys.VK_F1, (int) VirtualKeys.VK_F2,
			(int) VirtualKeys.VK_F3, (int) VirtualKeys.VK_F4, (int) VirtualKeys.VK_F5, (int) VirtualKeys.VK_F6, (int) VirtualKeys.VK_F7, (int) VirtualKeys.VK_F8, (int) VirtualKeys.VK_F9, (int) VirtualKeys.VK_F10,    /* FFC0 */
			(int) VirtualKeys.VK_F11, (int) VirtualKeys.VK_F12, (int) VirtualKeys.VK_F13, (int) VirtualKeys.VK_F14, (int) VirtualKeys.VK_F15, (int) VirtualKeys.VK_F16, 0, 0,	/* FFC8 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFD0 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFD8 */
			/* modifier keys */
			0, (int) VirtualKeys.VK_SHIFT, (int) VirtualKeys.VK_SHIFT, (int) VirtualKeys.VK_CONTROL,			  /* FFE0 */
			(int) VirtualKeys.VK_CONTROL, (int) VirtualKeys.VK_CAPITAL, 0, (int) VirtualKeys.VK_MENU,
			(int) VirtualKeys.VK_MENU, (int) VirtualKeys.VK_MENU, (int) VirtualKeys.VK_MENU, 0, 0, 0, 0, 0,			  /* FFE8 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFF0 */
			0, 0, 0, 0, 0, 0, 0, (int) VirtualKeys.VK_DELETE			      /* FFF8 */
		};

		private static readonly int [] nonchar_key_scan = new int []
		{
			/* unused */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF00 */
			/* special keys */
			0x0E, 0x0F, 0x00, /*?*/ 0, 0x00, 0x1C, 0x00, 0x00,	     /* FF08 */
			0x00, 0x00, 0x00, 0x45, 0x46, 0x00, 0x00, 0x00,		     /* FF10 */
			0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,		     /* FF18 */
			/* unused */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF20 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF28 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF30 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF38 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF40 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF48 */
			/* cursor keys */
			0x147, 0x14B, 0x148, 0x14D, 0x150, 0x149, 0x151, 0x14F,	     /* FF50 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF58 */
			/* misc keys */
			/*?*/ 0, 0x137, /*?*/ 0, 0x152, 0x00, 0x00, 0x00, 0x00,	     /* FF60 */
			/*?*/ 0, /*?*/ 0, 0x38, 0x146, 0x00, 0x00, 0x00, 0x00,	     /* FF68 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF70 */
			/* keypad keys */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x138, 0x145,	     /* FF78 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF80 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x11C, 0x00, 0x00,	     /* FF88 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x47, 0x4B, 0x48,		     /* FF90 */
			0x4D, 0x50, 0x49, 0x51, 0x4F, 0x4C, 0x52, 0x53,		     /* FF98 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFA0 */
			0x00, 0x00, 0x37, 0x4E, /*?*/ 0, 0x4A, 0x53, 0x135,	     /* FFA8 */
			0x52, 0x4F, 0x50, 0x51, 0x4B, 0x4C, 0x4D, 0x47,		     /* FFB0 */
			0x48, 0x49, 0x00, 0x00, 0x00, 0x00,			     /* FFB8 */
			/* function keys */
			0x3B, 0x3C,
			0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44,		     /* FFC0 */
			0x57, 0x58, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFC8 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFD0 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFD8 */
			/* modifier keys */
			0x00, 0x2A, 0x36, 0x1D, 0x11D, 0x3A, 0x00, 0x38,	     /* FFE0 */
			0x138, 0x38, 0x138, 0x00, 0x00, 0x00, 0x00, 0x00,	     /* FFE8 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFF0 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x153		     /* FFF8 */
		};

		private readonly static int [] nonchar_vkey_key = new int []
		{
			0, 0, 0, 0, 0,		/* 00-04 */ 
			0, 0, 0, (int)XKeySym.XK_BackSpace, (int)XKeySym.XK_Tab,		/* 05-09 */
			0, 0, (int)XKeySym.XK_Clear, (int)XKeySym.XK_Return,	0, 0,	/* 0A-0F */
			(int)XKeySym.XK_Shift_L, (int)XKeySym.XK_Control_L, (int)XKeySym.XK_Menu, 0, (int)XKeySym.XK_Caps_Lock,		/* 10-14 */ 
			0, 0, 0, 0, 0,		/* 15-19 */
			0, 0, 0, 0,	0, 0,	/* 1A-1F */
			0, 0, 0, (int)XKeySym.XK_End, (int)XKeySym.XK_Home,		/* 20-24 */ 
			(int)XKeySym.XK_Left, (int)XKeySym.XK_Up, (int)XKeySym.XK_Right, (int)XKeySym.XK_Down, 0,		/* 25-29 */
			0, 0, 0, 0,	0, 0,	/* 2A-2F */
			0, 0, 0, 0, 0,		/* 30-34 */ 
			0, 0, 0, 0, 0,		/* 35-39 */
			0, 0, 0, 0,	0, 0,	/* 3A-3F */
			0, 0, 0, 0, 0,		/* 40-44 */ 
			0, 0, 0, 0, 0,		/* 45-49 */
			0, 0, 0, 0,	0, 0,	/* 4A-4F */
			0, 0, 0, 0, 0,		/* 50-54 */ 
			0, 0, 0, 0, 0,		/* 55-59 */
			0, (int)XKeySym.XK_Meta_L, (int)XKeySym.XK_Meta_R, 0,	0, 0,	/* 5A-5F */
			0, 0, 0, 0, 0,		/* 60-64 */ 
			0, 0, 0, 0, 0,		/* 65-69 */
			0, 0, 0, 0,	0, 0,	/* 6A-6F */
			0, 0, 0, 0, 0,		/* 70-74 */ 
			0, 0, 0, 0, 0,		/* 75-79 */
			0, 0, 0, 0,	0, 0,	/* 7A-7F */
			0, 0, 0, 0, 0,		/* 80-84 */ 
			0, 0, 0, 0, 0,		/* 85-89 */
			0, 0, 0, 0,	0, 0,	/* 8A-8F */
			0, 0, 0, 0, 0,		/* 90-94 */ 
			0, 0, 0, 0, 0,		/* 95-99 */
			0, 0, 0, 0,	0, 0,	/* 9A-9F */
			(int)XKeySym.XK_Shift_L, (int)XKeySym.XK_Shift_R, (int)XKeySym.XK_Control_L, (int)XKeySym.XK_Control_R, (int)XKeySym.XK_Alt_L,		/* A0-A4 */ 
			(int)XKeySym.XK_Alt_R, 0, 0, 0, 0,		/* A5-A9 */
			0, 0, 0, 0,	0, 0,	/* AA-AF */
			0, 0, 0, 0, 0,		/* B0-B4 */ 
			0, 0, 0, 0, 0,		/* B5-B9 */
			0, 0, 0, 0,	0, 0,	/* BA-BF */
			0, 0, 0, 0, 0,		/* C0-C4 */ 
			0, 0, 0, 0, 0,		/* C5-C9 */
			0, 0, 0, 0,	0, 0,	/* CA-CF */
			0, 0, 0, 0, 0,		/* D0-D4 */ 
			0, 0, 0, 0, 0,		/* D5-D9 */
			0, 0, 0, 0,	0, 0,	/* DA-DF */
			0, 0, 0, 0, 0,		/* E0-E4 */ 
			0, 0, 0, 0, 0,		/* E5-E9 */
			0, 0, 0, 0,	0, 0,	/* EA-EF */
			0, 0, 0, 0, 0,		/* F0-F4 */ 
			0, 0, 0, 0, 0,		/* F5-F9 */
			0, 0, 0, 0,	0, 0	/* FA-FF */
		};
		
	}

}

