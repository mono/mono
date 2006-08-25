//
// EventEntryView.cs
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
using System.Diagnostics;
using System.Windows.Forms;

namespace Mono.Tools.EventViewer
{
	public class EventEntryView
	{
		readonly EventLogEntry _entry;

		public EventEntryView (EventLogEntry entry)
		{
			_entry = entry;
		}

		public string EntryType {
			get {
				EventLogEntryType entryType = _entry.EntryType;
				if ((int) entryType == 0)
					entryType = EventLogEntryType.Information;
				return entryType.ToString ();
			}
		}

		public string DateGenerated {
			get {
				return _entry.TimeGenerated.ToShortDateString ();
			}
		}

		public string TimeGenerated {
			get {
				return _entry.TimeGenerated.ToLongTimeString ();
			}
		}

		public string Message {
			get {
				return _entry.Message;
			}
		}

		public string Source {
			get {
				return _entry.Source;
			}
		}

		public string Category {
			get {
				int categoryNumber = _entry.CategoryNumber;
				return (categoryNumber == 0) ? "None" : _entry.Category;
			}
		}

		public byte [] Data {
			get {
				return _entry.Data;
			}
		}

		public string InstanceId {
			get {
#if NET_2_0
				return _entry.InstanceId.ToString ();
#else
				return _entry.EventID.ToString ();
#endif
			}
		}

		public string UserName {
			get {
				string userName = _entry.UserName;
				return (userName == null) ? "N/A" : userName;
			}
		}

		public string MachineName {
			get {
				return _entry.MachineName;
			}
		}

		public ListViewItem ListViewItem {
			get {
				ListViewItem item = new ListViewItem (EntryType, ImageIndex);
				item.SubItems.Add (DateGenerated);
				item.SubItems.Add (TimeGenerated);
				item.SubItems.Add (Source);
				item.SubItems.Add (Category);
				item.SubItems.Add (InstanceId);
				item.SubItems.Add (UserName);
				item.SubItems.Add (MachineName);
				item.Tag = this;
				return item;
			}
		}

		private int ImageIndex
		{
			get
			{
				switch (_entry.EntryType) {
				case EventLogEntryType.Error:
					return 0;
				case EventLogEntryType.FailureAudit:
					return 6;
				case EventLogEntryType.Information:
					return 1;
				case EventLogEntryType.SuccessAudit:
					return 5;
				case EventLogEntryType.Warning:
					return 2;
				default:
					return 1;
				}
			}
		}
	}
}
