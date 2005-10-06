// 
// System.Web.Configuration.ModulesConfiguration
//
// Authors:
// 	Patrik Torstensson (ptorsten@hotmail.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
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

