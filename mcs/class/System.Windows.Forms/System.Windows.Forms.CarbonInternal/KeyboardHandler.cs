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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//  Geoff Norton (gnorton@novell.com)
//
//
using System;
using System.Collections;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.CarbonInternal {
	internal class KeyboardHandler : EventHandlerBase, IEventHandler {
		internal const uint kEventRawKeyDown = 1;
		internal const uint kEventRawKeyRepeat = 2;
		internal const uint kEventRawKeyUp = 3;
		internal const uint kEventRawKeyModifiersChanged = 4;
		internal const uint kEventHotKeyPressed = 5;
		internal const uint kEventHotKeyReleased = 6;

		internal const uint kEventParamKeyMacCharCodes = 1801676914;
		internal const uint kEventParamKeyCode = 1801678692;
		internal const uint kEventParamKeyModifiers = 1802334052;
		internal const uint kEventTextInputUnicodeForKeyEvent = 2;
		internal const uint kEventParamTextInputSendText = 1953723512;

		internal const uint typeChar = 1413830740;
		internal const uint typeUInt32 = 1835100014;
		internal const uint typeUnicodeText = 1970567284;

		internal static byte [] key_filter_table;
		internal static byte [] key_modifier_table;
		internal static byte [] key_translation_table;
		internal static byte [] char_translation_table;

		internal static bool translate_modifier = false;

		internal string ComposedString;

		static KeyboardHandler () {
			// our key filter table is a 256 byte array - if the corresponding byte 
			// is set the key should be filtered from WM_CHAR (apple pushes unicode events
			// for some keys which win32 only handles as KEYDOWN
			// currently filtered:
			//	fn+f* == 16
			//	left == 28
			// 	right == 29
			// 	up == 30
			//	down == 31
			// Please update this list as well as the table as more keys are found
			key_filter_table = new byte [256] {
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
							};

			// our char translation table is a set of translations from mac char codes
			// to win32 vkey codes
			// most things map directly
			char_translation_table = new byte [256] {
0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 
16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 0x25, 0x27, 0x26, 0x28, 
32, 49, 34, 51, 52, 53, 55, 222, 57, 48, 56, 187, 188, 189, 190, 191, 
48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 186, 60, 61, 62, 63, 
50, 65, 66, 67, 68, 187, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 
80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 219, 220, 221, 54, 189, 
192, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 
80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 123, 124, 125, 126, 0x2e, 
128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 
144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 
160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 
176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 
192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 
208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 
224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 
240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255
							};
			key_translation_table = new byte [256] {
0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 
16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 
32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 
48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 
64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 
80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 
0x74, 0x75, 0x76, 0x72, 0x77, 0x78, 0x79, 103, 104, 105, 106, 107, 108, 109, 0x7a, 0x7b, 
112, 113, 114, 115, 116, 117, 0x73, 119, 0x71, 121, 0x70, 123, 124, 125, 126, 127, 
128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 
144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 
160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 
176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 
192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 
208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 
224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 
240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255 
							};
			// the key modifier table is a state table of the possible modifier keys
			// apple currently only goes up to 1 << 14 keys, we've extended this to 32
			// bytes as thats the size that apple uses
			key_modifier_table = new byte [32];
		}

		internal KeyboardHandler (XplatUICarbon driver) : base (driver) {}

		private void ModifierToVirtualKey (int i, ref MSG msg, bool down) {
			msg.hwnd = XplatUICarbon.FocusWindow;

			if (i == 9 || i == 13) {
				msg.message = (down ? Msg.WM_KEYDOWN : Msg.WM_KEYUP);
				msg.wParam = (IntPtr) VirtualKeys.VK_SHIFT;
				msg.lParam = IntPtr.Zero;
				return;
			}
			if (i == 12 || i == 14) {
				msg.message = (down ? Msg.WM_KEYDOWN : Msg.WM_KEYUP);
				msg.wParam = (IntPtr) VirtualKeys.VK_CONTROL;
				msg.lParam = IntPtr.Zero;
				return;
			}
			if (i == 8) {
				msg.message = (down ? Msg.WM_SYSKEYDOWN : Msg.WM_SYSKEYUP);
				msg.wParam = (IntPtr) VirtualKeys.VK_MENU;
				msg.lParam = new IntPtr (0x20000000);
				return;
			}
			
			return;
		}

		public void ProcessModifiers (IntPtr eventref, ref MSG msg) {
			// we get notified when modifiers change, but not specifically what changed
			UInt32 modifiers = 0;

			GetEventParameter (eventref, kEventParamKeyModifiers, typeUInt32, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (UInt32)), IntPtr.Zero, ref modifiers);

			for (int i = 0; i < 32; i++) {
				if (key_modifier_table [i] == 0x01 && (modifiers & (1 << i)) == 0) {
					ModifierToVirtualKey (i, ref msg, false);
					key_modifier_table [i] = 0x00;
					return;
				} else if (key_modifier_table [i] == 0x00 && (modifiers & (1 << i)) == (1 << i)) {
					ModifierToVirtualKey (i, ref msg, true);
					key_modifier_table [i] = 0x01;
					return;
				}
			}

			return;
		}

		public void ProcessText (IntPtr eventref, ref MSG msg) {
			UInt32 size = 0;
			IntPtr buffer = IntPtr.Zero;
			byte [] bdata;

			// get the size of the unicode buffer
			GetEventParameter (eventref, kEventParamTextInputSendText, typeUnicodeText, IntPtr.Zero, 0, ref size, IntPtr.Zero);

			buffer = Marshal.AllocHGlobal ((int) size);
			bdata = new byte [size];

			// get the actual text buffer
			GetEventParameter (eventref, kEventParamTextInputSendText, typeUnicodeText, IntPtr.Zero, size, IntPtr.Zero, buffer);

			Marshal.Copy (buffer, bdata, 0, (int) size);
			Marshal.FreeHGlobal (buffer);

			if (key_filter_table [bdata [0]] == 0x00) {
				if (size == 1) {
					msg.message = Msg.WM_CHAR;
					msg.wParam = BitConverter.IsLittleEndian ? (IntPtr) bdata [0] : (IntPtr) bdata [size-1];
					msg.lParam = IntPtr.Zero;
					msg.hwnd = XplatUICarbon.FocusWindow;
				} else {
					msg.message = Msg.WM_IME_COMPOSITION;
					Encoding enc = BitConverter.IsLittleEndian ? Encoding.Unicode : Encoding.BigEndianUnicode;
					ComposedString = enc.GetString (bdata);
					msg.hwnd = XplatUICarbon.FocusWindow;
				}
			}
		}

		public void ProcessKeyPress (IntPtr eventref, ref MSG msg) {
			byte charCode = 0x0;
			byte keyCode = 0x0;

			GetEventParameter (eventref, kEventParamKeyMacCharCodes, typeChar, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (byte)), IntPtr.Zero, ref charCode);
			GetEventParameter (eventref, kEventParamKeyCode, typeUInt32, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (byte)), IntPtr.Zero, ref keyCode);

			msg.lParam = (IntPtr) charCode;
			msg.wParam = charCode == 0x10 ? (IntPtr) key_translation_table [keyCode] : (IntPtr) char_translation_table [charCode];
			msg.hwnd = XplatUICarbon.FocusWindow;
		}

		public bool ProcessEvent (IntPtr callref, IntPtr eventref, IntPtr handle, uint kind, ref MSG msg) {
                        uint klass = EventHandler.GetEventClass (eventref);
			bool result = true;

			if (klass == EventHandler.kEventClassTextInput) {
				switch (kind) {
					case kEventTextInputUnicodeForKeyEvent:
						ProcessText (eventref, ref msg);
						break;
					default:
						Console.WriteLine ("WARNING: KeyboardHandler.ProcessEvent default handler for kEventClassTextInput should not be reached");
						break;
				}
			} else if (klass == EventHandler.kEventClassKeyboard) {
				switch (kind) {
					case kEventRawKeyDown:
					case kEventRawKeyRepeat:
						msg.message = Msg.WM_KEYDOWN;
						ProcessKeyPress (eventref, ref msg);
						break;
					case kEventRawKeyUp:
						msg.message = Msg.WM_KEYUP;
						ProcessKeyPress (eventref, ref msg);
						break;
					case kEventRawKeyModifiersChanged:
						ProcessModifiers (eventref, ref msg);
						break;
					default:
						Console.WriteLine ("WARNING: KeyboardHandler.ProcessEvent default handler for kEventClassKeyboard should not be reached");
						break;
				}
			} else {
				Console.WriteLine ("WARNING: KeyboardHandler.ProcessEvent default handler for kEventClassTextInput should not be reached");
			}

			return result;
		}

		public bool TranslateMessage (ref MSG msg) {
			bool res = false;
 
			if (msg.message >= Msg.WM_KEYFIRST && msg.message <= Msg.WM_KEYLAST)
				res = true;
			
			if (msg.message != Msg.WM_KEYDOWN && msg.message != Msg.WM_SYSKEYDOWN && msg.message != Msg.WM_KEYUP && msg.message != Msg.WM_SYSKEYUP && msg.message != Msg.WM_CHAR && msg.message != Msg.WM_SYSCHAR) 
				return res;

			if (key_modifier_table [8] == 0x01 && key_modifier_table [12] == 0x00 && key_modifier_table [14] == 0x00) {
				if (msg.message == Msg.WM_KEYDOWN) {
					msg.message = Msg.WM_SYSKEYDOWN;
				} else if (msg.message == Msg.WM_CHAR) {
					msg.message = Msg.WM_SYSCHAR;
					translate_modifier = true;
				} else if (msg.message == Msg.WM_KEYUP) {
					msg.message = Msg.WM_SYSKEYUP;
				} else {
					return res;
				}

				msg.lParam = new IntPtr (0x20000000);
			} else if (msg.message == Msg.WM_SYSKEYUP && translate_modifier && msg.wParam == (IntPtr) 18) {
				msg.message = Msg.WM_KEYUP;
				
				msg.lParam = IntPtr.Zero;
				translate_modifier = false;
			}

			return res;
		}

		internal Keys ModifierKeys {
			get {
				Keys keys = Keys.None;
				if (key_modifier_table [9] == 0x01 || key_modifier_table [13] == 0x01) { keys |= Keys.Shift; }
				if (key_modifier_table [8] == 0x01) { keys |= Keys.Alt; }
				if (key_modifier_table [12] == 0x01 || key_modifier_table [14] == 0x01) { keys |= Keys.Control; }
				return keys;
			}
		}

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, ref UInt32 outsize, IntPtr data);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, IntPtr data);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, ref byte data);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, ref UInt32 data);
	}

	internal enum KeyboardModifiers : uint {
		activeFlag = 1 << 0,
		btnState = 1 << 7,
		cmdKey = 1 << 8,
		shiftKey = 1 << 9,
		alphaLock = 1 << 10,
		optionKey = 1 << 11,
		controlKey = 1 << 12,
		rightShiftKey = 1 << 13,
		rightOptionKey = 1 << 14,
		rightControlKey = 1 << 14,
	}
}
