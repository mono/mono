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
//	Peter Bartok	pbartok@novell.com
//
//
// $Revision: 1.3 $
// $Modtime: $
// $Log: X11Structs.cs,v $
// Revision 1.3  2004/08/06 15:53:39  jordi
// X11 keyboard navigation
//
// Revision 1.2  2004/08/06 14:02:33  pbartok
// - Fixed reparenting
// - Fixed window border creation
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// NOT COMPLETE

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// X11 Version
namespace System.Windows.Forms {
	#region X11 Structures
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XAnyEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XKeyEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		root;
		internal IntPtr		subwindow;
		internal int		time;
		internal int		x;
		internal int		y;
		internal int		x_root;
		internal int		y_root;
		internal int		state;
		internal int		keycode;
		internal bool		same_screen;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XButtonEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		root;
		internal IntPtr		subwindow;
		internal int		time;
		internal int		x;
		internal int		y;
		internal int		x_root;
		internal int		y_root;
		internal int		state;
		internal int		button;
		internal bool		same_screen;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XMotionEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		root;
		internal IntPtr		subwindow;
		internal int		time;
		internal int		x;
		internal int		y;
		internal int		x_root;
		internal int		y_root;
		internal int		state;
		internal byte		is_hint;
		internal bool		same_screen;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XCrossingEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		root;
		internal IntPtr		subwindow;
		internal int		time;
		internal int		x;
		internal int		y;
		internal int		x_root;
		internal int		y_root;
		internal int		mode;
		internal int		detail;
		internal bool		same_screen;
		internal bool		focus;
		internal int		state;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XFocusChangeEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		mode;
		internal int		detail;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XKeymapEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal byte		key_vector0;
		internal byte		key_vector1;
		internal byte		key_vector2;
		internal byte		key_vector3;
		internal byte		key_vector4;
		internal byte		key_vector5;
		internal byte		key_vector6;
		internal byte		key_vector7;
		internal byte		key_vector8;
		internal byte		key_vector9;
		internal byte		key_vector10;
		internal byte		key_vector11;
		internal byte		key_vector12;
		internal byte		key_vector13;
		internal byte		key_vector14;
		internal byte		key_vector15;
		internal byte		key_vector16;
		internal byte		key_vector17;
		internal byte		key_vector18;
		internal byte		key_vector19;
		internal byte		key_vector20;
		internal byte		key_vector21;
		internal byte		key_vector22;
		internal byte		key_vector23;
		internal byte		key_vector24;
		internal byte		key_vector25;
		internal byte		key_vector26;
		internal byte		key_vector27;
		internal byte		key_vector28;
		internal byte		key_vector29;
		internal byte		key_vector30;
		internal byte		key_vector31;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XExposeEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal int		count;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XGraphicsExposeEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		drawable;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal int		count;
		internal int		major_code;
		internal int		minor_code;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XNoExposeEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		drawable;
		internal int		major_code;
		internal int		minor_code;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XVisibilityEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		state;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XCreateWindowEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		parent;
		internal IntPtr		window;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal int		border_width;
		internal bool		override_redirect;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XDestroyWindowEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XUnmapEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal bool		from_configure;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XMapEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal bool		override_redirect;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XMapRequestEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		parent;
		internal IntPtr		window;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XReparentEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal IntPtr		parent;
		internal int		x;
		internal int		y;
		internal bool		override_redirect;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XConfigureEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal int		border_width;
		internal IntPtr		above;
		internal bool		override_redirect;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XGravityEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal int		x;
		internal int		y;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XResizeRequestEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		width;
		internal int		height;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XConfigureRequestEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		width;
		internal int		height;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XCirculateEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal int		place;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XCirculateRequestEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		parent;
		internal IntPtr		window;
		internal int		place;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XPropertyEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		atom;
		internal int		time;
		internal int		state;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XSelectionClearEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		selection;
		internal int		time;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XSelectionRequestEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		owner;
		internal IntPtr		requestor;
		internal IntPtr		selection;
		internal IntPtr		target;
		internal IntPtr		property;
		internal int		time;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XSelectionEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		requestor;
		internal IntPtr		selection;
		internal IntPtr		target;
		internal IntPtr		property;
		internal int		time;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XColormapEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		colormap;
		internal bool		c_new;
		internal int		state;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XClientMessageEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		message_type;
		internal int		format;
		internal int		l0;
		internal int		l1;
		internal int		l2;
		internal int		l3;
		internal int		l4;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XMappingEvent {
		internal XEventName	type;
		internal int		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		request;
		internal int		first_keycode;
		internal int		count;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XErrorEvent {
		internal XEventName	type;
		internal IntPtr		display;
		internal IntPtr		resourceid;
		internal int		serial;
		internal byte		error_code;
		internal byte		request_code;
		internal byte		minor_code;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XEventPad {
		internal int pad0;
		internal int pad1;
		internal int pad2;
		internal int pad3;
		internal int pad4;
		internal int pad5;
		internal int pad6;
		internal int pad7;
		internal int pad8;
		internal int pad9;
		internal int pad10;
		internal int pad11;
		internal int pad12;
		internal int pad13;
		internal int pad14;
		internal int pad15;
		internal int pad16;
		internal int pad17;
		internal int pad18;
		internal int pad19;
		internal int pad20;
		internal int pad21;
		internal int pad22;
		internal int pad23;
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct XEvent {
		[ FieldOffset(0) ] internal XEventName type;
		[ FieldOffset(0) ] internal XAnyEvent AnyEvent;
		[ FieldOffset(0) ] internal XKeyEvent KeyEvent;
		[ FieldOffset(0) ] internal XButtonEvent ButtonEvent;
		[ FieldOffset(0) ] internal XMotionEvent MotionEvent;
		[ FieldOffset(0) ] internal XCrossingEvent CrossingEvent;
		[ FieldOffset(0) ] internal XFocusChangeEvent FocusChangeEvent;
		[ FieldOffset(0) ] internal XExposeEvent ExposeEvent;
		[ FieldOffset(0) ] internal XGraphicsExposeEvent GraphicsExposeEvent;
		[ FieldOffset(0) ] internal XNoExposeEvent NoExposeEvent;
		[ FieldOffset(0) ] internal XVisibilityEvent VisibilityEvent;
		[ FieldOffset(0) ] internal XCreateWindowEvent CreateWindowEvent;
		[ FieldOffset(0) ] internal XDestroyWindowEvent DestroyWindowEvent;
		[ FieldOffset(0) ] internal XUnmapEvent UnmapEvent;
		[ FieldOffset(0) ] internal XMapEvent MapEvent;
		[ FieldOffset(0) ] internal XMapRequestEvent MapRequestEvent;
		[ FieldOffset(0) ] internal XReparentEvent ReparentEvent;
		[ FieldOffset(0) ] internal XConfigureEvent ConfigureEvent;
		[ FieldOffset(0) ] internal XGravityEvent GravityEvent;
		[ FieldOffset(0) ] internal XResizeRequestEvent ResizeRequestEvent;
		[ FieldOffset(0) ] internal XConfigureRequestEvent ConfigureRequestEvent;
		[ FieldOffset(0) ] internal XCirculateEvent CirculateEvent;
		[ FieldOffset(0) ] internal XCirculateRequestEvent CirculateRequestEvent;
		[ FieldOffset(0) ] internal XPropertyEvent PropertyEvent;
		[ FieldOffset(0) ] internal XSelectionClearEvent SelectionClearEvent;
		[ FieldOffset(0) ] internal XSelectionRequestEvent SelectionRequestEvent;
		[ FieldOffset(0) ] internal XSelectionEvent SelectionEvent;
		[ FieldOffset(0) ] internal XColormapEvent ColormapEvent;
		[ FieldOffset(0) ] internal XClientMessageEvent ClientMessageEvent;
		[ FieldOffset(0) ] internal XMappingEvent MappingEvent;
		[ FieldOffset(0) ] internal XErrorEvent ErrorEvent;
		[ FieldOffset(0) ] internal XKeymapEvent KeymapEvent;

		//[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=24)]
		//[ FieldOffset(0) ] internal int[] pad;
		[ FieldOffset(0) ] internal XEventPad Pad;
	}

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct XWindowAttributes {
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal int		border_width;
		internal int		depth;
		internal IntPtr		visual;
		internal IntPtr		root;
		internal int		c_class;
		internal int		bit_gravity;
		internal int		win_gravity;
		internal int		backing_store;
		internal ulong		backing_planes;
		internal ulong		backing_pixel;
		internal bool		save_under;
		internal IntPtr		colormap;
		internal bool		map_installed;
		internal int		map_state;
		internal long		all_event_masks;
		internal long		your_event_mask;
		internal long		do_not_propagate_mask;
		internal bool		override_direct;
		internal IntPtr		screen;
	}
	#endregion

	#region X11 Enumerations
	internal enum XWindowClass {
		InputOutput	= 1,
		InputOnly	= 2
	}

	internal enum XEventName {
		KeyPress                = 2,
		KeyRelease              = 3,
		ButtonPress             = 4,
		ButtonRelease           = 5,
		MotionNotify            = 6,
		EnterNotify             = 7,
		LeaveNotify             = 8,
		FocusIn                 = 9,
		FocusOut                = 10,
		KeymapNotify            = 11,
		Expose                  = 12,
		GraphicsExpose          = 13,
		NoExpose                = 14,
		VisibilityNotify        = 15,
		CreateNotify            = 16,
		DestroyNotify           = 17,
		UnmapNotify             = 18,
		MapNotify               = 19,
		MapRequest              = 20,
		ReparentNotify          = 21,
		ConfigureNotify         = 22,
		ConfigureRequest        = 23,
		GravityNotify           = 24,
		ResizeRequest           = 25,
		CirculateNotify         = 26,
		CirculateRequest        = 27,
		PropertyNotify          = 28,
		SelectionClear          = 29,
		SelectionRequest        = 30,
		SelectionNotify         = 31,
		ColormapNotify          = 32,
		ClientMessage           = 33,
		MappingNotify           = 34,
		LASTEvent               = 35
	}

	internal enum XWindowAttribute {
		CWBackPixmap	= 1,
		CWBackPixel	= 2,
		CWBorderPixmap	= 4,
		CWBorderPixel	= 8,
		CWBitGravity	= 16,
		CWWinGravity	= 32,
		CWBackingStore	= 64,
		CWBackingPlanes	= 128,
		CWBackingPixel	= 256,
		CWOverrideRedirect = 512,
		CWSaveUnder	= 1024,
		CWEventMask	= 2048,
		CWDontPropagate	= 4096,
		CWColorMap	= 8192,
		CWCursor	= 16384
	}

	internal enum XKeySym {
		XK_BackSpace	= 0xFF08,
		XK_Tab		= 0xFF09,
		XK_Clear	= 0xFF0B,
		XK_Return	= 0xFF0D,
		XK_Home		= 0xFF50,
		XK_Left		= 0xFF51,
		XK_Up		= 0xFF52,
		XK_Right	= 0xFF53,
		XK_Down		= 0xFF54,
		XK_Page_Up	= 0xFF55,
		XK_Page_Down	= 0xFF56,
		XK_End		= 0xFF57,
		XK_Begin	= 0xFF58,
		XK_Menu		= 0xFF67,
		XK_Shift_L	= 0xFFE1,
		XK_Shift_R	= 0xFFE2,
		XK_Control_L	= 0xFFE3,
		XK_Control_R	= 0xFFE4,
		XK_Caps_Lock	= 0xFFE5,
		XK_Shift_Lock	= 0xFFE6,	
		XK_Meta_L	= 0xFFE7,
		XK_Meta_R	= 0xFFE8,
		XK_Alt_L	= 0xFFE9,
		XK_Alt_R	= 0xFFEA,
		XK_Super_L	= 0xFFEB,
		XK_Super_R	= 0xFFEC,
		XK_Hyper_L	= 0xFFED,
		XK_Hyper_R	= 0xFFEE,

	}
	#endregion
}

