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
//	Jackson Harper (jackson@ximian.com)
//
//


using System;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;


namespace System.Windows.Forms {

	internal class X11Dnd {

		private delegate void MimeConverter (IntPtr dsp,
				DataObject data, ref XEvent xevent);

		private class MimeHandler {
			public string Name;
			public IntPtr Type;
			public IntPtr NonProtocol;
			public MimeConverter Convert;
			
			public MimeHandler (string name, MimeConverter converter)
			{
				Name = name;
				Convert = converter;
			}
		}

		private MimeHandler [] MimeHandlers = {
//			  new MimeHandler ("WCF_DIB"),
//			  new MimeHandler ("image/gif", new MimeConverter (ImageConverter)),

			
			new MimeHandler ("text/rtf", new MimeConverter (RtfConverter)),
			new MimeHandler ("text/richtext", new MimeConverter (RtfConverter)),
			new MimeHandler ("text/plain", new MimeConverter (TextConverter)),
			new MimeHandler ("text/html", new MimeConverter (HtmlConverter)),
			new MimeHandler ("text/uri-list", new MimeConverter (UriListConverter)),
		};

		// This version seems to be the most common
		private static readonly uint [] XdndVersion = new uint [] { 4 }; 

		private IntPtr display;
		
	     
		private bool initialized;

		private IntPtr XdndAware;
		private IntPtr XdndSelection;
		private IntPtr XdndEnter;
		private IntPtr XdndLeave;
		private IntPtr XdndPosition;
		private IntPtr XdndDrop;
		private IntPtr XdndFinished;
		private IntPtr XdndStatus;
		private IntPtr XdndTypeList;
		private IntPtr XdndActionCopy;
		private IntPtr XdndActionMove;
		private IntPtr XdndActionLink;
		private IntPtr XdndActionPrivate;
		private IntPtr XdndActionList;
		private IntPtr XdndActionDescription;
		private IntPtr XdndActionAsk;

		private int converts_pending;
		private bool position_recieved;
		private bool status_sent;
		private IntPtr target;
		private IntPtr source;
		private IntPtr toplevel;
		private DataObject data;
		private Control control;
		private int pos_x, pos_y;
		private DragDropEffects allowed;
		private DragEventArgs drag_event;

		public X11Dnd (IntPtr display)
		{
			this.display = display;
		}

		public void SetAllowDrop (Hwnd hwnd, bool allow)
		{
			if (!initialized)
				Init ();

//			  if (hwnd.allow_drop == allow)
//				  return;

			XChangeProperty (display, hwnd.whole_window, XdndAware,
					(IntPtr) Atom.XA_ATOM, 32,
					PropertyMode.Replace, XdndVersion, allow ? 1 : 0);
//			  hwnd.allow_drop = allow;
		}

		// return true if the event is handled here
		public bool HandleClientMessage (ref XEvent xevent)
		{
			if (!initialized)
				Init ();

			// most common so we check it first
			if (xevent.ClientMessageEvent.message_type == XdndPosition)
				return HandlePositionEvent (ref xevent);
			if (xevent.ClientMessageEvent.message_type == XdndEnter)
				return HandleEnterEvent (ref xevent);
			if (xevent.ClientMessageEvent.message_type == XdndDrop)
				return HandleDropEvent (ref xevent);
			if (xevent.ClientMessageEvent.message_type == XdndLeave)
				return HandleLeaveEvent (ref xevent);
			
			return false;
		}

		public bool HandleSelectionNotifyEvent (ref XEvent xevent)
		{
			if (source != XGetSelectionOwner (display, XdndSelection))
				return false;

			MimeHandler handler = FindHandler ((IntPtr) xevent.SelectionEvent.target);
			if (handler == null)
				return false;
			if (data == null)
				data = new DataObject ();

			handler.Convert (display, data, ref xevent);

			converts_pending--;
			if (converts_pending <= 0 && position_recieved)
				SendEnterStatus ();
			return true;
		}

		private void Reset ()
		{
			ResetSourceData ();
			ResetTargetData ();
		}

		private void ResetSourceData ()
		{
			converts_pending = 0;
			data = null;
		}

		private void ResetTargetData ()
		{
			position_recieved = false;
			status_sent = false;
		}
		
		private bool HandleEnterEvent (ref XEvent xevent)
		{
			Reset ();

			source = xevent.ClientMessageEvent.ptr1;
			toplevel = xevent.AnyEvent.window;
			target = IntPtr.Zero;

			ConvertData (ref xevent);

			return true;
		}

		private bool HandlePositionEvent (ref XEvent xevent)
		{
			int x = (int) xevent.ClientMessageEvent.ptr3 >> 16;
			int y = (int) xevent.ClientMessageEvent.ptr3 & 0xFFFF;

			allowed = EffectFromAction (xevent.ClientMessageEvent.ptr5);

			IntPtr parent, child, new_child;
			parent = XplatUIX11.XRootWindow (display, 0);
			child = toplevel;
			while (true) {
				int xd, yd;
				new_child = IntPtr.Zero;
				
				if (!XplatUIX11.XTranslateCoordinates (display,
						    parent, child, x, y,
						    out xd, out yd, out new_child))
					break;
				if (new_child == IntPtr.Zero)
					break;
				child = new_child;
			}

			if (target != child) {
				// We have moved into a new control 
				// or into a control for the first time
				Finish ();
			}
			target = child;
			Hwnd hwnd = Hwnd.ObjectFromHandle (target);
			Control c = Control.FromHandle (hwnd.client_window);

			if (c == null)
				return true;
//			if (!c.AllowDrop) {
//				Finish ();
//				return true;
//			}

			control = c;
			position_recieved = true;			

			if (converts_pending > 0)
				return true;
			if (!status_sent) {
				SendEnterStatus ();
			} else {
				SendStatus ();
				control.DndOver (drag_event);
			}
			
			return true;
		}

		private void Finish ()
		{
			if (control != null) {
				if (drag_event == null) {
					if (data == null)
						data = new DataObject ();
					drag_event = new DragEventArgs (data,
							0, pos_x, pos_y,
					allowed, DragDropEffects.None);
				}
				control.DndLeave (drag_event);
			}
			ResetTargetData ();
		}

		private bool HandleDropEvent (ref XEvent xevent)
		{
			Console.WriteLine ("DROPPING EVENT");
			if (control != null && drag_event != null)
				control.DndDrop (drag_event);
			SendFinished ();
			return true;
		}

		private bool HandleLeaveEvent (ref XEvent xevent)
		{
			if (control != null && drag_event != null)
				control.DndLeave (drag_event);
			Reset ();
			return true;
		}

		private DragDropEffects EffectFromAction (IntPtr action)
		{
			DragDropEffects allowed = DragDropEffects.None;
			if (action == XdndActionCopy)
				allowed = DragDropEffects.Copy;
			if (action == XdndActionMove)
				allowed = DragDropEffects.Move;
			if (action == XdndActionLink)
				allowed = DragDropEffects.Link;
			return allowed;
		}

		private IntPtr ActionFromEffect (DragDropEffects effect)
		{
			IntPtr action = IntPtr.Zero;
			if (effect == DragDropEffects.Copy)
				action = XdndActionCopy;
			if (effect == DragDropEffects.Move)
				action = XdndActionMove;
			if (effect == DragDropEffects.Link)
				action = XdndActionLink;
			return action;
		}

		private bool ConvertData (ref XEvent xevent)
		{
			bool match = false;

			foreach (IntPtr atom in SourceSupportedList (ref xevent)) {
				MimeHandler handler = FindHandler (atom);
				if (handler == null)
					continue;
				XConvertSelection (display, XdndSelection, handler.Type,
					handler.NonProtocol, toplevel, 0 /* CurrentTime */);
				converts_pending++;
				match = true;
			}
			return match;
		}

		private MimeHandler FindHandler (IntPtr atom)
		{
			if (atom == IntPtr.Zero)
				return null;
			foreach (MimeHandler handler in MimeHandlers) {
				if (handler.Type == atom)
					return handler;
			}
			return null;
		}

		private void SendStatus ()
		{
			DragDropEffects action = drag_event.Effect;
			XEvent xevent = new XEvent ();

			xevent.AnyEvent.type = XEventName.ClientMessage;
			xevent.AnyEvent.display = display;
			xevent.ClientMessageEvent.window = source;
			xevent.ClientMessageEvent.message_type = XdndStatus;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = toplevel;
			if (drag_event.Effect != DragDropEffects.None)
				xevent.ClientMessageEvent.ptr2 = (IntPtr) 1;

			xevent.ClientMessageEvent.ptr5 = ActionFromEffect (action);
			XSendEvent (display, source, false, 0, ref xevent);
		}

		private void SendEnterStatus ()
		{
			drag_event = new DragEventArgs (data, 0, pos_x, pos_y,
					allowed, DragDropEffects.None);
			control.DndEnter (drag_event);

			XEvent xevent = new XEvent ();

			xevent.AnyEvent.type = XEventName.ClientMessage;
			xevent.AnyEvent.display = display;
			xevent.ClientMessageEvent.window = source;
			xevent.ClientMessageEvent.message_type = XdndStatus;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = toplevel;
			if (drag_event.Effect != DragDropEffects.None)
				xevent.ClientMessageEvent.ptr2 = (IntPtr) 1;

			xevent.ClientMessageEvent.ptr5 = ActionFromEffect (drag_event.Effect);
			XSendEvent (display, source, false, 0, ref xevent);

			status_sent = true;
		}

		private void SendFinished ()
		{
			XEvent xevent = new XEvent ();

			xevent.AnyEvent.type = XEventName.ClientMessage;
			xevent.AnyEvent.display = display;
			xevent.ClientMessageEvent.window = source;
			xevent.ClientMessageEvent.message_type = XdndFinished;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = toplevel;

			XSendEvent (display, source, false, 0, ref xevent);
		}

		// There is a somewhat decent amount of overhead
		// involved in setting up dnd so we do it lazily
		// as a lot of applications do not even use it.
		private void Init ()
		{
			XdndAware = XInternAtom (display, "XdndAware", false);
			XdndEnter = XInternAtom (display, "XdndEnter", false);
			XdndLeave = XInternAtom (display, "XdndLeave", false);
			XdndPosition = XInternAtom (display, "XdndPosition", false);
			XdndStatus = XInternAtom (display, "XdndStatus", false);
			XdndDrop = XInternAtom (display, "XdndDrop", false);
			XdndSelection = XInternAtom (display, "XdndSelection", false);
			XdndFinished = XInternAtom (display, "XdndFinished", false);
			XdndTypeList = XInternAtom (display, "XdndTypeList", false);
			XdndActionCopy = XInternAtom (display, "XdndActionCopy", false);
			XdndActionMove = XInternAtom (display, "XdndActionMove", false);
			XdndActionLink = XInternAtom (display, "XdndActionLink", false);
			XdndActionPrivate = XInternAtom (display, "XdndActionPrivate", false);
			XdndActionList = XInternAtom (display, "XdndActionList", false);
			XdndActionDescription = XInternAtom (display, "XdndActionDescription", false);
			XdndActionAsk = XInternAtom (display, "XdndActionAsk", false);

			foreach (MimeHandler handler in MimeHandlers) {
				handler.Type = XInternAtom (display, handler.Name, false);
				handler.NonProtocol = XInternAtom (display,
						String.Concat ("MWFNonP+", handler.Name), false);
			}

			initialized = true;
		}

		private IntPtr [] SourceSupportedList (ref XEvent xevent)
		{
			IntPtr [] res;

			if (((int) xevent.ClientMessageEvent.ptr2 & 0x1) == 0) {
				res = new IntPtr [3];
				res [0] = xevent.ClientMessageEvent.ptr3;
				res [1] = xevent.ClientMessageEvent.ptr4;
				res [2] = xevent.ClientMessageEvent.ptr5;
			} else {
				IntPtr type;
				int format, count, remaining;
				IntPtr data = IntPtr.Zero;

				XGetWindowProperty (display, source, XdndTypeList,
						0, 32, false, (IntPtr) Atom.XA_ATOM,
						out type, out format, out count,
						out remaining, out data);

				res = new IntPtr [count];
				for (int i = 0; i < count; i++) {
					res [i] = (IntPtr) Marshal.ReadInt32 (data, i * Marshal.SizeOf(typeof(int)));
				}
			}

			return res;
		}

		private static void UriListConverter (IntPtr display, DataObject data,
				ref XEvent xevent)
		{
			string text = GetText (display, ref xevent, false);
			if (text == null)
				return;

			// TODO: Do this in a loop instead of just splitting
			ArrayList uri_list = new ArrayList ();
			string [] lines = text.Split (new char [] { '\r', '\n' });
			foreach (string line in lines) {
				// # is a comment line (see RFC 2483)
				if (line.StartsWith ("#"))
					continue;
				try {
					Uri uri = new Uri (line);
					uri_list.Add (uri.LocalPath);
				} catch { }
			}

			string [] l = (string []) uri_list.ToArray (typeof (string));
			if (l.Length < 1)
				return;
			data.SetData (DataFormats.FileDrop, l);
			data.SetData ("FileName", l [0]);
			data.SetData ("FileNameW", l [0]);
		}

		private static void TextConverter (IntPtr display, DataObject data, ref XEvent xevent)
		{
			string text = GetText (display, ref xevent, false);
			if (text == null)
				return;
			data.SetData (DataFormats.Text, text);
			data.SetData (DataFormats.UnicodeText, text);
		}

		private static void HtmlConverter (IntPtr display, DataObject data, ref XEvent xevent)
		{
			string html = GetText (display, ref xevent, true);
			if (html == null)
				return;
			data.SetData (DataFormats.Html, html);
		}

		private static void ImageConverter (IntPtr display, DataObject data, ref XEvent xevent)
		{
		}

		private static void RtfConverter (IntPtr display, DataObject data, ref XEvent xevent)
		{
		}

		private static string GetText (IntPtr display, ref XEvent xevent, bool unicode)
		{
			int nread = 0;
			int nitems;
			int bytes_after;

			StringBuilder builder = new StringBuilder ();
			do {
				IntPtr actual_type;
				int actual_fmt;
				IntPtr data = IntPtr.Zero;

				if (0 != XGetWindowProperty (display,
						    xevent.AnyEvent.window,
						    (IntPtr) xevent.SelectionEvent.property,
						    0, 0xffffff, false,
						    (IntPtr) Atom.AnyPropertyType, out actual_type,
						    out actual_fmt, out nitems, out bytes_after,
						    out data)) {
					XFree (data);
					break;
				}

				if (unicode)
					builder.Append (Marshal.PtrToStringUni (data));
				else
					builder.Append (Marshal.PtrToStringAnsi (data));
				nread += nitems;

				XFree (data);
			} while (bytes_after > 0);
			if (nread == 0)
				return null;
			return builder.ToString ();
		}

		[DllImport ("libX11")]
		private extern static string XGetAtomName (IntPtr display, IntPtr atom);

		[DllImport ("libX11")]
		private extern static IntPtr XInternAtom (IntPtr display, string atom_name, bool only_if_exists);

		[DllImport ("libX11")]
		private extern static int XChangeProperty (IntPtr display, IntPtr window, IntPtr property,
				IntPtr format, int type, PropertyMode  mode, uint [] atoms, int nelements);

		[DllImport ("libX11")]
		private extern static int XGetWindowProperty (IntPtr display, IntPtr window,
				IntPtr atom, int long_offset, int long_length, bool delete,
				IntPtr req_type, out IntPtr actual_type, out int actual_format,
				out int nitems, out int bytes_after, out IntPtr prop);

		[DllImport ("libX11")]
		internal extern static int XSendEvent (IntPtr display, IntPtr window,
				bool propagate, EventMask event_mask, ref XEvent send_event);

		[DllImport ("libX11")]
		internal extern static int XConvertSelection (IntPtr display, IntPtr selection,
				IntPtr target, IntPtr property, IntPtr requestor, int time);

		[DllImport ("libX11")]
		internal extern static IntPtr XGetSelectionOwner(IntPtr display, IntPtr selection);

		[DllImport ("libX11")]
		internal extern static int XFree(IntPtr data);
	}

}
