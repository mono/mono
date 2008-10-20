// 
// System.Web.Configuration.ModulesConfiguration
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
// 	Miguel de Icaza (miguel@novell.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (C) 2005 Novell, Inc. (http://www.novell.com)
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
using System;
using System.Collections;

namespace System.Web.Configuration
{
	class ModuleItem {
		public string Name;
		public Type Type;
		
		public ModuleItem (string name, Type type)
		{
			Name = name;
			Type = type;
		}
	}
		
	class ModulesConfiguration
	{
		ArrayList Modules;

		ModulesConfiguration ()
		{
		}

		public ModulesConfiguration (ModulesConfiguration parent)
		{
			if (parent != null)
				Modules = new ArrayList (parent.Modules);
			else
				Modules = new ArrayList (8);
		}

		public void Add (string name, Type type)
		{
			Modules.Add (new ModuleItem (name, type));
		}
		
		public void Add (string name, string type)
		{
			Type item_type;
			
			try {
				item_type = HttpApplication.LoadType (type, true);
			} catch (Exception e){
				throw new HttpException (
					String.Format ("Failed to load module `{0}' from type `{1}'", name, type), e);
			}

			if (!typeof (IHttpModule).IsAssignableFrom (item_type))
				throw new HttpException (
					String.Format ("The type {0} can not be assigned to IHttpHandler in the <httpModule> section",
						       name));

			Add (name, item_type);
		}

		public ModuleItem Remove (string name)
		{
			int end = Modules.Count;

			for (int i = 0; i < end; i++) {
				ModuleItem item = (ModuleItem) Modules [i];

				if (item.Name == name){
					Modules.RemoveAt (i);
					return item;
				}
			}
			return null;
		}

		public void Clear ()
		{
			Modules.Clear ();
		}

		public HttpModuleCollection LoadModules (HttpApplication app)
		{
			HttpModuleCollection coll = new HttpModuleCollection ();
			foreach (ModuleItem item in Modules) {
				IHttpModule module = (IHttpModule) Activator.CreateInstance (item.Type, true);
				module.Init (app);
				coll.AddModule (item.Name, module);
			}

			return coll;
		}
	}
}

