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


// COMPLETE

namespace System.Windows.Forms {
	public class QueryContinueDragEventArgs : EventArgs {
		private int		key_state;
		private bool		escape_pressed;
		private DragAction	drag_action;

		#region Public Constructors
		public QueryContinueDragEventArgs(int keyState, bool escapePressed, DragAction action) {
			this.key_state=keyState;
			this.escape_pressed=escapePressed;
			this.drag_action=action;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public DragAction Action {
			get {
				return this.drag_action;
			}

			set {
				this.drag_action=value;
			}
		}

		public bool EscapePressed {
			get {
				return this.escape_pressed;
			}
		}

		public int KeyState {
			get {
				return this.key_state;
			}
		}
		#endregion	// Public Instance Properties
	}
}
