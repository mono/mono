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
//  - detect numlock
//  - dead chars are not translated properly
//  - There is a lot of potential for optimmization in here
// 
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	internal class X11Keyboard {

		private IntPtr display;
		private int min_keycode, max_keycode, keysyms_per_keycode, syms;
		private int [] keyc2vkey = new int [256];
		private int [] keyc2scan = new int [256];
		private byte [] key_state_table = new byte [256];
		private bool num_state, cap_state;
		private KeyboardLayout layout;

		// TODO
		private int NumLockMask;
		private int AltGrMask;
		
		public X11Keyboard (IntPtr display)
		{
			this.display = display;
			DetectLayout ();
			CreateConversionArray (layout);
		}

		public void KeyEvent (IntPtr hwnd, XEvent xevent, ref MSG msg)
		{
			if ((xevent.KeyEvent.keycode >> 8) == 0x10)
				xevent.KeyEvent.keycode = xevent.KeyEvent.keycode & 0xFF;

			int event_time = xevent.KeyEvent.time;
			int vkey = EventToVkey (xevent);

			if (vkey == 0)
				return;

			switch ((VirtualKeys) (vkey & 0xFF)) {
			case VirtualKeys.VK_NUMLOCK:
				GenerateMessage (VirtualKeys.VK_NUMLOCK, 0x45, xevent.type, event_time);
				break;
			case VirtualKeys.VK_CAPITAL:
				GenerateMessage (VirtualKeys.VK_CAPITAL, 0x3A, xevent.type, event_time);
				break;
			default:

				if (((key_state_table [(int) VirtualKeys.VK_NUMLOCK] & 0x01) == 0) != ((xevent.KeyEvent.state & NumLockMask) == 0)) {
					GenerateMessage (VirtualKeys.VK_NUMLOCK, 0x45, XEventName.KeyPress, event_time);
					GenerateMessage (VirtualKeys.VK_NUMLOCK, 0x45, XEventName.KeyRelease, event_time);
				}

				if (((key_state_table [(int) VirtualKeys.VK_CAPITAL] & 0x01) == 0) != ((xevent.KeyEvent.state & (int) KeyMasks.LockMask) == 0)) {
					GenerateMessage (VirtualKeys.VK_CAPITAL, 0x3A, XEventName.KeyPress, event_time);
					GenerateMessage (VirtualKeys.VK_CAPITAL, 0x3A, XEventName.KeyRelease, event_time);
				}

				num_state = false;
				cap_state = false;

				int bscan = (keyc2scan [xevent.KeyEvent.keycode] & 0xFF);
				KeybdEventFlags dw_flags = KeybdEventFlags.None;
				if (xevent.type == XEventName.KeyRelease)
					dw_flags |= KeybdEventFlags.KeyUp;
				if ((vkey & 0x100) != 0)
					dw_flags |= KeybdEventFlags.ExtendedKey;
				msg = SendKeyboardInput ((VirtualKeys) (vkey & 0xFF), bscan, dw_flags, event_time);
				break;
			}
		}

		public bool TranslateMessage (ref MSG msg)
		{
			bool res = false;

			if (msg.message >= Msg.WM_KEYFIRST && msg.message <= Msg.WM_KEYLAST)
				res = true;

			if (msg.message != Msg.WM_KEYDOWN && msg.message != Msg.WM_SYSKEYDOWN)
				return res;

			string buffer;
			Msg message;

			int tu = ToUnicode ((int) msg.wParam, Control.HighOrder ((int) msg.lParam), out buffer);
			switch (tu) {
			case 1:
				message = (msg.message == Msg.WM_KEYDOWN ? Msg.WM_CHAR : Msg.WM_SYSCHAR);
				XplatUIX11.PostMessage (msg.hwnd, message, (IntPtr) buffer [0], msg.lParam);
				break;
			case -1:
				message = (msg.message == Msg.WM_KEYDOWN ? Msg.WM_DEADCHAR : Msg.WM_SYSDEADCHAR);
				XplatUIX11.PostMessage (msg.hwnd, message, (IntPtr) buffer [0], msg.lParam);
				return true;
			}
			
			return res;
		}

		private int ToUnicode (int vkey, int scan, out string buffer)
		{
			if ((scan & 0x8000) != 0) {
				buffer = String.Empty;
				return 0;
			}

			XEvent e = new XEvent ();
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

			if (e.KeyEvent.keycode == 0) {
				// And I couldn't find the keycode so i returned the vkey and was like whatever
				Console.Error.WriteLine ("unknown virtual key {0:X}", vkey);
				buffer = String.Empty;
				return vkey; 
			}

			IntPtr	buf = Marshal.AllocHGlobal (2);
			XKeySym t;
			int res = XLookupString (ref e, buf, 2, out t, IntPtr.Zero);
			int keysym = (int) t;

			buffer = String.Empty;
			if (res == 0) {
				int dead_char = MapDeadKeySym (keysym);
				// TODO: deal with dead chars
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

				if (res != 0) {
					byte [] bytes = new byte [2];
					bytes [0] = Marshal.ReadByte (buf);
					bytes [1] = Marshal.ReadByte (buf, 1);
					Encoding encoding = Encoding.GetEncoding (layout.CodePage);
					buffer = new string (encoding.GetChars (bytes));
				}
			}

			return res;
		}

		private MSG SendKeyboardInput (VirtualKeys vkey, int scan, KeybdEventFlags dw_flags, int time)
		{
			Msg message;

			if ((dw_flags & KeybdEventFlags.KeyUp) != 0) {
				bool sys_key = (key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0 &&
					      ((key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) == 0);
				key_state_table [(int) vkey] &= ~0x80;
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
			msg.lParam = IntPtr.Zero;

			return msg;
		}

		private void GenerateMessage (VirtualKeys vkey, int scan, XEventName type, int event_time)
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
						// SendKeyboardInput (vkey, scan, down, event_time);
						// SendKeyboardInput (vkey, scan, up, event_time);
						SetState (vkey, false);
						key_state_table [(int) vkey] &= ~0x01;
					}
				} else {
					if (type == XEventName.KeyPress) {
						// SendKeyboardInput (vkey, scan, down, event_time);
						// SendKeyboardInput (vkey, scan, up, event_time);
						SetState (vkey, true);
						key_state_table [(int) vkey] |= 0x01;
					}
				}
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
			XKeySym ks;

			XLookupString (ref e, IntPtr.Zero, 0, out ks, IntPtr.Zero);
			int keysym = (int) ks;

			if ((keysym >= 0xFFAE) && (keysym <= 0xFFB9) && (keysym != 0xFFAF)
					&& ((e.KeyEvent.state & NumLockMask) !=0)) {
				// Only the Keypad keys 0-9 and . send different keysyms
				// depending on the NumLock state
				return KeyboardLayouts.nonchar_key_vkey [keysym & 0xFF];
			}

			return keyc2vkey [e.KeyEvent.keycode];
		}

		public void CreateConversionArray (KeyboardLayout layout)
		{

			XEvent e2 = new XEvent ();
			int keysym = 0;
			int [] ckey = new int [] { 0, 0, 0, 0 };

			VirtualKeys oem_key = VirtualKeys.VK_OEM_7;
			
			e2.KeyEvent.display = display;
			e2.KeyEvent.state = 0;

			int oem_vkey = (int) VirtualKeys.VK_OEM_7;
			for (int keyc = min_keycode; keyc <= max_keycode; keyc++) {
				int vkey = 0;
				int scan = 0;

				e2.KeyEvent.keycode = keyc;
				XKeySym t;
				XLookupString (ref e2, IntPtr.Zero, 0, out t, IntPtr.Zero);
				keysym = (int) t;
				if (keysym != 0) {
					if ((keysym >> 8) == 0xFF) {
						vkey = KeyboardLayouts.nonchar_key_vkey [keysym & 0xFF];
						scan = KeyboardLayouts.nonchar_key_scan [keysym & 0xFF];
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
						int ok;
						
						for (int i = 0; i < syms; i++) {
							keysym = (int) XKeycodeToKeysym (display, keyc, i);
							if ((keysym < 0x800) && (keysym != ' '))
								ckey [i] = keysym & 0xFF;
							else
								ckey [i] = MapDeadKeySym (keysym);
						}
						
						for (int keyn = 0; keyn < layout.Key.Length; keyn++) {
							int i = 0;
							int ml = (layout.Key [keyn].Length > 4 ? 4 : layout.Key [keyn].Length);
							for (ok = layout.Key [keyn][i]; (ok != 0) && (i < ml); i++) {
								if (layout.Key [keyn][i] != ckey [i])
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
							scan = layout.Scan [maxval];
							vkey = (int) layout.VKey [maxval];
						}
						
					}

					for (int i = 0; (i < keysyms_per_keycode) && (vkey == 0); i++) {
						keysym = (int) XLookupKeysym (ref e2, i);
						if ((keysym >= (int) VirtualKeys.VK_0 && keysym <= (int) VirtualKeys.VK_9) ||
								(keysym >= (int) VirtualKeys.VK_A && keysym <= (int) VirtualKeys.VK_Z)) {
							vkey = keysym;
						}
					}

					for (int i = 0; (i < keysyms_per_keycode) && (vkey != 0); i++) {
						keysym = (int) XLookupKeysym (ref e2, i);
						switch ((char) keysym) {
						case ';':
							vkey = (int) VirtualKeys.VK_OEM_1;
							break;
						case '/':
							vkey = (int) VirtualKeys.VK_OEM_2;
							break;
						case '`':
							vkey = (int) VirtualKeys.VK_OEM_3;
							break;
						case '[':
							vkey = (int) VirtualKeys.VK_OEM_4;
							break;
						case '\\':
							vkey = (int) VirtualKeys.VK_OEM_5;
							break;
						case ']':
							vkey = (int) VirtualKeys.VK_OEM_6;
							break;
						case '\'':
							vkey = (int) VirtualKeys.VK_OEM_7;
							break;
						case ',':
							vkey = (int) VirtualKeys.VK_OEM_COMMA;
							break;
						case '.':
							vkey = (int) VirtualKeys.VK_OEM_PERIOD;
							break;
						case '-':
							vkey = (int) VirtualKeys.VK_OEM_MINUS;
							break;
						case '+':
							vkey = (int) VirtualKeys.VK_OEM_PLUS;
							break;

						}
					}

					if (vkey == 0) {
						switch (++oem_vkey) {
						case 0xc1:
							oem_vkey = 0xDB;
							break;
						case 0xE5:
							oem_vkey = 0xE9;
							break;
						case 0xF6:
							oem_vkey = 0xF5;
							break;
						}
						vkey = oem_vkey;
					}
				}
				keyc2vkey [e2.KeyEvent.keycode] = vkey;
				keyc2scan [e2.KeyEvent.keycode] = scan;
			}
			
			
		}

		public void DetectLayout ()
		{
			XDisplayKeycodes (display, out min_keycode, out max_keycode);

			IntPtr ksp = XGetKeyboardMapping (display, (byte) min_keycode,
					max_keycode + 1 - min_keycode, out keysyms_per_keycode);
			XplatUIX11.XFree (ksp);

			syms = keysyms_per_keycode;
			if (syms > 4) {
				Console.Error.WriteLine ("{0} keysymbols per a keycode is not supported, setting to 4", syms);
				syms = 2;
			}

			int [] ckey = new int [4];
			bool vk_set = false;
			KeyboardLayout layout = null;
			int max_score = 0;
			int max_seq = 0;
			bool ismatch = false;
			
			foreach (KeyboardLayout current in KeyboardLayouts.Layouts) {
				Console.WriteLine ("testing layout: {0}", current.Comment);
				int ind = 0;
				int ok = 0;
				int score = 0;
				int match = 0;
				int seq = 0;
				int pkey = -1;
				bool mismatch = false;
				int key = min_keycode;

				for (int keyc = min_keycode; keyc <= max_keycode; keyc++) {
					for (int i = 0; i < syms; i++) {
						int keysym = (int) XKeycodeToKeysym (display, keyc, i);
						
						if ((keysym != 0xFF1B) && (keysym < 0x800) && (keysym != ' ')) {
							ckey [i] = keysym & 0xFF;
						} else {
							ckey [i] = MapDeadKeySym (keysym);
						}
					}
					if (ckey [0] != 0) {

						for (key = 0; key < current.Key.Length; key++) {
							ok = 0;
							int ml = (current.Key [key].Length > syms ? syms : current.Key [key].Length);
							for (int i = 0; (ok >= 0) && (i < ml); i++) {
								if (ckey [i] != 0 && current.Key [key][i] == (char) ckey [i]) {
									ok++;
								}
								if (ckey [i] != 0 && current.Key [key][i] != (char) ckey [i])
									ok = -1;
							}
							if (ok >= 0) {
								score += ok;
								break;
							}
						}
						if (ok > 0) {
							match++;
							if (key > pkey)
								seq++;
							pkey = key;
						} else {
							mismatch = true;
							score -= syms;
						}
					}
				}

				if ((score > max_score) || ((score == max_score) && (seq > max_seq))) {
					// best match so far
					layout = current;
					max_score = score;
					max_seq = seq;
					ismatch = !mismatch;
				}
			}

			if (layout != null) 
				Console.WriteLine ("done detecting keyboard:  " + layout.Comment);
			else
				Console.WriteLine ("no keyboard detected");
			this.layout = layout;
		}

		// TODO
		private int MapDeadKeySym (int val)
		{
			return 0;
		}

		[DllImport ("libX11")]
		internal extern static int XLookupString(ref XEvent xevent, IntPtr buffer,
				int num_bytes, out XKeySym keysym, IntPtr status);

		[DllImport ("libX11")]
		private static extern XKeySym XLookupKeysym (ref XEvent xevent, int index);

		[DllImport ("libX11")]
		private static extern IntPtr XGetKeyboardMapping (IntPtr display, byte first_keycode, int keycode_count, 
				out int keysyms_per_keycode_return);

		[DllImport ("libX11")]
		private static extern XModifierKeymap XGetModifierMapping (IntPtr display);

		[DllImport ("libX11")]
		private static extern void XDisplayKeycodes (IntPtr display, out int min, out int max);

		[DllImport ("libX11")]
		private static extern XKeySym XKeycodeToKeysym (IntPtr display, int keycode, int index);

		[DllImport ("libX11")]
		private static extern int XKeysymToKeycode (IntPtr display, int keysym);

	}

}

