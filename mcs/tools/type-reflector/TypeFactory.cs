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
		private static BooleanSwitch info = new BooleanSwitch ("type-factory",
				"Information about creating types.");

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
			Type type = null;
			try {
				type = (Type) entries[key];
				return CreateInstance (type);
			}
			catch (Exception e) {
				Console.WriteLine ("TypeFactory trace: {0}", info.Enabled);
				Console.WriteLine (
						"Exception creating ({0}, {1}): {2}", key, type, e.ToString());
				Trace.WriteLineIf (info.Enabled, string.Format (
						"Exception creating ({0}, {1}): {2}", key, type, e.ToString()));
				return null;
			}
		}
	}
}

