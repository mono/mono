//
// System.Web.UI.CssStyleCollection.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;

namespace System.Web.UI {

	public sealed class CssStyleCollection
	{
		Hashtable list = new Hashtable ();

		public int Count {
			get { return list.Count; }
		}

		public string this [string key] {

			get { return list [key] as string; }

			set { list [key] = value; }
		}

		public ICollection Keys {
			get { return list.Keys; }
		}

		public void Add (string key, string value)
		{
			list.Add (key, value);
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public void Remove (string key)
		{
			list.Remove (key);
		}
	}
}
