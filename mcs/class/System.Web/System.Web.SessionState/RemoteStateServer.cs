//
// System.Web.SessionState.RemoteStateServer
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//


using System;
using System.Collections;

namespace System.Web.SessionState {

	internal class RemoteStateServer : MarshalByRefObject {
		
		private Hashtable table;
		
		internal RemoteStateServer ()
		{
			table = new Hashtable ();
		}
		
		internal void Insert (string id, StateServerItem item)
		{
			table.Add (id, item);
		}

		internal void Update (string id, byte [] dict_data, byte [] sobjs_data)
		{
			StateServerItem item = table [id] as StateServerItem;

			if (item == null)
				return;

			item.DictionaryData = dict_data;
			item.StaticObjectsData = sobjs_data;
			item.Touch ();
		}
		
		internal StateServerItem Get (string id)
		{
			StateServerItem item = table [id] as StateServerItem;

			if (item == null || item.IsAbandoned ())
				return null;

			item.Touch ();
			return item;
		}
	}
}

