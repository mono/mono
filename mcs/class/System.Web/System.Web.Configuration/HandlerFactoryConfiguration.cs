// 
// System.Web.Configuration.HandlerFactoryConfiguration
//
// Authors:
// 	Patrik Torstensson (ptorsten@hotmail.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Collections;

namespace System.Web.Configuration
{
	class HandlerFactoryConfiguration
	{
		ArrayList mappings;
		int ownIndex;

		public HandlerFactoryConfiguration () : this (null)
		{
		}

		public HandlerFactoryConfiguration (HandlerFactoryConfiguration parent)
		{
			if (parent != null)
				mappings = new ArrayList (parent.mappings);
			else
				mappings = new ArrayList ();

			ownIndex = mappings.Count;
		}

		public void Add (HandlerItem mapping)
		{
			mappings.Add (mapping);
		}

		public HandlerItem Remove (string verb, string path)
		{
			int i = GetIndex (verb, path);
			if (i == -1)
				return null;
			
			HandlerItem item = (HandlerItem) mappings [i];
			mappings.RemoveAt (i);
			return item;
		}

		public void Clear ()
		{
			mappings.Clear ();
		}

		public HandlerItem FindHandler (string verb, string path)
		{
			int i = GetIndex (verb, path);
			if (i == -1)
				return null;

			return (HandlerItem) mappings [i];
		}

		int GetIndex (string verb, string path)
		{
			int end = mappings.Count;

			for (int i = ownIndex; i < end; i++) {
				HandlerItem item = (HandlerItem) mappings [i];
				if (item.IsMatch (verb, path))
					return i;
			}

			// parent mappings
			end = ownIndex;
			for (int i = 0; i < end; i++) {
				HandlerItem item = (HandlerItem) mappings [i];
				if (item.IsMatch (verb, path))
					return i;
			}

			return -1;
		}
	}
}

