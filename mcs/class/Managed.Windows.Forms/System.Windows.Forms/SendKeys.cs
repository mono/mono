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
//	Peter Bartok	(pbartok@novell.com)
//
//

// NOT COMPLETE

using System.Collections;

namespace System.Windows.Forms {
	public class SendKeys {
		#region Local variables
		private static Queue	keys = new Queue();
		#endregion

		#region Private methods
		private SendKeys() {
		}

		private static IntPtr CharToVKey(char key) {
			// FIXME - build a table to translate between vkeys and chars
			throw new NotImplementedException();
		}

		private static IntPtr SymbolToVKey(string symbol) {
			// FIXME - build a table to translate between vkeys and symbols
			throw new NotImplementedException();
		}

		private static void SendSymbol(IntPtr hwnd, string symbol, int repeat_count, bool down) {
			MSG	msg;

			if (down) {
				for (int i = 0; i < repeat_count; i++ ) {
					msg = new MSG();
					msg.hwnd = hwnd;
					msg.message = Msg.WM_KEYDOWN;
					msg.wParam = SymbolToVKey(symbol);
					msg.lParam = IntPtr.Zero;

					keys.Enqueue(msg);
				}
			} else {
				msg = new MSG();
				msg.hwnd = hwnd;
				msg.message = Msg.WM_KEYUP;
				msg.wParam = SymbolToVKey(symbol);
				msg.lParam = IntPtr.Zero;

				keys.Enqueue(msg);
			}
		}

		private static void SendKey(IntPtr hwnd, char key, int repeat_count) {
			MSG	msg;

			for (int i = 0; i < repeat_count; i++ ) {
				msg = new MSG();
				msg.hwnd = hwnd;
				msg.message = Msg.WM_KEYDOWN;
				msg.wParam = CharToVKey(key);
				msg.lParam = IntPtr.Zero;

				keys.Enqueue(msg);
			}

			msg = new MSG();
			msg.hwnd = hwnd;
			msg.message = Msg.WM_KEYUP;
			msg.wParam = CharToVKey(key);
			msg.lParam = IntPtr.Zero;

			keys.Enqueue(msg);
		}

		#endregion	// Private Methods

		#region Public Static Methods
		public static void Flush() {
			MSG msg;

			// FIXME - we only send to our own app, instead of the active app
			while (keys.Count > 0) {
				msg = (MSG)keys.Dequeue();
				XplatUI.TranslateMessage (ref msg);
				XplatUI.DispatchMessage (ref msg);
			}
		}

		[MonoTODO("Finish")]
		public static void Send(string key_string) {
			IntPtr	hwnd;
			int	shift_reset;
			int	control_reset;
			int	alt_reset;

			hwnd = XplatUI.GetActive();

			shift_reset = 0;
			control_reset = 0;
			alt_reset = 0;

			for (int i = 0; i < key_string.Length; i++) {
				switch(key_string[i]) {
					case '+': {
						SendSymbol(hwnd, "SHIFT", 1, true);
						shift_reset = 2;
						break;
					}

					case '^': {
						SendSymbol(hwnd, "CONTROL", 1, true);
						control_reset = 2;
						break;
					}

					case '%': {
						SendSymbol(hwnd, "ALT", 1, true);
						alt_reset = 2;
						break;
					}

					case '~': {
						SendSymbol(hwnd, "ENTER", 1, true);
						SendSymbol(hwnd, "ENTER", 1, false);
						break;
					}

					case '(':
					case ')': {
						// FIXME - implement group parser
						break;
					}

					case '{':
					case '}': {
						// FIXME - implement symbol parser
						break;
					}

					default: {
						SendKey(hwnd, key_string[i], 1);
						break;
					}
				}

				

				if (shift_reset > 0) {
					shift_reset--;
					if (shift_reset == 0) {
						SendSymbol(hwnd, "SHIFT", 1, false);
					}
				}

				if (control_reset > 0) {
					control_reset--;
					if (control_reset == 0) {
						SendSymbol(hwnd, "CONTROL", 1, false);
					}
				}

				if (alt_reset > 0) {
					alt_reset--;
					if (alt_reset == 0) {
						SendSymbol(hwnd, "ALT", 1, false);
					}
				}
			}
		}

		public static void SendWait(string keys) {
			Send(keys);
			Flush();
		}
		#endregion	// Public Instance Properties
	}
}
