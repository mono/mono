// 
// System.Web.Configuration.ModulesConfiguration
//
// Authors:
// 	Patrik Torstensson (ptorsten@hotmail.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//
using System;
using System.Collections;

namespace System.Web.Configuration
{
	class ModulesConfiguration
	{
		ArrayList modules;

		public ModulesConfiguration () : this (null)
		{
		}

		public ModulesConfiguration (ModulesConfiguration parent)
		{
			if (parent != null)
				modules = new ArrayList (parent.modules);
			else
				modules = new ArrayList ();
		}

		public void Add (ModuleItem item)
		{
			modules.Add (item);
		}

		public ModuleItem Remove (string name)
		{
			int i = GetIndex (name);
			if (i == -1)
				return null;
			
			ModuleItem item = (ModuleItem) modules [i];
			modules.RemoveAt (i);
			return item;
		}

		public void Clear ()
		{
			modules.Clear ();
		}

		public HttpModuleCollection CreateCollection ()
		{
			HttpModuleCollection items = new HttpModuleCollection ();
			foreach (ModuleItem item in modules)
				items.AddModule (item.ModuleName, item.Create ());

			return items;
		}

		int GetIndex (string name)
		{
			int end = modules.Count;

			for (int i = 0; i < end; i++) {
				ModuleItem item = (ModuleItem) modules [i];
				if (item.IsMatch (name))
					return i;
			}

			return -1;
		}
	}
}

