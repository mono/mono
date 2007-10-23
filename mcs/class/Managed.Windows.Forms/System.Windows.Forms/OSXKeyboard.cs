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

namespace System.Windows.Forms {
	internal class OSXKeyboard {
		UInt32 modifiers = 0;

		public OSXKeyboard () {
		}

		private bool ModifiersToVirtualKeys (UInt32 new_modifiers, ref MSG msg, out int vkey) {
			// TODO: X11 appears to only generate VK_SHIFT rather than VK_LSHIFT/VK_RSHIFT
			if ((new_modifiers & (uint)OSXKeyboardModifiers.shiftKey) == (uint)OSXKeyboardModifiers.shiftKey && ((modifiers & (uint)OSXKeyboardModifiers.shiftKey) == 0)) {
				msg.message = Msg.WM_KEYDOWN;
				vkey = (int) VirtualKeys.VK_SHIFT;
			} else if ((new_modifiers & (uint)OSXKeyboardModifiers.shiftKey) == 0 && ((modifiers & (uint)OSXKeyboardModifiers.shiftKey) == (uint)OSXKeyboardModifiers.shiftKey)) {
				msg.message = Msg.WM_KEYUP;
				vkey = (int) VirtualKeys.VK_SHIFT;
			} else if ((new_modifiers & (uint)OSXKeyboardModifiers.rightShiftKey) == (uint)OSXKeyboardModifiers.rightShiftKey && ((modifiers & (uint)OSXKeyboardModifiers.rightShiftKey) == 0)) {
				msg.message = Msg.WM_KEYDOWN;
				vkey = (int) VirtualKeys.VK_SHIFT;
			} else if ((new_modifiers & (uint)OSXKeyboardModifiers.rightShiftKey) == 0 && ((modifiers & (uint)OSXKeyboardModifiers.rightShiftKey) == (uint)OSXKeyboardModifiers.rightShiftKey)) {
				msg.message = Msg.WM_KEYUP;
				vkey = (int) VirtualKeys.VK_SHIFT;
			} else if ((new_modifiers & (uint)OSXKeyboardModifiers.cmdKey) == (uint)OSXKeyboardModifiers.cmdKey && ((modifiers & (uint)OSXKeyboardModifiers.cmdKey) == 0)) {
				msg.message = Msg.WM_KEYDOWN;
				vkey = (int) VirtualKeys.VK_LWIN;
			} else if ((new_modifiers & (uint)OSXKeyboardModifiers.cmdKey) == 0 && ((modifiers & (uint)OSXKeyboardModifiers.cmdKey) == (uint)OSXKeyboardModifiers.cmdKey)) {
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
			
			return true;
		}


		public bool KeyEvent (IntPtr inEvent, IntPtr handle, uint eventKind, ref MSG msg) {
			int vkey = -1;
			bool result = true;
			byte charCode = 0x0;

			switch (eventKind) {
				case OSXConstants.kEventRawKeyDown: {
					msg.message = Msg.WM_KEYDOWN;
					GetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamKeyMacCharCodes, OSXConstants.EventParamType.typeChar, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (byte)), IntPtr.Zero, ref charCode);
					result = CharCodeToVirtualKeys (charCode, out vkey);
					break;
				}
				case OSXConstants.kEventRawKeyUp: {
					msg.message = Msg.WM_KEYUP;
					GetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamKeyMacCharCodes, OSXConstants.EventParamType.typeChar, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (byte)), IntPtr.Zero, ref charCode);
					result = CharCodeToVirtualKeys (charCode, out vkey);
					break;
				}
				case OSXConstants.kEventRawKeyRepeat: {
					msg.message = Msg.WM_KEYDOWN;
					GetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamKeyMacCharCodes, OSXConstants.EventParamType.typeChar, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (byte)), IntPtr.Zero, ref charCode);
					result = CharCodeToVirtualKeys (charCode, out vkey);
					break;
				}
				case OSXConstants.kEventRawKeyModifiersChanged: {
					UInt32 new_modifiers = 0;
					GetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamKeyModifiers, OSXConstants.EventParamType.typeUInt32, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (UInt32)), IntPtr.Zero, ref new_modifiers);
					result = ModifiersToVirtualKeys (new_modifiers, ref msg, out vkey);
					modifiers = new_modifiers;
					break;
				}
			}
			if (result) {
				msg.lParam = IntPtr.Zero;
				msg.wParam = (IntPtr) vkey;
            	GetKeyboardFocus (handle, ref msg.hwnd);
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
		static extern int GetEventParameter (IntPtr evt, OSXConstants.EventParamName inName, OSXConstants.EventParamType inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref byte outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr evt, OSXConstants.EventParamName inName, OSXConstants.EventParamType inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref UInt32 outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetKeyboardFocus (IntPtr handle, ref IntPtr cntrl);
	}

	internal enum OSXKeyboardModifiers : uint {
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
