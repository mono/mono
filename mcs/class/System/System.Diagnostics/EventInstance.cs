//
// System.Diagnostics.EventInstance
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Novell
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

using System.ComponentModel;

namespace System.Diagnostics
{
	public class EventInstance
	{
		int _categoryId;
		EventLogEntryType _entryType;
		long _instanceId;

		public EventInstance (long instanceId, int categoryId)
			: this (instanceId, categoryId, EventLogEntryType.Information)
		{
		}

		public EventInstance (long instanceId, int categoryId, EventLogEntryType entryType)
		{
			InstanceId = instanceId;
			CategoryId = categoryId;
			EntryType = entryType;
		}

		public int CategoryId
		{
			get { return _categoryId; }
			set {
				if (value < 0 || value > ushort.MaxValue)
					throw new ArgumentOutOfRangeException ("value");
				_categoryId = value; 
			}
		}

		public EventLogEntryType EntryType
		{
			get { return _entryType; }
			set {
				if (!Enum.IsDefined (typeof (EventLogEntryType), value))
					throw new InvalidEnumArgumentException("value", (int) value,
						typeof(EventLogEntryType));
				_entryType = value; 
			}
		}

		public long InstanceId
		{
			get { return _instanceId; }
			set {
				if (value < 0 || value > uint.MaxValue)
					throw new ArgumentOutOfRangeException ("value");
				_instanceId = value;
			}
		}
	}
}
