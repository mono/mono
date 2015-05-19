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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System.Windows.Forms.X11Internal {

	internal class X11RootHwnd : X11Hwnd
	{
		public X11RootHwnd (X11Display display, IntPtr window_handle) : base (display)
		{
			WholeWindow = ClientWindow = window_handle;

			Xlib.XSelectInput(display.Handle, WholeWindow, new IntPtr ((int)EventMask.PropertyChangeMask));
		}

		public override void CreateWindow (CreateParams cp)
		{
			// we don't do anything here
		}

		public override void PropertyChanged (XEvent xevent)
		{
			if (xevent.PropertyEvent.atom == Display.Atoms._NET_ACTIVE_WINDOW) {
				IntPtr actual_atom;
				int actual_format;
				IntPtr nitems;
				IntPtr bytes_after;
				IntPtr prop = IntPtr.Zero;

				Xlib.XGetWindowProperty (Display.Handle, WholeWindow,
							 Display.Atoms._NET_ACTIVE_WINDOW, IntPtr.Zero, new IntPtr (1), false,
							 Display.Atoms.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);

				if (((long)nitems > 0) && (prop != IntPtr.Zero)) {
					// FIXME - is this 64 bit clean?
					Display.SetActiveWindow ((X11Hwnd)Hwnd.ObjectFromHandle((IntPtr)Marshal.ReadInt32(prop)));
					Xlib.XFree(prop);
				}
			}
			else if (xevent.PropertyEvent.atom == Display.Atoms._NET_SUPPORTED) {
				// we'll need to refetch the supported protocols list
				refetch_net_supported = true;
				_net_supported = null;
			}
			else
				base.PropertyChanged (xevent);
		}

		bool refetch_net_supported = true;
		IntPtr[] _net_supported;
		public IntPtr[] _NET_SUPPORTED {
			get {
				if (refetch_net_supported) {
					_net_supported = GetAtomListProperty (Display.Atoms._NET_SUPPORTED);
					refetch_net_supported = false;
				}

				return _net_supported;
			}
		}
	}

}

