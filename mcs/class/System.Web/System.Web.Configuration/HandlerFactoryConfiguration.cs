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
		Hashtable _cache;
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
				
			_cache = Hashtable.Synchronized(new Hashtable());
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
			if (_cache.ContainsKey(verb+"+"+path))
				_cache.Remove(verb+"+"+path);
			return item;
		}

		public void Clear ()
		{
			mappings.Clear ();
			_cache.Clear();
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
			string cahceKey = verb+"+"+path;
			object answer = _cache[cahceKey];
			if (answer != null)
				return (int)answer;
			int end = mappings.Count;

			for (int i = ownIndex; i < end; i++) {
				HandlerItem item = (HandlerItem) mappings [i];
				if (item.IsMatch (verb, path))
				{
					_cache[cahceKey] = i;
					return i;
				}
			}

			// parent mappings
			end = ownIndex;
			for (int i = 0; i < end; i++) {
				HandlerItem item = (HandlerItem) mappings [i];
				if (item.IsMatch (verb, path))
				{
					_cache[cahceKey] = i;
					return i;
				}
			}
			_cache[cahceKey] = -1;
			return -1;
		}
	}
}

