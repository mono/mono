//
// System.Web.UI.CssStyleCollection.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
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
using System.Collections;
using System.Text;

namespace System.Web.UI {

	public sealed class CssStyleCollection
	{
		private StateBag bag;
		private StateBag style;

		internal CssStyleCollection ()
		{
		}
		
		internal CssStyleCollection (StateBag bag)
		{
			this.bag = bag;
			style = new StateBag ();
			string st_string = bag ["style"] as string;
			if (st_string != null)
				FillStyle (st_string);
		}

		internal void FillStyle (string s)
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

		private string BagToString ()
		{
			StringBuilder sb = new StringBuilder ();
			foreach (string k in style.Keys)
				sb.AppendFormat ("{0}: {1}; ", k, style [k]);
			return sb.ToString ();
		}
		
		public int Count
		{
			get { return style.Count; }
		}

		public string this [string key]
		{
			get {
				return style [key] as string;
			}

			set {
				Add (key, value);
			}
		}

		public ICollection Keys {
			get { return style.Keys; }
		}

		public void Add (string key, string value)
		{
			style [key] = value;
			if (bag != null)
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
			if (bag != null)
				bag.Remove ("style");
			style.Clear ();
		}

		public void Remove (string key)
		{
			if (style [key] != null) {
				style.Remove (key);
				if (bag != null)
					bag ["style"] = BagToString ();
			}
		}
	}
}

