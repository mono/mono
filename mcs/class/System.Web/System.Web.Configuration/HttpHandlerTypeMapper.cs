//
// System.Web.Configuration.HttpHandlerTypeMapper
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Collections;

namespace System.Web.Configuration
{
	class HttpHandlerTypeMapper
	{
		ArrayList mappings;

		public HttpHandlerTypeMapper () : this (null)
		{
		}

		public HttpHandlerTypeMapper (HttpHandlerTypeMapper parent)
		{
			if (parent != null)
				mappings = new ArrayList (parent.mappings);
			else
				mappings = new ArrayList ();
		}

		public void Add (HandlerItem mapping)
		{
			mappings.Add (mapping);
		}

		public HandlerItem Remove (string verb, string path)
		{
			int i = SearchHandler (verb, path);
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
			int i = SearchHandler (verb, path);
			if (i == -1)
				return null;

			return (HandlerItem) mappings [i];
		}

		int SearchHandler (string verb, string path)
		{
			int end = mappings.Count;

			for (int i = 0; i < end; i++) {
				HandlerItem item = (HandlerItem) mappings [i];
				if (item.IsMatch (verb, path))
					return i;
			}

			return -1;
		}
		
	}
}

