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
// Copyright (c) 2021 Thomas Kuehne
//
// Authors:
//	Thomas Kuehne	thomas@kuehne.cn
//

namespace System.Windows.Forms {

	//
	// see https://tronche.com/gui/x/icccm/sec-2.html for X11's selection handling
	//
	internal abstract class X11Selection {
		internal enum ID {
			CLIPBOARD = 0,
			PRIMARY = 1,
			XdndSelection = 2
		}

		internal static IntPtr ATOM_PAIR;
		internal static IntPtr CLIPBOARD;
		internal static IntPtr CLIPBOARD_MANAGER;
		internal static IntPtr DELETE;
		internal static IntPtr MULTIPLE;
		internal static IntPtr SAVE_TARGETS;
		internal static IntPtr TARGETS;
		internal static IntPtr XdndAware;
		internal static IntPtr XdndEnter;
		internal static IntPtr XdndLeave;
		internal static IntPtr XdndPosition;
		internal static IntPtr XdndDrop;
		internal static IntPtr XdndFinished;
		internal static IntPtr XdndStatus;
		internal static IntPtr XdndTypeList;
		internal static IntPtr XdndActionCopy;
		internal static IntPtr XdndActionMove;
		internal static IntPtr XdndActionLink;
		internal static IntPtr XdndActionList;

		internal readonly IntPtr Selection;
		internal readonly ID SelectionName;

		// Incomming and Outgoing have to be 2 seperate Objects for
		// Clipboard but are identical for DND
		protected IDataObject Incomming {get; set;}
		protected virtual IDataObject Outgoing {get; set;}

		protected int ConvertsPending;

		internal X11Selection (ID selection)
		{
			SelectionName = selection;
			Selection = XplatUIX11.XInternAtom (XplatUIX11.Display, selection.ToString(), false);

			if (XdndTypeList == IntPtr.Zero){
				var names = new []{
					nameof(ATOM_PAIR),
					nameof(CLIPBOARD),
					nameof(CLIPBOARD_MANAGER),
					nameof(DELETE),
					nameof(MULTIPLE),
					nameof(SAVE_TARGETS),
					nameof(TARGETS),
					nameof(XdndActionCopy),
					nameof(XdndActionLink),
					nameof(XdndActionList),
					nameof(XdndActionMove),
					nameof(XdndAware),
					nameof(XdndDrop),
					nameof(XdndEnter),
					nameof(XdndFinished),
					nameof(XdndLeave),
					nameof(XdndPosition),
					nameof(XdndStatus),
					nameof(XdndTypeList)
				};

				var atoms = new IntPtr [names.Length];;

				XplatUIX11.XInternAtoms (XplatUIX11.Display, names, names.Length, false, atoms);

				int pos = 0;
				ATOM_PAIR = atoms[pos++];
				CLIPBOARD = atoms[pos++];
				CLIPBOARD_MANAGER = atoms[pos++];
				DELETE = atoms[pos++];
				MULTIPLE = atoms[pos++];
				SAVE_TARGETS = atoms[pos++];
				TARGETS = atoms[pos++];
				XdndActionCopy = atoms[pos++];
				XdndActionLink = atoms[pos++];
				XdndActionList = atoms[pos++];
				XdndActionMove = atoms[pos++];
				XdndAware = atoms[pos++];
				XdndDrop = atoms[pos++];
				XdndEnter = atoms[pos++];
				XdndFinished = atoms[pos++];
				XdndLeave = atoms[pos++];
				XdndPosition = atoms[pos++];
				XdndStatus = atoms[pos++];
				XdndTypeList = atoms[pos++];
			}

			X11SelectionHandler.Init();
		}

		internal virtual void HandleSelectionRequestEvent (ref XEvent xevent)
		{

			if (xevent.SelectionRequestEvent.target == DELETE) {
				// we are only clearing the buffer and not actualy "deleting" the content
				// there doesn't seem to be any way to ask a dotnet application to "delete"
				XplatUIX11.XSetSelectionOwner (XplatUIX11.Display,
						xevent.SelectionRequestEvent.selection,
						IntPtr.Zero,
						xevent.SelectionRequestEvent.time);
				Outgoing = null;
				X11SelectionHandler.SetEmpty (ref xevent);
			} else {
				X11SelectionHandler handler = X11SelectionHandler.Find (xevent.SelectionRequestEvent.target);
				if (handler == null) {
					X11SelectionHandler.SetUnsupported (ref xevent);
				} else {
					handler.SetData (ref xevent, Outgoing);
				}
			}
		}

		internal virtual void HandleSelectionNotifyEvent (ref XEvent xevent)
		{
			ConvertsPending--;

			// we requested something the source right now doesn't support or there is no source
			if (xevent.SelectionEvent.property == IntPtr.Zero)
				return;

			try{
				X11SelectionHandler handler = X11SelectionHandler.Find ((IntPtr) xevent.SelectionEvent.target);
				if (handler == null)
					return;

				if (Incomming == null)
					Incomming = new DataObject ();

				handler.GetData (ref xevent, Incomming);
			} finally {
				XplatUIX11.XDeleteProperty (xevent.AnyEvent.display,
					xevent.AnyEvent.window, xevent.SelectionEvent.property);
			}
		}

		internal virtual void HandleSelectionClearEvent (ref XEvent xevent) {
			Outgoing = null;
			X11SelectionHandler.FreeNativeSelectionBuffers(Selection);
		}
	}
}

