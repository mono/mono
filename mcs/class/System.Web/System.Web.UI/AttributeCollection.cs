//
// System.Web.UI.AttributeCollection.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System.Web.Util;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class AttributeCollection
	{
		StateBag bag;
		CssStyleCollection styleCollection;
		internal const string StyleAttribute = "style";
		
		public AttributeCollection (StateBag bag)
		{
			this.bag = bag;
		}

		public override bool Equals (object o) 
		{
			AttributeCollection other = o as AttributeCollection;
			if (other == null) {
				return false;
			}

			if (Count != other.Count) {
				return false;
			}

			foreach (string key in Keys) {
				if (0 == String.CompareOrdinal (key, StyleAttribute)) {
					continue;
				}
				if (0 == String.CompareOrdinal (other [key], this [key])) {
					return false;
				}
			}

			if ((styleCollection == null && other.styleCollection != null) ||
				(styleCollection != null && other.styleCollection == null)) {
				return false;
			}
			else if (styleCollection != null){
				// other.styleCollection != null too
				if (styleCollection.Count != other.styleCollection.Count){
					return false;
				}
				foreach (string styleKey in styleCollection.Keys){
					if (0 == String.CompareOrdinal(styleCollection [styleKey], other.styleCollection [styleKey])) {
						return false;
					}
				}
			}

			return true;
		}

		public override int GetHashCode () 
		{
			int hashValue = 0;
			
			foreach (string key in Keys) {
				if (key == StyleAttribute) {
					continue;
				}
				hashValue ^= key.GetHashCode ();
				string value = this [key];
				if (value != null) {
					hashValue ^= value.GetHashCode ();
				}
			}

			if (styleCollection != null) {
				foreach (string styleKey in styleCollection.Keys) {
					hashValue ^= styleCollection [styleKey].GetHashCode ();
					string styleValue = styleCollection [styleKey];
					if (styleValue != null) {
						hashValue ^= styleValue.GetHashCode ();
					}
				}
			}

			return hashValue;
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
			if (0 == String.Compare (key, StyleAttribute, true, Helpers.InvariantCulture)) {
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
			if (0 == String.Compare (key, StyleAttribute, true, Helpers.InvariantCulture)) {
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

		internal void CopyFrom (AttributeCollection attributeCollection)
		{
			if (attributeCollection == null || attributeCollection.Count == 0)
				return;

			foreach (string key in attributeCollection.bag.Keys)
				this.Add (key, attributeCollection [key]);
		}
	}
}
