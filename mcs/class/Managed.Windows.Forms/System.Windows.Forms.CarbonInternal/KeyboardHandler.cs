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
		internal const uint kEventParamKeyModifiers = 1802334052;

		internal const uint typeChar = 1413830740;
		internal const uint typeUInt32 = 1835100014;

		UInt32 modifiers = 0;

		internal KeyboardHandler (XplatUICarbon driver) : base (driver) {}

		private bool ModifiersToVirtualKeys (UInt32 new_modifiers, ref MSG msg, out int vkey) {
			// TODO: X11 appears to only generate VK_SHIFT rather than VK_LSHIFT/VK_RSHIFT
			if ((new_modifiers & (uint)KeyboardModifiers.shiftKey) == (uint)KeyboardModifiers.shiftKey && ((modifiers & (uint)KeyboardModifiers.shiftKey) == 0)) {
				msg.message = Msg.WM_KEYDOWN;
				vkey = (int) VirtualKeys.VK_SHIFT;
			} else if ((new_modifiers & (uint)KeyboardModifiers.shiftKey) == 0 && ((modifiers & (uint)KeyboardModifiers.shiftKey) == (uint)KeyboardModifiers.shiftKey)) {
				msg.message = Msg.WM_KEYUP;
				vkey = (int) VirtualKeys.VK_SHIFT;
			} else if ((new_modifiers & (uint)KeyboardModifiers.rightShiftKey) == (uint)KeyboardModifiers.rightShiftKey && ((modifiers & (uint)KeyboardModifiers.rightShiftKey) == 0)) {
				msg.message = Msg.WM_KEYDOWN;
				vkey = (int) VirtualKeys.VK_SHIFT;
			} else if ((new_modifiers & (uint)KeyboardModifiers.rightShiftKey) == 0 && ((modifiers & (uint)KeyboardModifiers.rightShiftKey) == (uint)KeyboardModifiers.rightShiftKey)) {
				msg.message = Msg.WM_KEYUP;
				vkey = (int) VirtualKeys.VK_SHIFT;
			} else if ((new_modifiers & (uint)KeyboardModifiers.cmdKey) == (uint)KeyboardModifiers.cmdKey && ((modifiers & (uint)KeyboardModifiers.cmdKey) == 0)) {
				msg.message = Msg.WM_KEYDOWN;
				vkey = (int) VirtualKeys.VK_LWIN;
			} else if ((new_modifiers & (uint)KeyboardModifiers.cmdKey) == 0 && ((modifiers & (uint)KeyboardModifiers.cmdKey) == (uint)KeyboardModifiers.cmdKey)) {
				msg.message = Msg.WM_KEYUP;
				vkey = (int) VirtualKeys.VK_LWIN;
			} else {
				vkey = -1;
				return false;
			}
			return true;
		}

		private bool CharCodeToVirtualKeys (byte charCode, out int vkey) {
			vkey = (int) charCode;
			switch (vkey) {
				case 28:
					vkey = (int) VirtualKeys.VK_LEFT;
					break;
				case 29:
					vkey = (int) VirtualKeys.VK_RIGHT;
					break;
				case 30:
					vkey = (int) VirtualKeys.VK_UP;
					break;
				case 31:
					vkey = (int) VirtualKeys.VK_DOWN;
					break;
			}	
			
			return true;
		}


		public bool ProcessEvent (IntPtr eventref, IntPtr handle, uint kind, ref MSG msg) {
			int vkey = -1;
			bool result = true;
			byte charCode = 0x0;
			UInt32 new_modifiers = 0;

			GetEventParameter (eventref, kEventParamKeyMacCharCodes, typeChar, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (byte)), IntPtr.Zero, ref charCode);
			GetEventParameter (eventref, kEventParamKeyModifiers, typeUInt32, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (UInt32)), IntPtr.Zero, ref new_modifiers);

			switch (kind) {
				case kEventRawKeyDown:
				case kEventRawKeyRepeat:
					msg.message = Msg.WM_KEYDOWN;
					result = CharCodeToVirtualKeys (charCode, out vkey);
					break;
				case kEventRawKeyUp: 
					msg.message = Msg.WM_KEYUP;
					result = CharCodeToVirtualKeys (charCode, out vkey);
					break;
				case kEventRawKeyModifiersChanged: 
					result = ModifiersToVirtualKeys (new_modifiers, ref msg, out vkey);
					modifiers = new_modifiers;
					break;
			}
			msg.wParam = (IntPtr) vkey;

			if (result) {
				msg.hwnd = XplatUICarbon.FocusWindow;
			}
			
			return result;
		}
		
		internal bool TranslateMessage (ref MSG msg)
		{
			bool res = false;

			if (msg.message >= Msg.WM_KEYFIRST && msg.message <= Msg.WM_KEYLAST)
				res = true;

			if (msg.message != Msg.WM_KEYDOWN && msg.message != Msg.WM_SYSKEYDOWN)
				return res;

			Msg message = (msg.message == Msg.WM_KEYDOWN ? Msg.WM_CHAR : Msg.WM_SYSCHAR);
			XplatUI.PostMessage (msg.hwnd, message, msg.wParam, msg.lParam);
			
			return res;
		}

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
