//
// System.Web.SessionState.StateServerItem
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace System.Web.SessionState {

	[Serializable]
	public class StateServerItem {

		private byte [] dict_data;
		private byte [] sobjs_data;
		private DateTime last_access;
		private int timeout;

		public StateServerItem (byte [] dict_data, byte [] sobjs_data, int timeout)
		{
			this.dict_data = dict_data;
			this.sobjs_data = sobjs_data;
			this.timeout = timeout;
			this.last_access = DateTime.Now;
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
			last_access = DateTime.Now;
		}

		public bool IsAbandoned () {
			if (last_access.AddMinutes (timeout) < DateTime.Now)
				return true;
			return false;
		}
	}
}

