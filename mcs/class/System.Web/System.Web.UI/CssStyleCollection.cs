//
// System.Web.UI.CssStyleCollection.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;

namespace System.Web.UI {

	public sealed class CssStyleCollection
	{
		private StateBag bag;

		internal CssStyleCollection (StateBag bag)
		{
			this.bag = bag;
		}
		
		public int Count {
			get { return bag.Count; }
		}

		public string this [string key] {

			get { return bag [key] as string; }

			set { bag [key] = value; }
		}

		public ICollection Keys {
			get { return bag.Keys; }
		}

		public void Add (string key, string value)
		{
			bag.Add (key, value);
		}

		public void Clear ()
		{
			bag.Clear ();
		}

		public void Remove (string key)
		{
			bag.Remove (key);
		}
	}
}

