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
// $Revision: 1.2 $
// $Modtime: $
// $Log: ScrollEventArgs.cs,v $
// Revision 1.2  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// COMPLETE

namespace System.Windows.Forms {
	public class ScrollEventArgs : EventArgs {
		#region Local Variables
		private ScrollEventType	type;
		private int		new_value;
		#endregion

		#region Public Constructors
		public ScrollEventArgs(ScrollEventType type, int newValue) {
			this.type = type;
			this.new_value = newValue;
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

		public ScrollEventType Type {
			get {
				return type;
			}
		}
		#endregion	// Public Instance Properties
	}
}
