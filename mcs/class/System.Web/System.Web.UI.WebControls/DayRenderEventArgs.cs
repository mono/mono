
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
/**
 * Namespace: System.Web.UI.WebControls
 * Delegate:  DayRenderEventArgs
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 */

namespace System.Web.UI.WebControls
{
	public sealed class DayRenderEventArgs
	{
		private TableCell   cell;
		private CalendarDay day;
		
		public DayRenderEventArgs(TableCell cell, CalendarDay day)
		{
			this.cell = cell;
			this.day  = day;
		}
		
#if NET_2_0

		string selectUrl;
		
		public DayRenderEventArgs(TableCell cell, CalendarDay day, string selectUrl)
		{
			this.cell = cell;
			this.day  = day;
			this.selectUrl = selectUrl;
		}
		
		public string SelectUrl {
			get { return selectUrl; }
		}
		
#endif
		
		public TableCell Cell
		{
			get
			{
				return cell;
			}
		}
		
		public CalendarDay Day
		{
			get
			{
				return day;
			}
		}
	}
}
