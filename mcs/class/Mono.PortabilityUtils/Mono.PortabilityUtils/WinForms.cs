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
// Copyright (c) 2019 AxxonSoft.
//
// Authors:
//	Nikita Voronchev <nikita.voronchev@ru.axxonsoft.com>
//

using System;
using System.Windows.Forms;

namespace Mono.PortabilityUtils
{
	public static class WinForms
	{
		public delegate bool EnumWindowsProc (IntPtr hwnd, IntPtr lParam);

		public static int RegisterWindowMessage (string lpString)
		{
			try {
				return (int)XplatUI.RegisterWindowMessage (lpString);
			}
			catch (Exception ex) {
				Console.Error.WriteLine ("WinForms.RegisterWindowMessage fails: {0}", ex);
				return 0;
			}
		}

		/* Uncomment to DEBUG:
		public static void Clear ()
		{
			XplatUIX11.WindowMessagesStorage.Clear ();
		}
		*/

		public static bool PostMessage (IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam)
		{
			try {
				return XplatUI.InterProcessPostMessage (hwnd, message, wParam, lParam);
			}
			catch (Exception ex) {
				Console.Error.WriteLine ("WinForms.PostMessage fails: {0}", ex);
				return false;
			}
		}

		public static bool EnumTopLevelWindows (EnumWindowsProc lpEnumFunc, IntPtr lParam)
		{
			try {
				return XplatUI.EnumTopLevelWindows (new XplatUI.EnumWindowsProc (lpEnumFunc), lParam);
			}
			catch (Exception ex) {
				Console.Error.WriteLine ("WinForms.EnumTopLevelWindows fails: {0}", ex);
				return false;
			}
		}
	}
}

