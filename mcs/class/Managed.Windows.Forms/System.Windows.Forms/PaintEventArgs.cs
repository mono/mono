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


// COMPLETE

using System.Drawing;

namespace System.Windows.Forms {
	public class PaintEventArgs : EventArgs {
		private Graphics	graphics;
		private Rectangle	clip_rectangle;

		#region Public Constructors
		public PaintEventArgs(System.Drawing.Graphics graphics, System.Drawing.Rectangle clipRect) {
			this.graphics=graphics;
			this.clip_rectangle=clipRect;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public Rectangle ClipRectangle {
			get {
				return this.clip_rectangle;
			}
		}

		public Graphics Graphics {
			get {
				return this.graphics;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		~PaintEventArgs() {
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing) {
			if (this.graphics!=null) {
				graphics.Dispose();
				this.graphics=null;
			}
		}
		#endregion	// Protected Instance Methods
	}
}
