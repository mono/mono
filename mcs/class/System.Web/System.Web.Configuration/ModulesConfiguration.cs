// 
// System.Web.Configuration.ModulesConfiguration
//
// Author:
//   Patrik Torstensson (ptorsten@hotmail.com)
//
using System;
using System.Collections;

namespace System.Web.Configuration {
	[MonoTODO]
	public class ModulesConfiguration {
		static private ArrayList _items = new ArrayList();

		static public void Add(string name, string type) {
			ModuleItem item = new ModuleItem(name, type);
			_items.Add(item);
		}

		public ModulesConfiguration() {
		}

		public HttpModuleCollection CreateCollection() {
			HttpModuleCollection items = new HttpModuleCollection();
			int pos = 0;
			int count = _items.Count;

			for (pos = 0; pos != count; pos++) {
				items.AddModule(((ModuleItem) _items[pos]).ModuleName, ((ModuleItem) _items[pos]).Create());
			}

			return items;
		}
	}
}
