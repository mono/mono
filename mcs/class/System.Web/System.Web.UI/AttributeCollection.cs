//
// System.Web.UI.AttributeCollection.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;

namespace System.Web.UI {

	public sealed class AttributeCollection
	{
		StateBag bag;
		Hashtable list;
		
		public AttributeCollection (StateBag bag)
		{
			this.bag = bag;
			list = new Hashtable ();
		}

		public int Count {
			get { return list.Count; }
		}

		[MonoTODO]
		public CssStyleCollection CssStyle {
			get { return null; }
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

		public void AddAttributes (HtmlTextWriter writer)
		{
			foreach (object key in list.Keys) {

				object value = list [key];
				writer.AddAttribute ((string) key, (string) value);
			}
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public void Remove (string key)
		{
			list.Remove (key);
		}

		public void Render (HtmlTextWriter writer)
		{
			foreach (object key in list.Keys) {
				object value = list [key];
				writer.WriteAttribute ((string) key, (string) value);
			}
		}
	}
}
