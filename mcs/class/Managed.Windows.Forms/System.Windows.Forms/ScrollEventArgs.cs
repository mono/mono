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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// COMPLETE

using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[ComVisible(true)]
	public class ScrollEventArgs : EventArgs {
		#region Local Variables
		private ScrollEventType	type;
		private int		new_value;
		private int		old_value;
		private ScrollOrientation	scroll_orientation;
		#endregion

		#region Public Constructors
		public ScrollEventArgs(ScrollEventType type, int newValue) :
			this (type, -1, newValue, ScrollOrientation.HorizontalScroll)
		{
		}

		public ScrollEventArgs (ScrollEventType type, int oldValue, int newValue) :
			this (type, oldValue, newValue, ScrollOrientation.HorizontalScroll)
		{
		}

		public ScrollEventArgs (ScrollEventType type, int newValue, ScrollOrientation scroll) :
			this (type, -1, newValue, scroll)
		{
		}


		public ScrollEventArgs (ScrollEventType type, int oldValue, int newValue, ScrollOrientation scroll)
		{
			this.new_value = newValue;
			this.old_value = oldValue;
			this.scroll_orientation = scroll;
			this.type = type;
		}
		
		#endregion	// Public Constructors

		#region Public Instance Properties
		public int NewValue {
			get {
				return new_value;
			}

			set {
				new_value = value;
			}
		}

		public int OldValue {
			get { return old_value; }
		}

		public ScrollOrientation ScrollOrientation {
			get { return scroll_orientation; }
		}
		
		public ScrollEventType Type {
			get {
				return type;
			}
		}
		#endregion	// Public Instance Properties
	}
}
