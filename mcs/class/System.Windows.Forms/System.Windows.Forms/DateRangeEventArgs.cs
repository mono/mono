//
// System.Windows.Forms.DateRangeEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Implemented by Richard Baumann <biochem333@nyc.rr.com>
//   Dennis Hayes (dennish@Raytek.com)
//	 Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//

//
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

namespace System.Windows.Forms {

	/// <summary>
	///	Provides data for the DateChanged or DateSelected events of the MonthCalendar control.
	/// </summary>
	public class DateRangeEventArgs : EventArgs {

		#region Fields

		private DateTime end;
		private DateTime start;
		
		#endregion

		//
		//  --- Constructors/Destructors
		//
		public DateRangeEventArgs(DateTime start, DateTime end) : base()
		{
			this.start = start;
			this.end = end;
		}

		#region Public Properties

		public DateTime End 
		{
			get { 
					return end; 
			}
		}

		public DateTime Start {
			get { 
					return start; 
			}
		}

		#endregion
	}
}
