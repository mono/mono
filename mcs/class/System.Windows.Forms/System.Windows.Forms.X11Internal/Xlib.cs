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
using System.Runtime.InteropServices;

namespace System.Windows.Forms.X11Internal {

	internal class Xlib {
		const string libX11 = "X11";

		[DllImport (libX11)]
		public extern static IntPtr XOpenDisplay(IntPtr display);

		[DllImport (libX11)]
		public extern static int XCloseDisplay(IntPtr display);

		[DllImport (libX11)]
		public extern static IntPtr XSynchronize(IntPtr display, bool onoff);

		[DllImport (libX11)]
		public extern static IntPtr XCreateWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int depth, int xclass, IntPtr visual, UIntPtr valuemask, ref XSetWindowAttributes attributes);

		[DllImport (libX11)]
		public extern static IntPtr XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, UIntPtr border, UIntPtr background);

		[DllImport (libX11)]
		public extern static int XMapWindow(IntPtr display, IntPtr window);

		[DllImport (libX11)]
		public extern static int XUnmapWindow(IntPtr display, IntPtr window);

		[DllImport (libX11)]
		public extern static int XMapSubindows(IntPtr display, IntPtr window);

		[DllImport (libX11)]
		public extern static int XUnmapSubwindows(IntPtr display, IntPtr window);

		[DllImport (libX11)]
		public extern static IntPtr XRootWindow(IntPtr display, int screen_number);

		[DllImport (libX11)]
		public extern static IntPtr XNextEvent(IntPtr display, ref XEvent xevent);

		[DllImport (libX11)]
		public extern static int XConnectionNumber (IntPtr diplay);

		[DllImport (libX11)]
		public extern static int XPending (IntPtr diplay);

		[DllImport (libX11)]
		public extern static IntPtr XSelectInput(IntPtr display, IntPtr window, IntPtr mask);

		[DllImport (libX11)]
		public extern static int XDestroyWindow(IntPtr display, IntPtr window);

		[DllImport (libX11)]
		public extern static int XReparentWindow(IntPtr display, IntPtr window, IntPtr parent, int x, int y);

		[DllImport (libX11)]
		public extern static int XMoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height);

		[DllImport (libX11)]
		public extern static int XResizeWindow(IntPtr display, IntPtr window, int width, int height);

		[DllImport (libX11)]
		public extern static int XGetWindowAttributes(IntPtr display, IntPtr window, ref XWindowAttributes attributes);

		[DllImport (libX11)]
		public extern static int XFlush(IntPtr display);

		[DllImport (libX11)]
		public extern static int XSetWMName(IntPtr display, IntPtr window, ref XTextProperty text_prop);

		[DllImport (libX11)]
		public extern static int XStoreName(IntPtr display, IntPtr window, string window_name);

		[DllImport (libX11)]
		public extern static int XFetchName(IntPtr display, IntPtr window, ref IntPtr window_name);

		[DllImport (libX11)]
		public extern static int XSendEvent(IntPtr display, IntPtr window, bool propagate, IntPtr event_mask, ref XEvent send_event);

		[DllImport (libX11)]
		public extern static int XQueryTree(IntPtr display, IntPtr window, out IntPtr root_return, out IntPtr parent_return, out IntPtr children_return, out int nchildren_return);

		[DllImport (libX11)]
		public extern static int XFree(IntPtr data);

		[DllImport (libX11)]
		public extern static int XRaiseWindow(IntPtr display, IntPtr window);

		[DllImport (libX11)]
		public extern static uint XLowerWindow(IntPtr display, IntPtr window);

		[DllImport (libX11)]
		public extern static uint XConfigureWindow(IntPtr display, IntPtr window, ChangeWindowFlags value_mask, ref XWindowChanges values);

		[DllImport (libX11)]
		public extern static IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

		[DllImport (libX11)]
		public extern static int XInternAtoms(IntPtr display, string[] atom_names, int atom_count, bool only_if_exists, IntPtr[] atoms);

		[DllImport (libX11)]
		public extern static int XSetWMProtocols(IntPtr display, IntPtr window, IntPtr[] protocols, int count);

		[DllImport (libX11)]
		public extern static int XGrabPointer(IntPtr display, IntPtr window, bool owner_events, EventMask event_mask, GrabMode pointer_mode, GrabMode keyboard_mode, IntPtr confine_to, IntPtr cursor, IntPtr timestamp);

		[DllImport (libX11)]
		public extern static int XUngrabPointer(IntPtr display, IntPtr timestamp);

		[DllImport (libX11)]
		public extern static bool XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child, out int root_x, out int root_y, out int win_x, out int win_y, out int keys_buttons);

		[DllImport (libX11)]
		public extern static bool XTranslateCoordinates (IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, out int intdest_x_return,	 out int dest_y_return, out IntPtr child_return);

		[DllImport (libX11)]
		public extern static bool XGetGeometry(IntPtr display, IntPtr window, out IntPtr root, out int x, out int y, out int width, out int height, out int border_width, out int depth);

		[DllImport (libX11)]
		public extern static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, out int x, out int y, out int width, out int height, IntPtr border_width, IntPtr depth);

		[DllImport (libX11)]
		public extern static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, out int x, out int y, IntPtr width, IntPtr height, IntPtr border_width, IntPtr depth);

		[DllImport (libX11)]
		public extern static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, IntPtr x, IntPtr y, out int width, out int height, IntPtr border_width, IntPtr depth);

		[DllImport (libX11)]
		public extern static uint XWarpPointer(IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);

		[DllImport (libX11)]
		public extern static int XClearWindow(IntPtr display, IntPtr window);

		[DllImport (libX11)]
		public extern static int XClearArea(IntPtr display, IntPtr window, int x, int y, int width, int height, bool exposures);

		// Colormaps
		[DllImport (libX11)]
		public extern static IntPtr XDefaultScreenOfDisplay(IntPtr display);

		[DllImport (libX11)]
		public extern static int XScreenNumberOfScreen(IntPtr display, IntPtr Screen);

		[DllImport (libX11)]
		public extern static IntPtr XDefaultVisual(IntPtr display, int screen_number);

		[DllImport (libX11)]
		public extern static uint XDefaultDepth(IntPtr display, int screen_number);

		[DllImport (libX11)]
		public extern static int XDefaultScreen(IntPtr display);

		[DllImport (libX11)]
		public extern static IntPtr XDefaultColormap(IntPtr display, int screen_number);

		[DllImport (libX11)]
		public extern static int XLookupColor(IntPtr display, IntPtr Colormap, string Coloranem, ref XColor exact_def_color, ref XColor screen_def_color);

		[DllImport (libX11)]
		public extern static int XAllocColor(IntPtr display, IntPtr Colormap, ref XColor colorcell_def);

		[DllImport (libX11)]
		public extern static int XSetTransientForHint(IntPtr display, IntPtr window, IntPtr prop_window);

		[DllImport (libX11)]
		public extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref MotifWmHints data, int nelements);

		[DllImport (libX11)]
		public extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref uint value, int nelements);

		[DllImport (libX11)]
		public extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref IntPtr value, int nelements);

		[DllImport (libX11)]
		public extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, uint[] data, int nelements);

		[DllImport (libX11)]
		public extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, int[] data, int nelements);

		[DllImport (libX11)]
		public extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, IntPtr[] data, int nelements);

		[DllImport (libX11)]
		public extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, IntPtr atoms, int nelements);

		[DllImport (libX11, CharSet=CharSet.Ansi)]
		public extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, string text, int text_length);

		[DllImport (libX11)]
		public extern static int XDeleteProperty(IntPtr display, IntPtr window, IntPtr property);

		// Drawing
		[DllImport (libX11)]
		public extern static IntPtr XCreateGC(IntPtr display, IntPtr window, IntPtr valuemask, ref XGCValues values);

		[DllImport (libX11)]
		public extern static int XFreeGC(IntPtr display, IntPtr gc);

		[DllImport (libX11)]
		public extern static int XSetFunction(IntPtr display, IntPtr gc, GXFunction function);

		[DllImport (libX11)]
		internal extern static int XSetLineAttributes(IntPtr display, IntPtr gc, int line_width, GCLineStyle line_style, GCCapStyle cap_style, GCJoinStyle join_style);

		[DllImport (libX11)]
		public extern static int XDrawLine(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int x2, int y2);

		[DllImport (libX11)]
		public extern static int XDrawRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int width, int height);

		[DllImport (libX11)]
		public extern static int XFillRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int width, int height);

		[DllImport (libX11)]
		public extern static int XSetWindowBackground(IntPtr display, IntPtr window, IntPtr background);

		[DllImport (libX11)]
		public extern static int XCopyArea(IntPtr display, IntPtr src, IntPtr dest, IntPtr gc, int src_x, int src_y, int width, int height, int dest_x, int dest_y);

		[DllImport (libX11)]
		public extern static int XGetWindowProperty(IntPtr display, IntPtr window, IntPtr atom, IntPtr long_offset, IntPtr long_length, bool delete, IntPtr req_type, out IntPtr actual_type, out int actual_format, out IntPtr nitems, out IntPtr bytes_after, ref IntPtr prop);

		[DllImport (libX11)]
		public extern static int XSetInputFocus(IntPtr display, IntPtr window, RevertTo revert_to, IntPtr time);

		[DllImport (libX11)]
		public extern static int XIconifyWindow(IntPtr display, IntPtr window, int screen_number);

		[DllImport (libX11)]
		public extern static int XDefineCursor(IntPtr display, IntPtr window, IntPtr cursor);

		[DllImport (libX11)]
		public extern static int XUndefineCursor(IntPtr display, IntPtr window);

		[DllImport (libX11)]
		public extern static int XFreeCursor(IntPtr display, IntPtr cursor);

		[DllImport (libX11)]
		public extern static IntPtr XCreateFontCursor(IntPtr display, CursorFontShape shape);

		[DllImport (libX11)]
		public extern static IntPtr XCreatePixmapCursor(IntPtr display, IntPtr source, IntPtr mask, ref XColor foreground_color, ref XColor background_color, int x_hot, int y_hot);

		[DllImport (libX11)]
		public extern static IntPtr XCreatePixmapFromBitmapData(IntPtr display, IntPtr drawable, byte[] data, int width, int height, IntPtr fg, IntPtr bg, int depth);

		[DllImport (libX11)]
		public extern static IntPtr XCreatePixmap(IntPtr display, IntPtr d, int width, int height, int depth);

		[DllImport (libX11)]
		public extern static IntPtr XFreePixmap(IntPtr display, IntPtr pixmap);

		[DllImport (libX11)]
		public extern static int XQueryBestCursor(IntPtr display, IntPtr drawable, int width, int height, out int best_width, out int best_height);

		[DllImport (libX11)]
		public extern static IntPtr XWhitePixel(IntPtr display, int screen_no);

		[DllImport (libX11)]
		public extern static IntPtr XBlackPixel(IntPtr display, int screen_no);

		[DllImport (libX11)]
		public extern static void XGrabServer(IntPtr display);

		[DllImport (libX11)]
		public extern static void XUngrabServer(IntPtr display);

		[DllImport (libX11)]
		public extern static void XGetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints, out IntPtr supplied_return);

		[DllImport (libX11)]
		public extern static void XSetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints);

		[DllImport (libX11)]
		public extern static void XSetZoomHints(IntPtr display, IntPtr window, ref XSizeHints hints);

		[DllImport (libX11)]
		public extern static void XSetWMHints(IntPtr display, IntPtr window, ref XWMHints wmhints);

		[DllImport (libX11)]
		public extern static int XGetIconSizes(IntPtr display, IntPtr window, out IntPtr size_list, out int count);

		[DllImport (libX11)]
		public extern static IntPtr XSetErrorHandler(XErrorHandler error_handler);

		[DllImport (libX11)]
		public extern static IntPtr XGetErrorText(IntPtr display, byte code, StringBuilder buffer, int length);

		[DllImport (libX11)]
		public extern static int XInitThreads();

		[DllImport (libX11)]
		public extern static int XConvertSelection(IntPtr display, IntPtr selection, IntPtr target, IntPtr property, IntPtr requestor, IntPtr time);

		[DllImport (libX11)]
		public extern static IntPtr XGetSelectionOwner(IntPtr display, IntPtr selection);

		[DllImport (libX11)]
		public extern static int XSetSelectionOwner(IntPtr display, IntPtr selection, IntPtr owner, IntPtr time);

		[DllImport (libX11)]
		public extern static int XSetPlaneMask(IntPtr display, IntPtr gc, IntPtr mask);

		[DllImport (libX11)]
		public extern static int XSetForeground(IntPtr display, IntPtr gc, UIntPtr foreground);

		[DllImport (libX11)]
		public extern static int XSetBackground(IntPtr display, IntPtr gc, UIntPtr background);

		[DllImport (libX11)]
		public extern static int XBell(IntPtr display, int percent);

		[DllImport (libX11)]
		public extern static int XChangeActivePointerGrab (IntPtr display, EventMask event_mask, IntPtr cursor, IntPtr time);

		[DllImport (libX11)]
		public extern static bool XFilterEvent(ref XEvent xevent, IntPtr window);

		[DllImport (libX11)]
		public extern static void XkbSetDetectableAutoRepeat (IntPtr display, bool detectable, IntPtr supported);

		[DllImport (libX11)]
		public extern static void XPeekEvent (IntPtr display, ref XEvent xevent);
	}
}
