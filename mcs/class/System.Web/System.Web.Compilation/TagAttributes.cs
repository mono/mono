//
// System.Web.Compilation.TagAttributes
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Text;

namespace System.Web.Compilation
{
	class TagAttributes
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
			atts_hash = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
						   CaseInsensitiveComparer.Default);
			for (int i = 0; i < keys.Count; i++)
				atts_hash.Add (keys [i], values [i]);
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
			    0 == String.Compare ((string) key,  "runat", true)) {
			    	if (0 == String.Compare ((string) value,  "server", true))
					MakeHash ();
				else
					throw new HttpException ("runat attribute must have a 'server' value");
			}

			if (got_hashed) {
				atts_hash [key] = value;
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

		private int CaseInsensitiveSearch (string key)
		{
			// Hope not to have many attributes when the tag is not a server tag...
			for (int i = 0; i < keys.Count; i++){
				if (0 == String.Compare ((string) keys [i], key, true))
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
				if (got_hashed)
					atts_hash [key] = value;
				else {
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

			return (att.StartsWith ("<%#") && att.EndsWith ("%>"));
		}
		
		public Hashtable GetDictionary (string key)
		{
			if (got_hashed)
				return atts_hash;

			if (tmp_hash == null)
				tmp_hash = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
							  CaseInsensitiveComparer.Default);
			
			tmp_hash.Clear ();
			for (int i = keys.Count - 1; i >= 0; i--)
				if (key == null || String.Compare (key, (string) keys [i], true) == 0)
					tmp_hash [keys [i]] = values [i];

			return tmp_hash;
		}
		
		public override string ToString ()
		{
			StringBuilder result = new StringBuilder ();
			string value;
			foreach (string key in Keys){
				result.Append (key);
				value = this [key] as string;
				if (value != null)
					result.AppendFormat ("=\"{0}\"", value);

				result.Append (' ');
			}

			if (result.Length > 0 && result [result.Length - 1] == ' ')
				result.Length--;
				
			return result.ToString ();
		}
	}
}

