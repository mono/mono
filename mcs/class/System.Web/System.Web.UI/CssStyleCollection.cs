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

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class CssStyleCollection
	{
		StateBag bag;
		StateBag style;

		internal CssStyleCollection (StateBag bag)
		{
			this.bag = bag;
			if (bag != null)
				InitFromStyle ();
		}

		void InitFromStyle ()
		{
			style = new StateBag ();
			string att = (string) bag ["style"];
			if (att != null) {
				FillStyle (att);
			}
		}

		void FillStyle (string s)
		{
			int mark = s.IndexOf (':');
			if (mark == -1)
				return;
			string key = s.Substring (0, mark). Trim ();
			if (mark + 1 > s.Length)
				return;

			string fullValue = s.Substring (mark + 1);
			if (fullValue == "")
				return;

			mark = fullValue.IndexOf (';');
			string value;
			if (mark == -1)
				value = fullValue.Trim ();
			else
				value = fullValue.Substring (0, mark).Trim ();

			style.Add (key, value);
			if (mark + 1 > fullValue.Length)
				return;
			FillStyle (fullValue.Substring (mark + 1));
		}

		string BagToString ()
		{
			StringBuilder sb = new StringBuilder ();
			foreach (string key in style.Keys) {
				if (key == "background-image")
					sb.AppendFormat ("{0}:url({1});", key, HttpUtility.UrlPathEncode ((string) style [key]));
				else
					sb.AppendFormat ("{0}:{1};", key, style [key]);
			}

			return sb.ToString ();
		}

		public int Count {
			get {
				InitFromStyle ();
				return style.Count;
			}
		}

		public string this [string key] {
			get {
				InitFromStyle ();
				return style [key] as string;
			}

			set {
				Add (key, value);
			}
		}

		public ICollection Keys {
			get {
				InitFromStyle ();
				return style.Keys;
			}
		}

		public void Add (string key, string value)
		{
			InitFromStyle ();
			style [key] = value;
			bag ["style"] = BagToString ();
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
			bag.Remove ("style");
			InitFromStyle ();
		}

		public void Remove (string key)
		{
			InitFromStyle ();
			if (style == null || style [key] == null)
				return;
			style.Remove (key);
			bag ["style"] = BagToString ();
		}
#if NET_2_0
		public string this [HtmlTextWriterStyle key] {
			get {
				InitFromStyle ();
				return style [HtmlTextWriter.StaticGetStyleName (key)] as string;
			}
			set {
				Add (HtmlTextWriter.StaticGetStyleName (key), value);
			}
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
			get { return BagToString (); }
			set {
				bag ["style"] = value;
				InitFromStyle ();
			}
		}
	}
}

