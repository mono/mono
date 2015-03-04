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
// NOTE: We have some tests in Test/System.Windows.Forms/DragAndDropTest.cs, which I *highly* recommend
// to run after any change made here, since those tests are interactive, and thus are not part of
// the common tests.
//


using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
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
			void GetData (X11Dnd dnd, IDataObject data, ref XEvent xevent);
			void SetData (X11Dnd dnd, object data, ref XEvent xevent);
		}

		private delegate void MimeConverter (IntPtr dsp,
				IDataObject data, ref XEvent xevent);

		private class MimeHandler {
			public string Name;
			public string [] Aliases;
			public IntPtr Type;
			public IntPtr NonProtocol;
			public IDataConverter Converter;
			
			public MimeHandler (string name, IDataConverter converter) : this (name, converter, name)
			{
			}

			public MimeHandler (string name, IDataConverter converter, params string [] aliases)
			{
				Name = name;
				Converter = converter;
				Aliases = aliases;
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
			new MimeHandler ("text/plain", new TextConverter (), "System.String", DataFormats.Text),
			new MimeHandler ("text/html", new HtmlConverter (), DataFormats.Html),
			new MimeHandler ("text/uri-list", new UriListConverter (), DataFormats.FileDrop),
			new MimeHandler ("application/x-mono-serialized-object",
					new SerializedObjectConverter ())
		};

		private class SerializedObjectConverter : IDataConverter {

			public void GetData (X11Dnd dnd, IDataObject data, ref XEvent xevent)
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

			public void GetData (X11Dnd dnd, IDataObject data, ref XEvent xevent)
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

				if (xevent.SelectionRequestEvent.target == (IntPtr)Atom.XA_STRING) {
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

			public void GetData (X11Dnd dnd, IDataObject data, ref XEvent xevent)
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

				if (str == null) {
					IDataObject dobj = data as IDataObject;
					if (dobj == null)
						return;
					str = (string) dobj.GetData ("System.String", true);
				}

				if (xevent.SelectionRequestEvent.target == (IntPtr)Atom.XA_STRING) {
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

			public void GetData (X11Dnd dnd, IDataObject data, ref XEvent xevent)
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

				if (uri_list == null) {
					IDataObject dobj = data as IDataObject;
					if (dobj == null)
						return;
					uri_list = dobj.GetData (DataFormats.FileDrop, true) as string [];
				}

				if (uri_list == null)
					return;

				StringBuilder res = new StringBuilder ();
				foreach (string uri_str in uri_list) {
					Uri uri = new Uri (uri_str);
					res.Append (uri.ToString ());
					res.Append ("\r\n");
				}

				IntPtr buffer = Marshal.StringToHGlobalAnsi ((string) res.ToString ());
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
			public MouseButtons MouseState;
			public DragDropEffects AllowedEffects;
			public Point CurMousePos;
			
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

		// This version seems to be the most common
		private static readonly IntPtr [] XdndVersion = new IntPtr [] { new IntPtr (4) }; 

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
		//private IntPtr XdndActionPrivate;
		private IntPtr XdndActionList;
		//private IntPtr XdndActionDescription;
		//private IntPtr XdndActionAsk;

		//private State state;

		private int converts_pending;
		private bool position_recieved;
		private bool status_sent;
		private IntPtr target;
		private IntPtr source;
		private IntPtr toplevel;
		private IDataObject data;

		private Control control;
		private int pos_x, pos_y;
		private DragDropEffects allowed;
		private DragEventArgs drag_event;

		private Cursor CursorNo;
		private Cursor CursorCopy;
		private Cursor CursorMove;
		private Cursor CursorLink;
		// check out the TODO below
		//private IntPtr CurrentCursorHandle;

		private bool tracking = false;
		private bool dropped = false;
		private int motion_poll;
		//private X11Keyboard keyboard;

		public X11Dnd (IntPtr display, X11Keyboard keyboard)
		{
			this.display = display;
			//this.keyboard = keyboard;

			Init ();
		}

		public bool InDrag()
		{
			if (drag_data == null)
				return false;
			return drag_data.State != DragState.None;
		}
		
		public void SetAllowDrop (Hwnd hwnd, bool allow)
		{
			int[] atoms;

			if (hwnd.allow_drop == allow)
				return;

			atoms = new int[XdndVersion.Length];
			for (int i = 0; i < XdndVersion.Length; i++) {
				atoms[i] = XdndVersion[i].ToInt32();
			}

			XplatUIX11.XChangeProperty (display, hwnd.whole_window, XdndAware,
					(IntPtr) Atom.XA_ATOM, 32,
					PropertyMode.Replace, atoms, allow ? 1 : 0);
			hwnd.allow_drop = allow;
		}

		public DragDropEffects StartDrag (IntPtr handle, object data,
				DragDropEffects allowed_effects)
		{
			drag_data = new DragData ();
			drag_data.Window = handle;
			drag_data.State = DragState.Beginning;
			drag_data.MouseState = XplatUIX11.MouseState;
			drag_data.Data = data;
			drag_data.SupportedTypes = DetermineSupportedTypes (data);
			drag_data.AllowedEffects = allowed_effects;
			drag_data.Action = ActionFromEffect (allowed_effects);

			if (CursorNo == null) {
				// Make sure the cursors are created
				CursorNo = new Cursor (typeof (X11Dnd), "DnDNo.cur");
				CursorCopy = new Cursor (typeof (X11Dnd), "DnDCopy.cur");
				CursorMove = new Cursor (typeof (X11Dnd), "DnDMove.cur");
				CursorLink = new Cursor (typeof (X11Dnd), "DnDLink.cur");
			}

			drag_data.LastTopLevel = IntPtr.Zero;
			control = null;

			System.Windows.Forms.MSG msg = new MSG();
			object queue_id = XplatUI.StartLoop (Thread.CurrentThread);

			Timer timer = new Timer ();
			timer.Tick += new EventHandler (DndTickHandler);
			timer.Interval = 100;

			int suc;
			drag_data.State = DragState.Dragging;

			suc = XplatUIX11.XSetSelectionOwner (display, XdndSelection,
					drag_data.Window, IntPtr.Zero);

			if (suc == 0) {
				Console.Error.WriteLine ("Could not take ownership of XdndSelection aborting drag.");
				drag_data.Reset ();
				return DragDropEffects.None;
			}

			drag_data.State = DragState.Dragging;
			drag_data.CurMousePos = new Point ();
			source = toplevel = target = IntPtr.Zero;
			dropped = false;
			tracking = true;
			motion_poll = -1;
			timer.Start ();

			// Send Enter to the window initializing the dnd operation - which initializes the data
			SendEnter (drag_data.Window, drag_data.Window, drag_data.SupportedTypes);
			drag_data.LastTopLevel = toplevel;

			while (tracking && XplatUI.GetMessage (queue_id, ref msg, IntPtr.Zero, 0, 0)) {

				if (msg.message >= Msg.WM_KEYFIRST && msg.message <= Msg.WM_KEYLAST) {
					HandleKeyMessage (msg);
				} else {
					switch (msg.message) {
					case Msg.WM_LBUTTONUP:
					case Msg.WM_RBUTTONUP:
					case Msg.WM_MBUTTONUP:
						if (msg.message == Msg.WM_LBUTTONDOWN && drag_data.MouseState != MouseButtons.Left)
							break;;
						if (msg.message == Msg.WM_RBUTTONDOWN && drag_data.MouseState != MouseButtons.Right)
							break;
						if (msg.message == Msg.WM_MBUTTONDOWN && drag_data.MouseState != MouseButtons.Middle)
							break;
						
						HandleButtonUpMsg ();

						// We don't want to dispatch button up neither (Match .Net)
						// Thus we have to remove capture by ourselves
						RemoveCapture (msg.hwnd);
						continue;
					case Msg.WM_MOUSEMOVE:
						motion_poll = 0;

						drag_data.CurMousePos.X = Control.LowOrder ((int) msg.lParam.ToInt32 ());
						drag_data.CurMousePos.Y = Control.HighOrder ((int) msg.lParam.ToInt32 ());

						HandleMouseOver ();
						// We don't want to dispatch mouse move
						continue;
					}

					XplatUI.DispatchMessage (ref msg);
				}
			}

			timer.Stop ();

			// If the target is a mwf control, return until DragEnter/DragLeave has been fired,
			// which means the respective -already sent- dnd ClientMessages have been received and handled.
			if (control != null)
				Application.DoEvents ();

			if (!dropped)
				return DragDropEffects.None;
			if (drag_event != null)
				return drag_event.Effect;

			// Fallback.
			return DragDropEffects.None;
		}

		private void DndTickHandler (object sender, EventArgs e)
		{
			// This is to make sure we don't get stuck in a loop if another
			// app doesn't finish the DND operation
			if (dropped) {
				Timer t = (Timer) sender;
				if (t.Interval == 500)
					tracking = false;
				else
					t.Interval = 500;
			}


			// If motion_poll is -1, there hasn't been motion at all, so don't simulate motion yet.
			// Otherwise if more than 100 milliseconds have lapsed, we assume the pointer is not
			// in motion anymore, and we simulate the mouse over operation, like .Net does.
			if (motion_poll > 1)
				HandleMouseOver ();
			else if (motion_poll > -1)
				motion_poll++;
		}

		// This routines helps us to have a DndEnter/DndLeave fallback when there wasn't any mouse movement
		// as .Net does
		private void DefaultEnterLeave (object user_data)
		{
			IntPtr toplevel, window;
			int x_root, y_root;

			// The window generating the operation could be a different than the one under pointer
			GetWindowsUnderPointer (out window, out toplevel, out x_root, out y_root);
			Control source_control = Control.FromHandle (window);
			if (source_control == null || !source_control.AllowDrop)
				return;

			// `data' and other members are already available
			Point pos = Control.MousePosition;
			DragEventArgs drag_args = new DragEventArgs (data, 0, pos.X, pos.Y, drag_data.AllowedEffects, DragDropEffects.None);

			source_control.DndEnter (drag_args);
			if ((drag_args.Effect & drag_data.AllowedEffects) != 0)
				source_control.DndDrop (drag_args);
			else
				source_control.DndLeave (EventArgs.Empty);
		}

		public void HandleButtonUpMsg ()
		{
			if (drag_data.State == DragState.Beginning) {
				//state = State.Accepting;
			} else if (drag_data.State != DragState.None) {

				if (drag_data.WillAccept) {

					if (QueryContinue (false, DragAction.Drop))
						return;					
				} else {

					if (QueryContinue (false, DragAction.Cancel))
						return;

					// fallback if no movement was detected, as .net does.
					if (motion_poll == -1)
						DefaultEnterLeave (drag_data.Data);
				}

				drag_data.State = DragState.None;
				// WE can't reset the drag data yet as it is still
				// most likely going to be used by the SelectionRequest
				// handlers
			}

			return;
		}

		private void RemoveCapture (IntPtr handle)
		{
			Control c = MwfWindow (handle);
			if (c.InternalCapture)
				c.InternalCapture = false;
		}

		public bool HandleMouseOver ()
		{
			IntPtr toplevel, window;
			int x_root, y_root;

			GetWindowsUnderPointer (out window, out toplevel, out x_root, out y_root);

			if (window != drag_data.LastWindow && drag_data.State == DragState.Entered) {
				drag_data.State = DragState.Dragging;

				// TODO: Send a Leave if this is an MWF window

				if (toplevel != drag_data.LastTopLevel)
					SendLeave (drag_data.LastTopLevel, toplevel);
			}

			drag_data.State = DragState.Entered;
			if (toplevel != drag_data.LastTopLevel) {
				// Entering a new toplevel window
				SendEnter (toplevel, drag_data.Window, drag_data.SupportedTypes);
			} else {
				// Already in a toplevel window, so send a position
				SendPosition (toplevel, drag_data.Window,
						drag_data.Action,
						x_root,	y_root,
						IntPtr.Zero);
			}

			drag_data.LastTopLevel = toplevel;
			drag_data.LastWindow = window;
			return true;
		}

		void GetWindowsUnderPointer (out IntPtr window, out IntPtr toplevel, out int x_root, out int y_root)
		{
			toplevel = IntPtr.Zero;
			window = XplatUIX11.RootWindowHandle;

			IntPtr root, child;
			bool dnd_aware = false;
			int x_temp, y_temp;
			int mask_return;
			int x = x_root = drag_data.CurMousePos.X;
			int y = y_root = drag_data.CurMousePos.Y;

			while (XplatUIX11.XQueryPointer (display, window, out root, out child,
					       out x_temp, out y_temp, out x, out y, out mask_return)) {
					
				if (!dnd_aware) {
					dnd_aware = IsWindowDndAware (window);
					if (dnd_aware) {
						toplevel = window;
						x_root = x_temp;
						y_root = y_temp;
					}
				}

				if (child == IntPtr.Zero)
					break;
					
				window = child;
			}
		}

		public void HandleKeyMessage (MSG msg)
		{
			if (VirtualKeys.VK_ESCAPE == (VirtualKeys) msg.wParam.ToInt32()) {
				QueryContinue (true, DragAction.Cancel);
			}
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
			if (xevent.ClientMessageEvent.message_type == XdndFinished)
				return HandleFinishedEvent (ref xevent);

			return false;
		}

		public bool HandleSelectionNotifyEvent (ref XEvent xevent)
		{
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
			if (xevent.SelectionRequestEvent.selection != XdndSelection)
				return false;

			MimeHandler handler = FindHandler (xevent.SelectionRequestEvent.target);
			if (handler == null)
				return false;

			handler.Converter.SetData (this, drag_data.Data, ref xevent);

			return true;
		}

		private bool QueryContinue (bool escape, DragAction action)
		{
			QueryContinueDragEventArgs qce = new QueryContinueDragEventArgs ((int) XplatUI.State.ModifierKeys,
					escape, action);

			Control c = MwfWindow (source);
			
			if (c == null) {
				tracking = false;
				return false;
			}
			
			c.DndContinueDrag (qce);

			switch (qce.Action) {
			case DragAction.Continue:
				return true;
			case DragAction.Drop:
				SendDrop (drag_data.LastTopLevel, source, IntPtr.Zero);
				tracking = false;
				return true;
			case DragAction.Cancel:
				drag_data.Reset ();
				c.InternalCapture = false;
				break;
			}

			SendLeave (drag_data.LastTopLevel, toplevel);

			RestoreDefaultCursor ();
			tracking = false;
			return false;
		}

		private void RestoreDefaultCursor ()
		{
			// Releasing the mouse buttons should automatically restore the default cursor,
			// but canceling the operation using QueryContinue should restore it even if the
			// mouse buttons are not released yet.
			XplatUIX11.XChangeActivePointerGrab (display,
					EventMask.ButtonMotionMask |
					EventMask.PointerMotionMask |
					EventMask.ButtonPressMask |
					EventMask.ButtonReleaseMask,
					Cursors.Default.Handle, IntPtr.Zero);

		}

		private void GiveFeedback (IntPtr action)
		{
			GiveFeedbackEventArgs gfe = new GiveFeedbackEventArgs (EffectFromAction (drag_data.Action), true);

			Control c = MwfWindow (source);
			c.DndFeedback (gfe);

			if (gfe.UseDefaultCursors) {
				Cursor cursor = CursorNo;
				if (drag_data.WillAccept) {
					// Same order as on MS
					if (action == XdndActionCopy)
						cursor = CursorCopy;
					else if (action == XdndActionLink)
						cursor = CursorLink;
					else if (action == XdndActionMove)
						cursor = CursorMove;
				}
				// TODO: Try not to set the cursor so much
				//if (cursor.Handle != CurrentCursorHandle) {
				XplatUIX11.XChangeActivePointerGrab (display,
						EventMask.ButtonMotionMask |
						EventMask.PointerMotionMask |
						EventMask.ButtonPressMask |
						EventMask.ButtonReleaseMask,
						cursor.Handle, IntPtr.Zero);
				//CurrentCursorHandle = cursor.Handle;
				//}
			}
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
			sel.SelectionEvent.property = IntPtr.Zero;

			XplatUIX11.XChangeProperty (display, xevent.SelectionRequestEvent.requestor,
					xevent.SelectionRequestEvent.property,
					xevent.SelectionRequestEvent.target,
					8, PropertyMode.Replace, data, length);
			sel.SelectionEvent.property = xevent.SelectionRequestEvent.property;

			XplatUIX11.XSendEvent (display, xevent.SelectionRequestEvent.requestor, false,
					(IntPtr)EventMask.NoEventMask, ref sel);
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
			pos_x = (int) xevent.ClientMessageEvent.ptr3 >> 16;
			pos_y = (int) xevent.ClientMessageEvent.ptr3 & 0xFFFF;

			// Copy is implicitly allowed
			Control source_control = MwfWindow (source);
			if (source_control == null)
				allowed = EffectsFromX11Source (source, xevent.ClientMessageEvent.ptr5) | DragDropEffects.Copy;
			else
				allowed = drag_data.AllowedEffects;

			IntPtr parent, child, new_child, last_drop_child;
			parent = XplatUIX11.XRootWindow (display, 0);
			child = toplevel;
			last_drop_child = IntPtr.Zero;
			while (true) {
				int xd, yd;
				new_child = IntPtr.Zero;
				
				if (!XplatUIX11.XTranslateCoordinates (display,
						    parent, child, pos_x, pos_y,
						    out xd, out yd, out new_child))
					break;
				if (new_child == IntPtr.Zero)
					break;
				child = new_child;

				Hwnd h = Hwnd.ObjectFromHandle (child);
				if (h != null) {
					Control d = Control.FromHandle (h.client_window);
					if (d != null && d.allow_drop)
						last_drop_child = child;
				}
			}

			if (last_drop_child != IntPtr.Zero)
				child = last_drop_child;

			if (target != child) {
				// We have moved into a new control 
				// or into a control for the first time
				Finish ();
			}
			target = child;
			Hwnd hwnd = Hwnd.ObjectFromHandle (target);
			if (hwnd == null)
				return true;

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
				drag_event.x = pos_x;
				drag_event.y = pos_y;
				control.DndOver (drag_event);

				SendStatus (source, drag_event.Effect);
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
				control = null;
			}
			ResetTargetData ();
		}

		private bool Accepting_HandleDropEvent (ref XEvent xevent)
		{
			if (control != null && drag_event != null) {
				drag_event = new DragEventArgs (data,
						0, pos_x, pos_y,
					allowed, drag_event.Effect);
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

				if (!QueryContinue (false, DragAction.Continue))
					return true;

				drag_data.WillAccept = ((int) xevent.ClientMessageEvent.ptr2 & 0x1) != 0;
				
				GiveFeedback (xevent.ClientMessageEvent.ptr5);
			}
			return true;
		}

		private bool HandleFinishedEvent (ref XEvent xevent)
		{
			return true;
		}

		private DragDropEffects EffectsFromX11Source (IntPtr source, IntPtr action_atom)
		{
			DragDropEffects allowed = DragDropEffects.None;
			IntPtr type, count, remaining, data = IntPtr.Zero;
			int format;

			XplatUIX11.XGetWindowProperty (display, source, XdndActionList,
					IntPtr.Zero, new IntPtr (32), false, (IntPtr) Atom.AnyPropertyType,
					out type, out format, out count, out remaining, ref data);

			int intptr_size = Marshal.SizeOf (typeof (IntPtr));
			for (int i = 0; i < count.ToInt32 (); i++) {
				IntPtr current_atom = Marshal.ReadIntPtr (data, i * intptr_size);
				allowed |= EffectFromAction (current_atom);
			}

			// if source is not providing the action list, use the
			// default action passed in the x11 dnd position message
			if (allowed == DragDropEffects.None)
				allowed = EffectFromAction (action_atom);

			return allowed;
		}

		private DragDropEffects EffectFromAction (IntPtr action)
		{
			if (action == XdndActionCopy)
				return DragDropEffects.Copy;
			else if (action == XdndActionMove)
				return DragDropEffects.Move;
			if (action == XdndActionLink)
				return DragDropEffects.Link;

			return DragDropEffects.None;
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

			Control mwfcontrol = MwfWindow (source);

			/* To take advantage of the mwfcontrol, we have to be sure
			   that the dnd operation is still happening (since messages are asynchronous) */
			if (mwfcontrol != null && drag_data != null) {
				if (!tracking)
					return false;

				IDataObject dragged = drag_data.Data as IDataObject;
				if (dragged != null) {
					data = dragged;
				} else {
					if (data == null)
						data = new DataObject ();
					SetDataWithFormats (drag_data.Data);
				}
				return true;
			}

			foreach (IntPtr atom in SourceSupportedList (ref xevent)) {
				MimeHandler handler = FindHandler (atom);
				if (handler == null)
					continue;
				XplatUIX11.XConvertSelection (display, XdndSelection, handler.Type,
					handler.NonProtocol, toplevel, IntPtr.Zero /* CurrentTime */);
				converts_pending++;
				match = true;
			}
			return match;
		}

		private void SetDataWithFormats (object value)
		{
			if (value is string) {
				data.SetData (DataFormats.Text, value);
				data.SetData (DataFormats.UnicodeText, value);
			}

			data.SetData (value);
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
				foreach (string alias in handler.Aliases) {
					if (alias == name)
						return handler;
				}
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
			if (effect != DragDropEffects.None && (effect & allowed) != 0)
				xevent.ClientMessageEvent.ptr2 = (IntPtr) 1;

			xevent.ClientMessageEvent.ptr5 = ActionFromEffect (effect);
			XplatUIX11.XSendEvent (display, source, false, IntPtr.Zero, ref xevent);
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
			xevent.ClientMessageEvent.ptr2 = (IntPtr) ((long)XdndVersion [0] << 24);
			
			if (supported.Length > 0)
				xevent.ClientMessageEvent.ptr3 = supported [0];
			if (supported.Length > 1)
				xevent.ClientMessageEvent.ptr4 = supported [1];
			if (supported.Length > 2)
				xevent.ClientMessageEvent.ptr5 = supported [2];

			XplatUIX11.XSendEvent (display, handle, false, IntPtr.Zero, ref xevent);
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
			
			XplatUIX11.XSendEvent (display, handle, false, IntPtr.Zero, ref xevent);
			dropped = true;
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
			
			XplatUIX11.XSendEvent (display, handle, false, IntPtr.Zero, ref xevent);
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

			XplatUIX11.XSendEvent (display, handle, false, IntPtr.Zero, ref xevent);
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

			XplatUIX11.XSendEvent (display, source, false, IntPtr.Zero, ref xevent);
		}

		// There is a somewhat decent amount of overhead
		// involved in setting up dnd so we do it lazily
		// as a lot of applications do not even use it.
		private void Init ()
		{
			XdndAware = XplatUIX11.XInternAtom (display, "XdndAware", false);
			XdndEnter = XplatUIX11.XInternAtom (display, "XdndEnter", false);
			XdndLeave = XplatUIX11.XInternAtom (display, "XdndLeave", false);
			XdndPosition = XplatUIX11.XInternAtom (display, "XdndPosition", false);
			XdndStatus = XplatUIX11.XInternAtom (display, "XdndStatus", false);
			XdndDrop = XplatUIX11.XInternAtom (display, "XdndDrop", false);
			XdndSelection = XplatUIX11.XInternAtom (display, "XdndSelection", false);
			XdndFinished = XplatUIX11.XInternAtom (display, "XdndFinished", false);
			XdndTypeList = XplatUIX11.XInternAtom (display, "XdndTypeList", false);
			XdndActionCopy = XplatUIX11.XInternAtom (display, "XdndActionCopy", false);
			XdndActionMove = XplatUIX11.XInternAtom (display, "XdndActionMove", false);
			XdndActionLink = XplatUIX11.XInternAtom (display, "XdndActionLink", false);
			//XdndActionPrivate = XplatUIX11.XInternAtom (display, "XdndActionPrivate", false);
			XdndActionList = XplatUIX11.XInternAtom (display, "XdndActionList", false);
			//XdndActionDescription = XplatUIX11.XInternAtom (display, "XdndActionDescription", false);
			//XdndActionAsk = XplatUIX11.XInternAtom (display, "XdndActionAsk", false);

			foreach (MimeHandler handler in MimeHandlers) {
				handler.Type = XplatUIX11.XInternAtom (display, handler.Name, false);
				handler.NonProtocol = XplatUIX11.XInternAtom (display,
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
				int format;
				IntPtr count;
				IntPtr remaining;
				IntPtr data = IntPtr.Zero;

				XplatUIX11.XGetWindowProperty (display, source, XdndTypeList,
						IntPtr.Zero, new IntPtr(32), false, (IntPtr) Atom.XA_ATOM,
						out type, out format, out count,
						out remaining, ref data);

				res = new IntPtr [count.ToInt32()];
				for (int i = 0; i < count.ToInt32(); i++) {
					res [i] = (IntPtr) Marshal.ReadInt32 (data, i *
							Marshal.SizeOf (typeof (int)));
				}

				XplatUIX11.XFree (data);
			}

			return res;
		}

		private string GetText (ref XEvent xevent, bool unicode)
		{
			int nread = 0;
			IntPtr nitems;
			IntPtr bytes_after;

			StringBuilder builder = new StringBuilder ();
			do {
				IntPtr actual_type;
				int actual_fmt;
				IntPtr data = IntPtr.Zero;

				if (0 != XplatUIX11.XGetWindowProperty (display,
						    xevent.AnyEvent.window,
						    (IntPtr) xevent.SelectionEvent.property,
						    IntPtr.Zero, new IntPtr(0xffffff), false,
						    (IntPtr) Atom.AnyPropertyType, out actual_type,
						    out actual_fmt, out nitems, out bytes_after,
						    ref data)) {
					XplatUIX11.XFree (data);
					break;
				}

				if (unicode)
					builder.Append (Marshal.PtrToStringUni (data));
				else
					builder.Append (Marshal.PtrToStringAnsi (data));
				nread += nitems.ToInt32();

				XplatUIX11.XFree (data);
			} while (bytes_after.ToInt32() > 0);
			if (nread == 0)
				return null;
			return builder.ToString ();
		}

		private MemoryStream GetData (ref XEvent xevent)
		{
			int nread = 0;
			IntPtr nitems;
			IntPtr bytes_after;

			MemoryStream res = new MemoryStream ();
			do {
				IntPtr actual_type;
				int actual_fmt;
				IntPtr data = IntPtr.Zero;

				if (0 != XplatUIX11.XGetWindowProperty (display,
						    xevent.AnyEvent.window,
						    (IntPtr) xevent.SelectionEvent.property,
						    IntPtr.Zero, new IntPtr(0xffffff), false,
						    (IntPtr) Atom.AnyPropertyType, out actual_type,
						    out actual_fmt, out nitems, out bytes_after,
						    ref data)) {
					XplatUIX11.XFree (data);
					break;
				}

				for (int i = 0; i < nitems.ToInt32(); i++)
					res.WriteByte (Marshal.ReadByte (data, i));
				nread += nitems.ToInt32();

				XplatUIX11.XFree (data);
			} while (bytes_after.ToInt32() > 0);
			return res;
		}

		private Control MwfWindow (IntPtr window)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (window);
			if (hwnd == null)
				return null;

			Control res = Control.FromHandle (hwnd.client_window);
			
			if (res == null)
				res = Control.FromHandle (window);
				
			return res;
		}

		private bool IsWindowDndAware (IntPtr handle)
		{
			bool res = true;
			// Check the version number, we need greater than 3
			IntPtr actual;
			int format;
			IntPtr count;
			IntPtr remaining;
			IntPtr data = IntPtr.Zero;
			
			XplatUIX11.XGetWindowProperty (display, handle, XdndAware, IntPtr.Zero, new IntPtr(0x8000000), false,
					(IntPtr) Atom.XA_ATOM, out actual, out format,
					out count, out remaining, ref data);
			
			if (actual != (IntPtr) Atom.XA_ATOM || format != 32 ||
					count.ToInt32() == 0 || data == IntPtr.Zero) {
				if (data != IntPtr.Zero)
					XplatUIX11.XFree (data);
				return false;
			}

			int version = Marshal.ReadInt32 (data, 0);

			if (version < 3) {
				Console.Error.WriteLine ("XDND Version too old (" + version + ").");
				XplatUIX11.XFree (data);
				return false;
			}

			// First type is actually the XDND version
			if (count.ToInt32() > 1) {
				res = false;
				for (int i = 1; i < count.ToInt32(); i++) {
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

			XplatUIX11.XFree (data);
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

			IDataObject data_object = data as IDataObject;
			if (data_object != null) {
				foreach (string format in data_object.GetFormats (true)) {
					MimeHandler handler = FindHandler (format);
					if (handler != null && !res.Contains (handler.Type))
						res.Add (handler.Type);
				}
			}

			if (data is ISerializable) {
				MimeHandler handler = FindHandler ("application/x-mono-serialized-object");
				if (handler != null)
					res.Add (handler.Type);
			}

			return (IntPtr []) res.ToArray (typeof (IntPtr));
		}
	}
}
