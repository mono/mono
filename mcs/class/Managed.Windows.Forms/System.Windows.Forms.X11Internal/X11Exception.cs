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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
//

using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace System.Windows.Forms.X11Internal {

	internal class X11Exception : ApplicationException {
		IntPtr		Display;
		IntPtr		ResourceID;
		IntPtr		Serial;
		XRequest	RequestCode;
		byte		ErrorCode;
		byte		MinorCode;

		public X11Exception (IntPtr Display, IntPtr ResourceID, IntPtr Serial, byte ErrorCode, XRequest RequestCode, byte MinorCode)
		{
			this.Display = Display;
			this.ResourceID = ResourceID;
			this.Serial = Serial;
			this.RequestCode = RequestCode;
			this.ErrorCode = ErrorCode;
			this.MinorCode = MinorCode;
		}

		public override string Message {
			get {
				return GetMessage (Display, ResourceID, Serial, ErrorCode, RequestCode, MinorCode);
			}
		}

		public static string GetMessage (IntPtr Display, IntPtr ResourceID, IntPtr Serial, byte ErrorCode, XRequest RequestCode, byte MinorCode)
		{
			StringBuilder	sb;
			string		x_error_text;
			string		error;
			string		hwnd_text;
			string		control_text;
			Hwnd		hwnd;
			Control		c;

			sb = new StringBuilder(160);
			Xlib.XGetErrorText (Display, ErrorCode, sb, sb.Capacity);
			x_error_text = sb.ToString();
			hwnd = Hwnd.ObjectFromHandle(ResourceID);
			if (hwnd != null) {
				hwnd_text = hwnd.ToString();
				c = Control.FromHandle(hwnd.Handle);
				if (c != null) {
					control_text = c.ToString();
				} else {
					control_text = String.Format("<handle {0:X} non-existant>", hwnd.Handle);
				}
			}
			else {
				hwnd_text = "<null>";
				control_text = "<null>";
			}

			error = String.Format("\n  Error: {0}\n  Request:     {1:D} ({2})\n  Resource ID: 0x{3:X}\n  Serial:      {4}\n  Hwnd:        {5}\n  Control:     {6}", x_error_text, RequestCode, RequestCode, ResourceID.ToInt32(), Serial, hwnd_text, control_text);
			return error;
		}
	}
}
