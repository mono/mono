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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	John BouAntoun	jba-mono@optusnet.com.au
//

using System.ComponentModel;

namespace System.Windows.Forms {
	[TypeConverter(typeof(SelectionRangeConverter))]
	public sealed class SelectionRange {
		#region local members 
	
		DateTime end;
		DateTime start;	

		#endregion // local members 

		#region public constructors

		// default parameterless construcor, use default values
		public SelectionRange () {
			end = DateTime.MaxValue.Date;
			start = DateTime.MinValue.Date;
		}	
	
		// constructor that receives another range, copies it's Start and End values
		public SelectionRange (SelectionRange range) {
			end = range.End;
			start = range.Start;
		}

		// constructor that receives two dates, uses the lower of the two as start
		public SelectionRange (DateTime lower, DateTime upper) {
			if (lower <= upper) {
				end = upper.Date;
				start = lower.Date;
			} else {
				end = lower.Date;
				start = upper.Date;
			}
		}

		#endregion // public constructors

		#region public properties

		// end date of this range
		public DateTime End {
			set {
				if (end != value) {
					end = value;
				}
			}
			get {
				return end;
			}
		}

		// start date of this range
		public DateTime Start {
			set {
				if (start != value) {
					start = value;
				}
			}
			get {
				return start;
			}
		}
			
		#endregion // public properties

		#region public methods

		public override string ToString() {
			return "SelectionRange: Start: " + Start.ToString() + ", End: " + End.ToString();
		}

		#endregion // public methods
	}
}
