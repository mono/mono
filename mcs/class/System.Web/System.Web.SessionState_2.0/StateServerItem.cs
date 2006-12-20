//
// System.Web.SessionState.StateServerItem
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//  Marek Habersack (grendello@gmail.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
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
#if NET_2_0
using System;

namespace System.Web.SessionState {
	[Serializable]
	internal class StateServerItem {
		public byte [] CollectionData;
		public byte [] StaticObjectsData;
		DateTime last_access;
		public int Timeout;
		public Int32 LockId;
		public bool Locked;
		public DateTime LockedTime;
		public SessionStateActions Action;

		public StateServerItem (int timeout) : this (null, null, timeout)
		{
		}
		
		public StateServerItem (byte [] collection_data, byte [] sobjs_data, int timeout)
		{
			this.CollectionData = collection_data;
			this.StaticObjectsData = sobjs_data;
			this.Timeout = timeout;
			this.last_access = DateTime.UtcNow;
			this.Locked = false;
			this.LockId = Int32.MinValue;
			this.LockedTime = DateTime.MinValue;
			this.Action = SessionStateActions.None;
		}
		
		public void Touch ()
		{
			last_access = DateTime.UtcNow;
		}

		public bool IsAbandoned () {
			if (last_access.AddMinutes (Timeout) < DateTime.UtcNow)
				return true;
			return false;
		}
	}
}
#endif
