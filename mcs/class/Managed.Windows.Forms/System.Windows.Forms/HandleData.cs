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
//	Jackson Harper (jackson@ximian.com)
//
//


using System;
using System.Drawing;
using System.Collections;

namespace System.Windows.Forms {

	internal class HandleData : IDisposable {

		private Queue message_queue;
		private Rectangle invalid = Rectangle.Empty;
		private Object dc;

		#region Constructors and destructors
		public HandleData ()
		{
		}

		public void Dispose () {
			DeviceContext = null;
		}
		#endregion

		#region Paint area handling
		public Object DeviceContext {
			get { return dc; }
			set {
				if (value is Graphics) {
					if (dc != null) {
						((Graphics)dc).Dispose ();
					}
				}
				dc = value;
			}
		}

		public Rectangle InvalidArea {
			get {
				return invalid;
			}
		}
		public void AddToInvalidArea (int x, int y, int width, int height)
		{
			if (invalid == Rectangle.Empty) {
				invalid = new Rectangle (x, y, width, height);
				return;
			}
			invalid = Rectangle.Union (invalid, new Rectangle (x, y, width, height));
		}

		public void AddToInvalidArea (Rectangle r)
		{
			if (invalid == Rectangle.Empty) {
				invalid = r;
				return;
			}
			invalid = Rectangle.Union (invalid, r);
		}

		public void ClearInvalidArea ()
		{
			invalid = Rectangle.Empty;
		}
		#endregion	// Paint area methods

		#region Message storage and retrieval
		public bool MessageWaiting {
			get {
				if ((message_queue == null) || (message_queue.Count == 0)) {
					return false;
				}
				return true;
			}
		}

		public bool GetMessage(ref MSG msg) {
			MSG	message;

			if ((message_queue == null) || (message_queue.Count == 0)) {
				return false;
			}

			message = (MSG)message_queue.Dequeue();
			msg = message;

			return true;
		}

		public bool StoreMessage(ref MSG msg) {
			MSG message = new MSG();

			if (message_queue == null) {
				message_queue = new Queue();
			}

			message = msg;
			message_queue.Enqueue(message);

			return true;
		}
		#endregion	// Message storage and retrieval
	}
}

