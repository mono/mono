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
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Windows.Forms {

	internal class X11Dnd {

		private enum State {
			Accepting,
			Dragging
		}

		private enum DragState {
			None,
			Beginning,
			Dragging,
			Entered
		}

		private interface IDataConverter {
			void GetData (X11Dnd dnd, DataObject data, ref XEvent xevent);
			void SetData (X11Dnd dnd, object data, ref XEvent xevent);
		}

		private delegate void MimeConverter (IntPtr dsp,
				DataObject data, ref XEvent xevent);

		private class MimeHandler {
			public string Name;
			public IntPtr Type;
			public IntPtr NonProtocol;
			public IDataConverter Converter;
			
			public MimeHandler (string name, IDataConverter converter)
			{
				Name = name;
				Converter = converter;
			}

			public override string ToString ()
			{
				return "MimeHandler {" + Name + "}";
			}
		}

		private MimeHandler [] MimeHandlers = {
//			  new MimeHandler ("WCF_DIB"),
//			  new MimeHandler ("image/gif", new MimeConverter (ImageConverter)),
//			new MimeHandler ("text/rtf", new MimeConverter (RtfConverter)),
//			new MimeHandler ("text/richtext", new MimeConverter (RtfConverter)),

			new MimeHandler ("text/plain", new TextConverter ()),
			new MimeHandler ("text/html", new HtmlConverter ()),
			new MimeHandler ("text/uri-list", new UriListConverter ()),
			new MimeHandler ("application/x-mono-serialized-object",
					new SerializedObjectConverter ())
		};

		private class SerializedObjectConverter : IDataConverter {

			public void GetData (X11Dnd dnd, DataObject data, ref XEvent xevent)
			{
				MemoryStream stream = dnd.GetData (ref xevent);
				BinaryFormatter bf = new BinaryFormatter ();

				if (stream.Length == 0)
					return;

				stream.Seek (0, 0);
				object obj = bf.Deserialize (stream);
				data.SetData (obj);
			}

			public void SetData (X11Dnd dnd, object data, ref XEvent xevent)
			{
				if (data == null)
					return;

				MemoryStream stream = new MemoryStream ();
				BinaryFormatter bf = new BinaryFormatter ();

				bf.Serialize (stream, data);

				IntPtr buffer = Marshal.AllocHGlobal ((int) stream.Length);
				stream.Seek (0, 0);

				for (int i = 0; i < stream.Length; i++) {
					Marshal.WriteByte (buffer, i, (byte) stream.ReadByte ());
				}

				dnd.SetProperty (ref xevent, buffer, (int) stream.Length);
			}
		}

		private class HtmlConverter : IDataConverter {

			public void GetData (X11Dnd dnd, DataObject data, ref XEvent xevent)
			{
				string text = dnd.GetText (ref xevent, false);
				if (text == null)
					return;
				data.SetData (DataFormats.Text, text);
				data.SetData (DataFormats.UnicodeText, text);
			}

			public void SetData (X11Dnd dnd, object data, ref XEvent xevent)
			{
				IntPtr buffer;
				int len;
				string str = data as string;

				if (str == null)
					return;

				if (xevent.SelectionRequestEvent.target == (int) Atom.XA_STRING) {
					byte [] bytes = Encoding.ASCII.GetBytes (str);
					buffer = Marshal.AllocHGlobal (bytes.Length);
					len = bytes.Length;
					for (int i = 0; i < len; i++)
						Marshal.WriteByte (buffer, i, bytes [i]);
				} else {
					buffer = Marshal.StringToHGlobalAnsi (str);
					len = 0;
					while (Marshal.ReadByte (buffer, len) != 0)
						len++;
				}

				dnd.SetProperty (ref xevent, buffer, len);

				Marshal.FreeHGlobal (buffer);
			}
		}

		private class TextConverter : IDataConverter {

			public void GetData (X11Dnd dnd, DataObject data, ref XEvent xevent)
			{
				string text = dnd.GetText (ref xevent, true);
				if (text == null)
					return;
				data.SetData (DataFormats.Text, text);
				data.SetData (DataFormats.UnicodeText, text);
			}

			public void SetData (X11Dnd dnd, object data, ref XEvent xevent)
			{
				IntPtr buffer;
				int len;
				string str = data as string;

				if (data == null)
					return;
				
				if (xevent.SelectionRequestEvent.target == (int) Atom.XA_STRING) {
					byte [] bytes = Encoding.ASCII.GetBytes (str);
					buffer = Marshal.AllocHGlobal (bytes.Length);
					len = bytes.Length;
					for (int i = 0; i < len; i++)
						Marshal.WriteByte (buffer, i, bytes [i]);
				} else {
					buffer = Marshal.StringToHGlobalAnsi (str);
					len = 0;
					while (Marshal.ReadByte (buffer, len) != 0)
						len++;
				}

				dnd.SetProperty (ref xevent, buffer, len);

				Marshal.FreeHGlobal (buffer);
			}
		}

		private class UriListConverter : IDataConverter {

			public void GetData (X11Dnd dnd, DataObject data, ref XEvent xevent)
			{
				string text = dnd.GetText (ref xevent, false);
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

			public void SetData (X11Dnd dnd, object data, ref XEvent xevent)
			{
				string [] uri_list = data as string [];

				if (uri_list == null)
					return;

				StringBuilder res = new StringBuilder ();
				foreach (string uri_str in uri_list) {
					Uri uri = new Uri (uri_str);
					res.Append (uri.ToString ());
					res.Append ("\r\n");
				}

				IntPtr buffer = Marshal.StringToHGlobalAnsi ((string) data);
				int len = 0;
				while (Marshal.ReadByte (buffer, len) != 0)
					len++;

				dnd.SetProperty (ref xevent, buffer, len);
			}
		}

		private class DragData {
			public IntPtr Window;
			public DragState State;
			public object Data;
			public IntPtr Action;
			public IntPtr [] SupportedTypes;

			public IntPtr LastWindow;
			public IntPtr LastTopLevel;

			public bool WillAccept;
			
			public void Reset ()
			{
				State = DragState.None;
				Data = null;
				SupportedTypes = null;
				WillAccept = false;
			}
		}
		
		private class DropData {

		}

		// This version seems to be the most common
		private static readonly uint [] XdndVersion = new uint [] { 4 }; 

		private IntPtr display;
		private DragData drag_data;
		
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

		private State state;

		private int converts_pending;
		private bool position_recieved;
		private bool status_sent;
		private IntPtr target;
		private IntPtr source;
		private IntPtr toplevel;
		private DataObject data;

		private IntPtr drag_action;
		private Control control;
		private int pos_x, pos_y;
		private DragDropEffects allowed;
		private DragEventArgs drag_event;

		private Cursor CursorNo;
		private Cursor CursorCopy;
		private Cursor CursorMove;
		private Cursor CursorLink;
		private IntPtr CurrentCursorHandle;
		
		public X11Dnd (IntPtr display)
		{
			this.display = display;
			Init ();
		}

		public void SetAllowDrop (Hwnd hwnd, bool allow)
		{
			if (hwnd.allow_drop == allow)
				return;

			XChangeProperty (display, hwnd.whole_window, XdndAware,
					(IntPtr) Atom.XA_ATOM, 32,
					PropertyMode.Replace, XdndVersion, allow ? 1 : 0);
			hwnd.allow_drop = allow;
		}

		public DragDropEffects StartDrag (IntPtr handle, object data,
				DragDropEffects allowed_effects)
		{
			drag_data = new DragData ();
			drag_data.Window = handle;
			drag_data.State = DragState.Beginning;
			
			drag_data.Data = data;
			drag_data.SupportedTypes = DetermineSupportedTypes (data);

			drag_data.Action = ActionFromEffect (allowed_effects);

			if (CursorNo == null) {
				// Make sure the cursors are created
				CursorNo = new Cursor (typeof (X11Dnd), "DnDNo.cur");
				CursorCopy = new Cursor (typeof (X11Dnd), "DnDCopy.cur");
				CursorMove = new Cursor (typeof (X11Dnd), "DnDMove.cur");
				CursorLink = new Cursor (typeof (X11Dnd), "DnDLink.cur");
			}

			drag_data.LastTopLevel = IntPtr.Zero;
			return DragDropEffects.Copy;
		}

		public void HandleButtonRelease (ref XEvent xevent)
		{

			if (drag_data == null)
				return;

			if (drag_data.State == DragState.Beginning) {
				state = State.Accepting;
			} else if (drag_data.State != DragState.None) {

				if (drag_data.WillAccept) {
					SendDrop (drag_data.LastTopLevel, xevent.AnyEvent.window,
							xevent.ButtonEvent.time);
				}

				XplatUIX11.XUngrabPointer (display, 0);
				drag_data.State = DragState.None;
				// WE can't reset the drag data yet as it is still
				// most likely going to be used by the SelectionRequest
				// handlers
			}
		}

		public void HandleMotionNotify (ref XEvent xevent)
		{
			if (drag_data == null)
				return;

			if (drag_data.State == DragState.Beginning) {
				int suc;

				drag_data.State = DragState.Dragging;

				suc = XplatUIX11.XSetSelectionOwner (display, (int) XdndSelection,
						drag_data.Window,
						xevent.ButtonEvent.time);

				if (suc == 0) {
					Console.Error.WriteLine ("Could not take ownership of XdndSelection aborting drag.");
					drag_data.Reset ();
					return;
				}

				suc = XGrabPointer (display, xevent.AnyEvent.window,
						false,
						EventMask.ButtonMotionMask |
						EventMask.PointerMotionMask |
						EventMask.ButtonPressMask |
						EventMask.ButtonReleaseMask,
						GrabMode.GrabModeAsync,
						GrabMode.GrabModeAsync,
						IntPtr.Zero, IntPtr.Zero/*CursorCopy.Handle*/, 0);

				if (suc != 0) {
					Console.Error.WriteLine ("Could not grab pointer aborting drag.");
					drag_data.Reset ();
					return;
				}

				drag_data.State = DragState.Dragging;
			} else if (drag_data.State != DragState.None) {
				bool dnd_aware = false;
				IntPtr toplevel = IntPtr.Zero;
				IntPtr window = XplatUIX11.RootWindowHandle;

				IntPtr root, child;
				int x_temp, y_temp;
				int mask_return;

				while (XQueryPointer (display,
						       window,
						       out root, out child,
						       out x_temp, out y_temp,
						       out xevent.MotionEvent.x,
						       out xevent.MotionEvent.y,
						       out mask_return)) {
					
					if (!dnd_aware) {
						dnd_aware = IsWindowDndAware (window);
						if (dnd_aware) {
							toplevel = window;
							xevent.MotionEvent.x_root = x_temp;
							xevent.MotionEvent.y_root = y_temp;
						}
					}
					
					if (child == IntPtr.Zero)
						break;
					
					window = child;
				}

				if (window != drag_data.LastWindow && drag_data.State == DragState.Entered) {
					drag_data.State = DragState.Dragging;

					// TODO: Send a Leave if this is an MWF window

					if (toplevel != drag_data.LastTopLevel)
						SendLeave (drag_data.LastTopLevel, xevent.MotionEvent.window);
				}

				drag_data.State = DragState.Entered;
				if (toplevel != drag_data.LastTopLevel) {
					// Entering a new toplevel window
					SendEnter (toplevel, drag_data.Window, drag_data.SupportedTypes);
				} else {
					// Already in a toplevel window, so send a position
					SendPosition (toplevel, drag_data.Window,
							drag_data.Action,
							xevent.MotionEvent.x_root,
							xevent.MotionEvent.y_root,
							xevent.MotionEvent.time);
				}

				drag_data.LastTopLevel = toplevel;
				drag_data.LastWindow = window;
			}
		}

		// DEBUG CODE REMOVE
		private string GetText (IntPtr handle) {
			string text = String.Empty;
			IntPtr	textptr;

			textptr = IntPtr.Zero;

			XFetchName (display, handle, ref textptr);
			if (textptr != IntPtr.Zero) {
				text = Marshal.PtrToStringAnsi(textptr);
				XFree (textptr);
			}

			return text;
		}

			
		// return true if the event is handled here
		public bool HandleClientMessage (ref XEvent xevent)
		{
			// most common so we check it first
			if (xevent.ClientMessageEvent.message_type == XdndPosition)
				return Accepting_HandlePositionEvent (ref xevent);
			if (xevent.ClientMessageEvent.message_type == XdndEnter)
				return Accepting_HandleEnterEvent (ref xevent);
			if (xevent.ClientMessageEvent.message_type == XdndDrop)
				return Accepting_HandleDropEvent (ref xevent);
			if (xevent.ClientMessageEvent.message_type == XdndLeave)
				return Accepting_HandleLeaveEvent (ref xevent);
			if (xevent.ClientMessageEvent.message_type == XdndStatus)
				return HandleStatusEvent (ref xevent);

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

			handler.Converter.GetData (this, data, ref xevent);

			converts_pending--;
			if (converts_pending <= 0 && position_recieved) {
				drag_event = new DragEventArgs (data, 0, pos_x, pos_y,
					allowed, DragDropEffects.None);
				control.DndEnter (drag_event);
				SendStatus (source, drag_event.Effect);
				status_sent = true;
			}
			return true;
		}

		public bool HandleSelectionRequestEvent (ref XEvent xevent)
		{
			if (xevent.SelectionRequestEvent.selection != (int) XdndSelection)
				return false;

			MimeHandler handler = FindHandler ((IntPtr) xevent.SelectionRequestEvent.target);
			if (handler == null)
				return false;

			handler.Converter.SetData (this, drag_data.Data, ref xevent);

			return true;
		}

		private void SetProperty (ref XEvent xevent, IntPtr data, int length)
		{
			XEvent sel = new XEvent();
			sel.SelectionEvent.type = XEventName.SelectionNotify;
			sel.SelectionEvent.send_event = true;
			sel.SelectionEvent.display = display;
			sel.SelectionEvent.selection = xevent.SelectionRequestEvent.selection;
			sel.SelectionEvent.target = xevent.SelectionRequestEvent.target;
			sel.SelectionEvent.requestor = xevent.SelectionRequestEvent.requestor;
			sel.SelectionEvent.time = xevent.SelectionRequestEvent.time;
			sel.SelectionEvent.property = 0;

			XplatUIX11.XChangeProperty (display, xevent.SelectionRequestEvent.requestor,
					xevent.SelectionRequestEvent.property,
					xevent.SelectionRequestEvent.target,
					8, PropertyMode.Replace, data, length);
			sel.SelectionEvent.property = xevent.SelectionRequestEvent.property;

			XSendEvent (display, xevent.SelectionRequestEvent.requestor, false,
					EventMask.NoEventMask, ref sel);
			return;
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
		
		private bool Accepting_HandleEnterEvent (ref XEvent xevent)
		{
			Reset ();

			source = xevent.ClientMessageEvent.ptr1;
			toplevel = xevent.AnyEvent.window;
			target = IntPtr.Zero;

			ConvertData (ref xevent);

			return true;
		}

		private bool Accepting_HandlePositionEvent (ref XEvent xevent)
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
			if (!c.allow_drop) {
				SendStatus (source, DragDropEffects.None);
				Finish ();
				return true;
			}

			control = c;
			position_recieved = true;			

			if (converts_pending > 0)
				return true;
			if (!status_sent) {
				drag_event = new DragEventArgs (data, 0, pos_x, pos_y,
					allowed, DragDropEffects.None);
				control.DndEnter (drag_event);
				SendStatus (source, drag_event.Effect);
				status_sent = true;
			} else {
				SendStatus (source, drag_event.Effect);
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

		private bool Accepting_HandleDropEvent (ref XEvent xevent)
		{
			if (control != null && drag_event != null) {
				drag_event = new DragEventArgs (data,
						0, pos_x, pos_y,
					allowed, DragDropEffects.None);
				control.DndDrop (drag_event);
			}
			SendFinished ();
			return true;
		}

		private bool Accepting_HandleLeaveEvent (ref XEvent xevent)
		{
			if (control != null && drag_event != null)
				control.DndLeave (drag_event);
			// Reset ();
			return true;
		}

		private bool HandleStatusEvent (ref XEvent xevent)
		{
			if (drag_data != null && drag_data.State == DragState.Entered) {
				drag_data.WillAccept = ((int) xevent.ClientMessageEvent.ptr2 & 0x1) != 0;
				Cursor cursor = CursorNo;
				if (drag_data.WillAccept) {
					// Same order as on MS
					IntPtr action = xevent.ClientMessageEvent.ptr5;
					if (action == XdndActionCopy)
						cursor = CursorCopy;
					else if (action == XdndActionLink)
						cursor = CursorLink;
					else if (action == XdndActionMove)
						cursor = CursorMove;

				}

					// TODO: Try not to set the cursor so much
				//if (cursor.Handle != CurrentCursorHandle) {
					XChangeActivePointerGrab (display,
							EventMask.ButtonMotionMask |
							EventMask.PointerMotionMask |
							EventMask.ButtonPressMask |
							EventMask.ButtonReleaseMask,
							cursor.Handle, IntPtr.Zero);
					CurrentCursorHandle = cursor.Handle;
					//}	
			}
			return true;
		}

		private DragDropEffects EffectFromAction (IntPtr action)
		{
			DragDropEffects allowed = DragDropEffects.None;

			if (action == XdndActionCopy)
				allowed = DragDropEffects.Copy;
			else if (action == XdndActionMove)
				allowed |= DragDropEffects.Move;
			if (action == XdndActionLink)
				allowed |= DragDropEffects.Link;
			return allowed;
		}

		private IntPtr ActionFromEffect (DragDropEffects effect)
		{
			IntPtr action = IntPtr.Zero;

			// We can't OR together actions on XDND so sadly the primary
			// is the only one shown here
			if ((effect & DragDropEffects.Copy) != 0)
				action = XdndActionCopy;
			else if ((effect & DragDropEffects.Move) != 0)
				action = XdndActionMove;
			else if ((effect & DragDropEffects.Link) != 0)
				action = XdndActionLink;
			return action;
		}

		private bool ConvertData (ref XEvent xevent)
		{
			bool match = false;

			XplatUIX11.XGetSelectionOwner (display, (int) XdndSelection);

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

		private MimeHandler FindHandler (string name)
		{
			foreach (MimeHandler handler in MimeHandlers) {
				if (handler.Name == name)
					return handler;
			}
			return null;
		}

		private void SendStatus (IntPtr source, DragDropEffects effect)
		{
			XEvent xevent = new XEvent ();

			xevent.AnyEvent.type = XEventName.ClientMessage;
			xevent.AnyEvent.display = display;
			xevent.ClientMessageEvent.window = source;
			xevent.ClientMessageEvent.message_type = XdndStatus;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = toplevel;
			if (effect != DragDropEffects.None)
				xevent.ClientMessageEvent.ptr2 = (IntPtr) 1;

			xevent.ClientMessageEvent.ptr5 = ActionFromEffect (effect);
			XSendEvent (display, source, false, 0, ref xevent);
		}

		private void SendEnter (IntPtr handle, IntPtr from, IntPtr [] supported)
		{
			XEvent xevent = new XEvent ();

			xevent.AnyEvent.type = XEventName.ClientMessage;
			xevent.AnyEvent.display = display;
			xevent.ClientMessageEvent.window = handle;
			xevent.ClientMessageEvent.message_type = XdndEnter;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = from;

			// (int) xevent.ClientMessageEvent.ptr2 & 0x1)
			// int ptr2 = 0x1;
			// xevent.ClientMessageEvent.ptr2 = (IntPtr) ptr2;
			// (e)->xclient.data.l[1] = ((e)->xclient.data.l[1] & ~(0xFF << 24)) | ((v) << 24)
			xevent.ClientMessageEvent.ptr2 = (IntPtr) (XdndVersion [0] << 24);
			
			if (supported.Length > 0)
				xevent.ClientMessageEvent.ptr3 = supported [0];
			if (supported.Length > 1)
				xevent.ClientMessageEvent.ptr4 = supported [1];
			if (supported.Length > 2)
				xevent.ClientMessageEvent.ptr5 = supported [2];

			XSendEvent (display, handle, false, 0, ref xevent);
		}

		private void SendDrop (IntPtr handle, IntPtr from, IntPtr time)
		{
			XEvent xevent = new XEvent ();

			xevent.AnyEvent.type = XEventName.ClientMessage;
			xevent.AnyEvent.display = display;
			xevent.ClientMessageEvent.window = handle;
			xevent.ClientMessageEvent.message_type = XdndDrop;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = from;
			xevent.ClientMessageEvent.ptr3 = time;
			
			XSendEvent (display, handle, false, 0, ref xevent);
		}

		private void SendPosition (IntPtr handle, IntPtr from, IntPtr action, int x, int y, IntPtr time)
		{
			XEvent xevent = new XEvent ();

			xevent.AnyEvent.type = XEventName.ClientMessage;
			xevent.AnyEvent.display = display;
			xevent.ClientMessageEvent.window = handle;
			xevent.ClientMessageEvent.message_type = XdndPosition;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = from;
			xevent.ClientMessageEvent.ptr3 = (IntPtr) ((x << 16) | (y & 0xFFFF));
			xevent.ClientMessageEvent.ptr4 = time;
			xevent.ClientMessageEvent.ptr5 = action;
			
			XSendEvent (display, handle, false, 0, ref xevent);
		}

		private void SendLeave (IntPtr handle, IntPtr from)
		{
			XEvent xevent = new XEvent ();

			xevent.AnyEvent.type = XEventName.ClientMessage;
			xevent.AnyEvent.display = display;
			xevent.ClientMessageEvent.window = handle;
			xevent.ClientMessageEvent.message_type = XdndLeave;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = from;

			XSendEvent (display, handle, false, 0, ref xevent);
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
					res [i] = (IntPtr) Marshal.ReadInt32 (data, i *
							Marshal.SizeOf (typeof (int)));
				}

				XFree (data);
			}

			return res;
		}

		private string GetText (ref XEvent xevent, bool unicode)
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

		private MemoryStream GetData (ref XEvent xevent)
		{
			int nread = 0;
			int nitems;
			int bytes_after;

			MemoryStream res = new MemoryStream ();
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

				for (int i = 0; i < nitems; i++)
					res.WriteByte (Marshal.ReadByte (data, i));
				nread += nitems;

				XFree (data);
			} while (bytes_after > 0);
			return res;
		}

		private Control MwfWindow (IntPtr window)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (window);
			if (hwnd == null)
				return null;

			Control res = Control.FromHandle (hwnd.client_window);
			return res;
		}

		private bool IsWindowDndAware (IntPtr handle)
		{
			bool res = true;
			// Check the version number, we need greater than 3
			IntPtr actual;
			int format, count, remaining;
			IntPtr data = IntPtr.Zero;
			
			XGetWindowProperty (display, handle, XdndAware, 0, 0x8000000, false,
					(IntPtr) Atom.XA_ATOM, out actual, out format,
					out count, out remaining, out data);
			
			if (actual != (IntPtr) Atom.XA_ATOM || format != 32 ||
					count == 0 || data == IntPtr.Zero) {
				if (data != IntPtr.Zero)
					XFree (data);
				return false;
			}

			int version = Marshal.ReadInt32 (data, 0);

			if (version < 3) {
				Console.Error.WriteLine ("XDND Version too old (" + version + ").");
				XFree (data);
				return false;
			}

			// First type is actually the XDND version
			if (count > 1) {
				res = false;
				for (int i = 1; i < count; i++) {
					IntPtr type = (IntPtr) Marshal.ReadInt32 (data, i *
							Marshal.SizeOf (typeof (int)));
					for (int j = 0; j < drag_data.SupportedTypes.Length; j++) {
						if (drag_data.SupportedTypes [j] == type) {
							res = true;
							break;
						}
					}
				}
			}

			XFree (data);
			return res;
		}

		private IntPtr [] DetermineSupportedTypes (object data)
		{
			ArrayList res = new ArrayList ();

			if (data is string) {
				MimeHandler handler = FindHandler ("text/plain");
				if (handler != null)
					res.Add (handler.Type);
			}/* else if (data is Bitmap)
				res.Add (data);

			 */

			if (data is IDataObject) {
				

			}

			if (data is ISerializable) {
				MimeHandler handler = FindHandler ("application/x-mono-serialized-object");
				if (handler != null)
					res.Add (handler.Type);
			}

			return (IntPtr []) res.ToArray (typeof (IntPtr));
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
		internal extern static IntPtr XGetSelectionOwner (IntPtr display, IntPtr selection);

		[DllImport ("libX11")]
		internal extern static int XGrabPointer (IntPtr display, IntPtr window,
				bool owner_events, EventMask event_mask, GrabMode pointer_mode,
				GrabMode keyboard_mode, IntPtr confine_to, IntPtr cursor, uint timestamp);

		[DllImport ("libX11")]
		internal extern static bool XQueryPointer (IntPtr display, IntPtr window, out IntPtr root,
				out IntPtr child, out int root_x, out int root_y, out int win_x,
				out int win_y, out int keys_buttons);

		[DllImport ("libX11")]
		internal extern static int XAllowEvents (IntPtr display, int event_mode, IntPtr time);

		[DllImport ("libX11")]
		internal extern static int XFree(IntPtr data);

		[DllImport ("libX11")]
		internal extern static int XFetchName (IntPtr display, IntPtr window, ref IntPtr window_name);
		[DllImport ("libX11")]
		internal extern static int XChangeActivePointerGrab (IntPtr display, EventMask event_mask, IntPtr cursor, IntPtr time);
	}

}
