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
// Copyright (c) 2004-2006 Novell, Inc.
// Copyright (c) 2009 Novell, Inc.
// Copyright (c) 2021 Thomas Kuehne
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//	Thomas Kuehne	thomas@kuehne.cn
//

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System;

namespace System.Windows.Forms {

	internal sealed class X11Clipboard : X11Selection {
		internal delegate void UpdateMessageQueueDG (XEventQueue queue, bool allowIdle);

		readonly IntPtr FosterParent;
		readonly UpdateMessageQueueDG UpdateMessageQueue;
		readonly TimeSpan TimeToWaitForSelectionFormats;
		readonly TimeSpan TimeToWaitForSelectionData;

		bool FetchFormats;
		string[] Formats;

		bool FetchHandlers;
		X11SelectionHandler[] Handlers;
		X11SelectionHandler HandlerMultiple;

		internal X11Clipboard (bool primary, UpdateMessageQueueDG updateMessageQueue, IntPtr fosterParent,
				int formatTimeout, int dataTimeout)
			: base (primary ? X11Selection.ID.PRIMARY : X11Selection.ID.CLIPBOARD)
		{
			UpdateMessageQueue = updateMessageQueue;
			FosterParent = fosterParent;

			TimeToWaitForSelectionFormats = TimeSpan.FromSeconds(formatTimeout);
			TimeToWaitForSelectionData = TimeSpan.FromSeconds(dataTimeout);
		}

		internal override void HandleSelectionNotifyEvent (ref XEvent xevent)
		{
			if (xevent.SelectionEvent.target == TARGETS) {
				if (! (FetchFormats || FetchHandlers)) {
					// should never happen but ...
					ConvertsPending--;
				}

				if (xevent.SelectionEvent.property != IntPtr.Zero) {
					try {
						var fake_dnd = new XClientMessageEvent();
						fake_dnd.ptr2 = (IntPtr) 1; // use window property

						if (FetchFormats) {
							Formats = X11SelectionHandler.TypeListConvert(xevent.AnyEvent.display,
								    xevent.AnyEvent.window,
								    xevent.SelectionEvent.property, ref fake_dnd);
							FetchFormats = false;
						}
						if (FetchHandlers) {
							Handlers = X11SelectionHandler.TypeListHandlers(xevent.AnyEvent.display,
								    xevent.AnyEvent.window, xevent.SelectionEvent.property,
								    ref fake_dnd, out HandlerMultiple);
							FetchHandlers = false;
						}

					} finally {
						XplatUIX11.XDeleteProperty (xevent.AnyEvent.display,
								xevent.AnyEvent.window, xevent.SelectionEvent.property);
					}
				} else {
					FetchFormats = false;
					FetchHandlers = false;
				}
			} else {
				base.HandleSelectionNotifyEvent (ref xevent);
			}
		}

		private void RequestTargets () {
			var handler = X11SelectionHandler.Find (TARGETS);
			handler.ConvertSelection(XplatUIX11.Display, Selection, FosterParent);
		}

		internal string[] GetFormats () {
			if (Outgoing != null) {
				// short circuit from Mono - to mono
				return Outgoing.GetFormats();
			}

			Formats = null;
			FetchFormats = true;

			RequestTargets();

			var startTime = DateTime.UtcNow;
			while (FetchFormats) {
				UpdateMessageQueue(null, false);

				if (DateTime.UtcNow - startTime > TimeToWaitForSelectionFormats)
					break;
			}
			FetchFormats = false;

			return Formats ?? new string[0];
		}

		internal void Clear () {
			XplatUIX11.XSetSelectionOwner (XplatUIX11.Display, Selection, IntPtr.Zero, IntPtr.Zero);
		}

		internal IDataObject GetContent () {
			if (Outgoing != null) {
				// short circuit within Mono runtime
				return Outgoing;
			}

			Incomming = new DataObject();

			if (IntPtr.Zero == XplatUIX11.XGetSelectionOwner (XplatUIX11.Display, Selection)) {
				// short circuit if no selection owner
				return Incomming;
			}

			// request formats
			Handlers = null;
			HandlerMultiple = null;
			FetchHandlers = true;

			RequestTargets();

			var startTime = DateTime.UtcNow;
			while (FetchHandlers) {
				UpdateMessageQueue(null, false);

				if (DateTime.UtcNow - startTime > TimeToWaitForSelectionFormats)
					break;
			}
			FetchHandlers = false;

			// request data
			var handlers = Handlers;
			var handlerMultiple = HandlerMultiple;
			ConvertsPending = 0;

			if (null != handlers && 0 < handlers.Length) {
				if (null != handlerMultiple && 1 < handlers.Length) {
					handlerMultiple.ConvertSelection(XplatUIX11.Display, Selection, FosterParent, handlers);
					ConvertsPending++;
				} else {
					foreach (var handler in handlers){
						handler.ConvertSelection(XplatUIX11.Display, Selection, FosterParent);
						ConvertsPending++;
					}
				}
			}

			// wait for data
			startTime = DateTime.UtcNow;
			while (0 < ConvertsPending) {
				UpdateMessageQueue(null, false);

				if (DateTime.UtcNow - startTime > TimeToWaitForSelectionData)
					break;
			}

			return Incomming;
		}

		internal void SetContent (object data, bool copy) {
			var iData = data as IDataObject;
			if (iData == null) {
				Outgoing = X11SelectionHandler.SetDataWithFormats (data);
			} else {
				Outgoing = iData;
			}

			// always set owner - even if already the owner
			// -> supports XFIXES' selection tracking
			int success = XplatUIX11.XSetSelectionOwner (XplatUIX11.Display, Selection, FosterParent, IntPtr.Zero);

			if (success == 0 || FosterParent != XplatUIX11.XGetSelectionOwner (XplatUIX11.Display, Selection)) {
				Outgoing = null;
				throw new Exception($"failed to aquire ownership of X11 selection {SelectionName}");
			}

			if (copy) {
				// see https://freedesktop.org/wiki/ClipboardManager/ for details
				//
				// persist clipboard content imediately
				var manager = XplatUIX11.XGetSelectionOwner (XplatUIX11.Display, CLIPBOARD_MANAGER);
				if (manager != IntPtr.Zero) {
					XplatUIX11.XConvertSelection (XplatUIX11.Display, CLIPBOARD_MANAGER, SAVE_TARGETS,
						IntPtr.Zero, FosterParent, IntPtr.Zero);
				}
			}
		}
	}
}

