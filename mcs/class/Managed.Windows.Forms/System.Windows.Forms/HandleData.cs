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
		private Graphics dc;

		public HandleData ()
		{
		}

		public Graphics DeviceContext {
			get { return dc; }
			set {
				if (dc != null) {
					dc.Dispose ();
				}
				dc = value;
			}
		}

		public Queue MessageQueue {
			get {
				if (message_queue == null)
					message_queue = new Queue ();
				return message_queue;
			}
		}

		public Rectangle InvalidArea {
			get {
				return invalid;
			}
		}

		public void Dispose ()
		{
			DeviceContext = null;
		}

		public void AddToInvalidArea (int x, int y, int width, int height)
		{
			if (invalid == Rectangle.Empty) {
				invalid = new Rectangle (x, y, width, height);
				return;
			}

			if (x < invalid.X)
				invalid.X = x;
			if (y < invalid.Y)
				invalid.Y = y;
			if (height > invalid.Height)
				invalid.Height = height;
			if (width > invalid.Width)
				invalid.Width = width;
		}

		public void AddToInvalidArea (Rectangle r)
		{
			if (invalid == Rectangle.Empty) {
				invalid = r;
				return;
			}

			if (r.X < invalid.X)
				invalid.X = r.X;
			if (r.Y < invalid.Y)
				invalid.Y = r.Y;
			if (r.Height > invalid.Height)
				invalid.Height = r.Height;
			if (r.Width > invalid.Width)
				invalid.Width = r.Width;
		}

		public void ClearInvalidArea ()
		{
			invalid = Rectangle.Empty;
		}
	}
}

