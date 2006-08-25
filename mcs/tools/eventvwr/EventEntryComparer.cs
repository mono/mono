//
// EventLogEntryComparer.cs
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;

namespace Mono.Tools.EventViewer
{
	public class EventLogEntryComparer : IComparer
	{
		private int _column;

		public EventLogEntryComparer (int column)
		{
			_column = column;
		}

		public int Column
		{
			get { return _column; }
			set { _column = value; }
		}

		public int Compare (object x, object y)
		{
			ListViewItem itemX = x as ListViewItem;
			ListViewItem itemY = y as ListViewItem;

			if (itemX.ListView.Sorting == SortOrder.None)
				return 0;

			int sortFlag = (itemX.ListView.Sorting == SortOrder.Descending) ? -1 : 1;

			if (itemX == null)
				return -1 * sortFlag;
			if (itemY == null)
				return 1 * sortFlag;

			int retVal = 0;

			switch (_column) {
			case 1:
			case 2:
				DateTime dateX = DateTime.Parse (itemX.SubItems [_column].Text,
					CultureInfo.CurrentCulture);
				DateTime dateY = DateTime.Parse (itemY.SubItems [_column].Text,
					CultureInfo.CurrentCulture);
				if (_column == 1)
					retVal = DateTime.Compare (dateX, dateY);
				else
					retVal = TimeSpan.Compare (dateX.TimeOfDay, dateY.TimeOfDay);
				break;
			default:
				retVal = string.Compare (itemX.SubItems [_column].Text,
						itemY.SubItems [_column].Text);
				break;
			}

			return retVal * sortFlag;
		}

	}
}
