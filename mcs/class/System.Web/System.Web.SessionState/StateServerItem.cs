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

		private byte [] data;
		//	  private HttpStaticObjectsCollection static_objects;
		private DateTime last_access;
		private int timeout;

		public StateServerItem (byte[] data, int timeout)
		{
			this.data = data;
			this.timeout = timeout;
			this.last_access = DateTime.Now;
		}

		public byte [] Data {
			get { return data; }
			set { data = value; }
		}
		
/*
		internal HttpStaticObjectsCollection StaticObjects {
			get { return static_objects; }
		}
*/		  
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

