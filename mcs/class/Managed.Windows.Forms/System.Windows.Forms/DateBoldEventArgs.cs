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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// COMPLETE

namespace System.Windows.Forms {
	public class DateBoldEventArgs : EventArgs {
		#region Local Variables
		private int		size;
		private DateTime	start;
		private int[]		days_to_bold;
		#endregion	// Local Variables

		#region Internal Constructor
		DateBoldEventArgs(DateTime start, int size, int[] daysToBold) {
			this.start = start;
			this.size = size;
			this.days_to_bold = daysToBold;
		}
		#endregion

		#region Public Instance Properties
		public int[] DaysToBold {
			get {
				return days_to_bold;
			}

			set {
				days_to_bold = value;
			}
		}

		public int Size {
			get {
				return size;
			}
		}

		public DateTime StartDate {
			get {
				return start;
			}
		}
		#endregion	// Public Instance Properties
	}
}
