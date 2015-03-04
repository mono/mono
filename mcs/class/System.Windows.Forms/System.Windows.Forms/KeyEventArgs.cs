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
//


// COMPLETE

using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[ComVisible(true)]
	public class KeyEventArgs : EventArgs {
		private Keys	key_data;
		private bool	event_handled;
		private bool	supress_key_press;

		#region Public Constructors
		public KeyEventArgs(Keys keyData) {
			this.key_data=keyData;
			this.event_handled=false;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public virtual bool Alt {
			get {
				if ((this.key_data & Keys.Alt)==0) {
					return false;
				} else {
					return true;
				}
			}
		}

		public bool Control {
			get {
				if ((this.key_data & Keys.Control)==0) {
					return false;
				} else {
					return true;
				}
			}
		}

		public bool Handled {
			get {
				return this.event_handled;
			}

			set {
				this.event_handled=value;
			}
		}

		public Keys KeyCode {
			get {
				return (this.key_data & Keys.KeyCode);
			}
		}

		public Keys KeyData {
			get {
				return this.key_data;
			}
		}

		public int KeyValue {
			get {
				return Convert.ToInt32(this.key_data);
			}
		}

		public Keys Modifiers {
			get {
				return (this.key_data & Keys.Modifiers);
			}
		}

		public virtual bool Shift {
			get {
				if ((this.key_data & Keys.Shift)==0) {
					return false;
				} else {
					return true;
				}
			}
		}
		
		public bool SuppressKeyPress {
			get {
				return supress_key_press;
			}
			set {
				supress_key_press = value;
				event_handled = value;
			}
		}
		#endregion	// Public Instance Properties
	}
}
