//
// System.Web.UI.AttributeCollection.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Security.Permissions;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class AttributeCollection
	{
		private StateBag bag;
		private CssStyleCollection styleCollection;
		internal const string StyleAttribute = "style";
		
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

			set {
				Add (key, value);
			}
		}

		public ICollection Keys {
			get { return bag.Keys; }
		}

		public void Add (string key, string value)
		{
			if (0 == String.Compare (key, StyleAttribute, true, CultureInfo.InvariantCulture)) {
				CssStyle.Value = value;
				return;
			}
			bag.Add (key, value);
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
			CssStyle.Clear ();
			bag.Clear ();
		}

		public void Remove (string key)
		{
			if (0 == String.Compare (key, StyleAttribute, true, CultureInfo.InvariantCulture)) {
				CssStyle.Clear ();
				return;
			}
			bag.Remove (key);
		}

		public void Render (HtmlTextWriter writer)
		{
			foreach (string key in bag.Keys) {
				string value = bag [key] as string;
				if (value != null)
					writer.WriteAttribute (key, value, true);
			}
		}

#if NET_2_0
		internal void CopyFrom (AttributeCollection attributeCollection)
		{
			if (attributeCollection == null || attributeCollection.Count == 0)
				return;

			foreach (string key in attributeCollection.bag.Keys)
				this.Add (key, attributeCollection [key]);
		}
#endif
	}
}
