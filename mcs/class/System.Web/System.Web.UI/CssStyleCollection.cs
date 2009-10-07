//
// System.Web.UI.CssStyleCollection.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
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

using System.IO;
using System.Collections;
using System.Security.Permissions;
using System.Text;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Util;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class CssStyleCollection
	{
		StateBag bag;
		ListDictionary style;
		StringBuilder _value = new StringBuilder ();
		
		internal CssStyleCollection ()
		{
#if NET_2_0
			style = new ListDictionary (StringComparer.OrdinalIgnoreCase);
#else
			style = new ListDictionary ();
#endif
		}

		internal CssStyleCollection (StateBag bag) : this ()
		{
			this.bag = bag;
			if (bag != null && bag [AttributeCollection.StyleAttribute] != null)
				_value.Append (bag [AttributeCollection.StyleAttribute]);
			InitFromStyle ();
		}

		void InitFromStyle ()
		{
			style.Clear ();
			if (_value.Length > 0) {
				int startIndex = 0;
				while (startIndex >= 0)
					startIndex = ParseStyle (startIndex);
			}
		}

		int ParseStyle (int startIndex)
		{
			int colon = -1;
			for (int i = startIndex; i < _value.Length; i++) {
				if (_value [i] == ':') {
					colon = i;
					break;
				}
			}
			if (colon == -1 || colon + 1 == _value.Length)
				return -1;

			string key = _value.ToString (startIndex, colon - startIndex).Trim ();

			int semicolon = -1;
			for (int i = colon + 1; i < _value.Length; i++) {
				if (_value [i] == ';') {
					semicolon = i;
					break;
				}
			}
			string value;
			if (semicolon == -1)
				value = _value.ToString (colon + 1, _value.Length - colon - 1).Trim ();
			else
				value = _value.ToString (colon + 1, semicolon - colon - 1).Trim ();

			style.Add (key, value);
			if (semicolon == -1 || semicolon + 1 == _value.Length)
				return -1;

			return semicolon + 1;
		}

		void BagToValue ()
		{
			_value.Length = 0;
			foreach (string key in style.Keys)
				AppendStyle (_value, key, (string) style [key]);
		}

		static void AppendStyle (StringBuilder sb, string key, string value)
		{
#if NET_2_0
			if (String.Compare (key, "background-image", StringComparison.OrdinalIgnoreCase) == 0 &&
			    value.Length >= 3 && String.Compare ("url", 0, value, 0, 3, StringComparison.OrdinalIgnoreCase) != 0)
#else
			if (key == "background-image" && 0 != String.Compare ("url", value.Substring (0, 3), true,
									      Helpers.InvariantCulture))
#endif
				sb.AppendFormat ("{0}:url({1});", key, HttpUtility.UrlPathEncode (value));
			else
				sb.AppendFormat ("{0}:{1};", key, value);
		}

		public int Count {
			get { return style.Count; }
		}

		public string this [string key] {
			get { return style [key] as string; }
			set { Add (key, value); }
		}

		public ICollection Keys {
			get { return style.Keys; }
		}

		public void Add (string key, string value)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			if (value == null) {
				Remove (key);
				return;
			}

			string curr = (string) style [key];
			if (curr == null) {
				// just append
				style [key] = value;
				AppendStyle (_value, key, value);
			} else if (String.CompareOrdinal (curr, value) == 0) {
				// do nothing
				return;
			} else {
				style [key] = value;
				BagToValue ();
			}

			if (bag != null)
				bag [AttributeCollection.StyleAttribute] = _value.ToString ();
		}

#if NET_2_0
		public
#else
		internal
#endif
		void Add (HtmlTextWriterStyle key, string value)
		{
			Add (HtmlTextWriter.StaticGetStyleName (key), value);
		}

		public void Clear ()
		{
			style.Clear ();
			SetValueInternal (null);
		}

		public void Remove (string key)
		{
			if (style [key] == null)
				return;
			style.Remove (key);
			if (style.Count == 0)
				SetValueInternal (null);
			else
				BagToValue ();
		}
#if NET_2_0
		public string this [HtmlTextWriterStyle key] {
			get { return style [HtmlTextWriter.StaticGetStyleName (key)] as string; }
			set { Add (HtmlTextWriter.StaticGetStyleName (key), value); }
		}

		public void Remove (HtmlTextWriterStyle key)
		{
			Remove (HtmlTextWriter.StaticGetStyleName (key));
		}

		public
#else
		internal
#endif
		string Value {
			get { return _value.ToString (); }
			set {
				SetValueInternal (value);
				InitFromStyle ();
			}
		}

		void SetValueInternal (string value)
		{
			_value.Length = 0;
			if (value != null)
				_value.Append (value);
			if (bag != null) {
				if (value == null)
					bag.Remove (AttributeCollection.StyleAttribute);
				else
					bag [AttributeCollection.StyleAttribute] = value;
			}
		}
	}
}

