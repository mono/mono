// 
// System.Web.Configuration.HandlerFactoryConfiguration
//
// Author:
//   Patrik Torstensson (ptorsten@hotmail.com)
//
using System.Collections;

namespace System.Web.Configuration {
	[MonoTODO]
	public class HandlerFactoryConfiguration {
		static private ArrayList _items = new ArrayList();

		static public void Add(string rtype, string path, string type) {
			HandlerItem item = new HandlerItem(rtype, path, type);

			_items.Add(item);
		}

		static public HandlerItem FindHandler(string type, string path) {
			int pos = 0;
			int count = _items.Count;

			for (pos = 0; pos != count; pos++) {
				if (((HandlerItem) _items[pos]).IsMatch(type, path)) 
					return (HandlerItem) _items[pos];
			}

			return null;
		}

		public HandlerFactoryConfiguration() {
		}
	}
}
