//
// Authors:
//	Marek Habersack <grendel@twistedcode.net>
//
// (C) 2010 Novell, Inc (http://novell.com)
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
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	sealed class NamedCssStyleCollection
	{
		CssStyleCollection collection;
			
		public CssStyleCollection Collection {
			get {
				if (collection == null)
					collection = new CssStyleCollection ();

				return collection;
			}
		}
			
		public string Name { get; private set; }
			
		public NamedCssStyleCollection (string name)
		{
			if (name == null)
				name = String.Empty;

			Name = name;
		}

		public NamedCssStyleCollection CopyFrom (CssStyleCollection coll)
		{
			if (coll == null)
				return this;

			CssStyleCollection collection = Collection;
			foreach (string key in coll.Keys)
				collection.Add (key, coll [key]);

			return this;
		}

		public NamedCssStyleCollection Add (HtmlTextWriterStyle key, string value)
		{
			Collection.Add (key, value);
			return this;
		}

		public NamedCssStyleCollection Add (string key, string value)
		{
			Collection.Add (key, value);
			return this;
		}

		public NamedCssStyleCollection Add (Style style)
		{
			if (style != null)
				CopyFrom (style.GetStyleAttributes (null));

			return this;
		}
	}
}
