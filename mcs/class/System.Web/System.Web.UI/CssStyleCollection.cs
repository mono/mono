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
using System.Text;

namespace System.Web.UI {

	public sealed class CssStyleCollection
	{
		private StateBag bag;
		private StateBag style;

		internal CssStyleCollection (StateBag bag)
		{
			this.bag = bag;
			style = new StateBag ();
			string st_string = bag ["style"] as string;
			if (st_string != null)
				fillStyle (st_string);
		}
		
		private void fillStyle (string s)
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
			fillStyle (fullValue.Substring (mark + 1));
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
			bag ["style"] = BagToString ();
		}

		public void Clear ()
		{
			bag.Remove ("style");
			style.Clear ();
		}

		public void Remove (string key)
		{
			if (style [key] != null) {
				style.Remove (key);
				bag ["style"] = BagToString ();
			}
		}
	}
}

