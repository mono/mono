// 
// System.Web.Configuration.HandlerFactoryConfiguration
//
// Authors:
// 	Patrik Torstensson (ptorsten@hotmail.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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

