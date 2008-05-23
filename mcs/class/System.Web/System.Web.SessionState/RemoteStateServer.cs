//
// System.Web.SessionState.RemoteStateServer
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//  Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2003-2006 Novell, Inc (http://www.novell.com)
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
using System.Web.Caching;

namespace System.Web.SessionState {
	internal class RemoteStateServer : MarshalByRefObject {
		Cache cache;
		//CacheItemRemovedCallback removedCB;

		internal RemoteStateServer ()
		{
			cache = new Cache ();
			//removedCB = new CacheItemRemovedCallback (OnItemRemoved);
		}

		/*
		void OnItemRemoved (string key, object value, CacheItemRemovedReason reason)
		{
			Console.WriteLine ("{2} {0} removed. Reason: {1}", key, reason, Datetime.Now);
		}
		*/

		internal void Insert (string id, StateServerItem item)
		{
			//cache.Insert (id, item, null, Cache.NoAbsoluteExpiration, new TimeSpan (0, item.Timeout, 0), CacheItemPriority.Normal, removedCB);
			cache.Insert (id, item, null, Cache.NoAbsoluteExpiration, new TimeSpan (0, item.Timeout, 0));
		}

		internal void Update (string id, byte [] dict_data, byte [] sobjs_data)
		{
			StateServerItem item = cache [id] as StateServerItem;
			if (item == null)
				return;

			item.DictionaryData = dict_data;
			item.StaticObjectsData = sobjs_data;
		}

		internal void Touch (string id, int timeout)
		{
			StateServerItem item = Get (id);
			if (item == null)
				return;
			item.Timeout = timeout;
			cache.SetItemTimeout (id, Cache.NoAbsoluteExpiration, new TimeSpan (0, item.Timeout, 0), false);
		}
		
		internal StateServerItem Get (string id)
		{
			StateServerItem item = cache [id] as StateServerItem;
			if (item == null || item.IsAbandoned ())
				return null;

			return item;
		}

		internal void Remove (string id)
		{
			cache.Remove (id);
		}

		public override object InitializeLifetimeService ()
		{
			return null; // just in case...
		}
	}
}
#endif