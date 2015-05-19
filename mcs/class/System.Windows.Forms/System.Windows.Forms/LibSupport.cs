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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//

using System.Collections;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	class LibSupport {
		static ArrayList list = new ArrayList ();

		public static void Register ()
		{
			FindWindowExW fw = new FindWindowExW (FindWindow);
			list.Add (fw);
			support_register_delegate ("FindWindowExW", fw);
		}

		static IntPtr FindWindow (IntPtr hWnd)
		{
			NativeWindow nw = NativeWindow.FromHandle (hWnd);
			if (nw == null)
				return IntPtr.Zero;

			return nw.Handle;
		}

		delegate IntPtr FindWindowExW (IntPtr hWnd);

		[DllImport ("MonoSupportW")]
		extern static void support_register_delegate (string fmt, Delegate d);
	}
}

