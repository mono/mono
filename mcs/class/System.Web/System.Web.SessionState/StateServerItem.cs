//
// System.Web.SessionState.StateServerItem
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
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
#if !NET_2_0
using System;

namespace System.Web.SessionState {

	[Serializable]
	class StateServerItem {

		byte [] dict_data;
		byte [] sobjs_data;
		DateTime last_access;
		int timeout;

		public StateServerItem (byte [] dict_data, byte [] sobjs_data, int timeout)
		{
			this.dict_data = dict_data;
			this.sobjs_data = sobjs_data;
			this.timeout = timeout;
			this.last_access = DateTime.UtcNow;
		}

		public byte [] DictionaryData {
			get { return dict_data; }
			set { dict_data = value; }
		}

		public byte [] StaticObjectsData {
			get { return sobjs_data; }
			set { sobjs_data = value; }
		}
		
		public void Touch ()
		{
			last_access = DateTime.UtcNow;
		}

		public bool IsAbandoned () {
			if (last_access.AddMinutes (timeout) < DateTime.UtcNow)
				return true;
			return false;
		}

		public int Timeout {
			get { return timeout; }
			set { timeout = value; }
		}
	}
}
#endif
