//
// TypeFactory.cs: Generic factory implementation
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Mono.TypeReflector
{
	public class TypeFactory
	{
		private IDictionary entries = new Hashtable ();

		public void Add (object key, Type value)
		{
			entries.Add (key, value);
		}

		public void Remove (object key)
		{
			entries.Remove (key);
		}

		public ICollection Keys {
			get {return entries.Keys;}
		}

		private object CreateInstance (Type type)
		{
			return Activator.CreateInstance (type);
		}

		public object Create (object key)
		{
			try {
				Type type = (Type) entries[key];
				return CreateInstance (type);
			}
			catch {
				return null;
			}
		}
	}
}

