//
// System.Web.UI.AttributeCollection.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com
//

using System;
using System.Collections;

namespace System.Web.UI {

	public sealed class AttributeCollection
	{
		private StateBag bag;
		private CssStyleCollection styleCollection;
		
		public AttributeCollection (StateBag bag)
		{
			this.bag = bag;
		}

		public int Count {
			get { return bag.Count; }
		}

		public CssStyleCollection CssStyle {
			get {
				if (styleCollection == null)
					styleCollection = new CssStyleCollection (bag);
				return styleCollection;
			}
		}

		public string this [string key] {
			get { return bag [key] as string; }

			set { bag.Add (key, value); }
		}

		public ICollection Keys {
			get { return bag.Keys; }
		}

		public void Add (string key, string value)
		{
			bag.Add (key, value); // if exists, only the value is replaced.
		}

		public void AddAttributes (HtmlTextWriter writer)
		{
			foreach (string key in bag.Keys) {
				string value = bag [key] as string;
				writer.AddAttribute (key, value);
			}
		}

		public void Clear ()
		{
			bag.Clear ();
		}

		public void Remove (string key)
		{
			bag.Remove (key);
		}

		public void Render (HtmlTextWriter writer)
		{
			foreach (string key in bag.Keys) {
				string value = bag [key] as string;
				writer.WriteAttribute (key, value);
			}
		}
	}
}
