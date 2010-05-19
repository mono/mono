//
// System.Web.Compilation.TagAttributes
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2003-2009 Novell, Inc (http://novell.com/)
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
using System.Globalization;
using System.Text;
using System.Web.Util;

namespace System.Web.Compilation
{
	sealed class TagAttributes
	{
		Hashtable atts_hash;
		Hashtable tmp_hash;
		ArrayList keys;
		ArrayList values;
		bool got_hashed;

		public TagAttributes ()
		{
			got_hashed = false;
			keys = new ArrayList ();
			values = new ArrayList ();
		}

		void MakeHash ()
		{
			atts_hash = new Hashtable (StringComparer.InvariantCultureIgnoreCase);
			for (int i = 0; i < keys.Count; i++) {
				CheckServerKey (keys [i]);
				atts_hash.Add (keys [i], values [i]);
			}
			got_hashed = true;
			keys = null;
			values = null;
		}
		
		public bool IsRunAtServer ()
		{
			return got_hashed;
		}

		public void Add (object key, object value)
		{
			if (key != null && value != null &&
			    0 == String.Compare ((string) key,  "runat", true, Helpers.InvariantCulture)) {
			    	if (0 != String.Compare ((string) value,  "server", true))
					throw new HttpException ("runat attribute must have a 'server' value");

				if (got_hashed)
					return; // ignore duplicate runat="server"

				MakeHash ();
			}

			if (value != null)
				value = HttpUtility.HtmlDecode (value.ToString ());

			if (got_hashed) {
				CheckServerKey (key);
				if (atts_hash.ContainsKey (key))
					throw new HttpException ("Tag contains duplicated '" + key +
								 "' attributes.");
				atts_hash.Add (key, value);
			} else {
				keys.Add (key);
				values.Add (value);
			}
		}
		
		public ICollection Keys 
		{
			get { return (got_hashed ? atts_hash.Keys : keys); }
		}

		public ICollection Values 
		{
			get { return (got_hashed ? atts_hash.Values : values); }
		}

		int CaseInsensitiveSearch (string key)
		{
			// Hope not to have many attributes when the tag is not a server tag...
			for (int i = 0; i < keys.Count; i++){
				if (0 == String.Compare ((string) keys [i], key, true, Helpers.InvariantCulture))
					return i;
			}
			return -1;
		}
		
		public object this [object key]
		{
			get {
				if (got_hashed)
					return atts_hash [key];

				int idx = CaseInsensitiveSearch ((string) key);
				if (idx == -1)
					return null;
						
				return values [idx];
			}

			set {
				if (got_hashed) {
					CheckServerKey (key);
					atts_hash [key] = value;
				} else {
					int idx = CaseInsensitiveSearch ((string) key);
					keys [idx] = value;
				}
			}
		}
		
		public int Count 
		{
			get { return (got_hashed ? atts_hash.Count : keys.Count);}
		}

		public bool IsDataBound (string att)
		{
			if (att == null || !got_hashed)
				return false;

			return (StrUtils.StartsWith (att, "<%#") && StrUtils.EndsWith (att, "%>"));
		}
		
		public IDictionary GetDictionary (string key)
		{
			if (got_hashed)
				return atts_hash;

			if (tmp_hash == null)
				tmp_hash = new Hashtable (StringComparer.InvariantCultureIgnoreCase);
			
			tmp_hash.Clear ();
			for (int i = keys.Count - 1; i >= 0; i--)
				if (key == null || String.Compare (key, (string) keys [i], true, Helpers.InvariantCulture) == 0)
					tmp_hash [keys [i]] = values [i];

			return tmp_hash;
		}
		
		public override string ToString ()
		{
			StringBuilder result = new StringBuilder ("TagAttributes {");
			string value;
			foreach (string key in Keys){
				result.Append ('[');
				result.Append (key);
				value = this [key] as string;
				if (value != null)
					result.AppendFormat ("=\"{0}\"", value);

				result.Append ("] ");
			}

			if (result.Length > 0 && result [result.Length - 1] == ' ')
				result.Length--;

			result.Append ('}');
			if (IsRunAtServer ())
				result.Append (" @Server");
			
			return result.ToString ();
		}
		
		void CheckServerKey (object key)
		{
			if (key == null || ((string)key).Length == 0)
				throw new HttpException ("The server tag is not well formed.");
		}
	}
}

