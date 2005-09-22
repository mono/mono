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

namespace System.Windows.Forms {
	public class UICuesEventArgs : EventArgs {
		private UICues	cues;

		#region Public Constructors
		public UICuesEventArgs(UICues uicues) {
			this.cues = uicues;
		}
		#endregion

		#region Public Instance Properties
		public UICues Changed {
			get {
				return (cues & UICues.Changed);
			}
		}

		public bool ChangeFocus {
			get {
				if ((cues & UICues.ChangeFocus)==0) {
					return false;
				} else {
					return true;
				}
			}
		}

		public bool ChangeKeyboard {
			get {
				if ((cues & UICues.ChangeKeyboard)==0) {
					return false;
				} else {
					return true;
				}
			}
		}

		public bool ShowFocus {
			get {
				if ((cues & UICues.ShowFocus)==0) {
					return false;
				} else {
					return true;
				}
			}
		}

		public bool ShowKeyboard {
			get {
				if ((cues & UICues.ShowKeyboard)==0) {
					return false;
				} else {
					return true;
				}
			}
		}
		#endregion	// Public Instance Properties
	}
}
