//
// System.Data.Common.DbConnectionOptions
//	adapted from older (pre beta1) DbConnectionString
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Collections;
using System.Collections.Specialized;
using System.Security;
using System.Text;

namespace System.Data.Common {

	public class DbConnectionOptions {

		#region Fields

		internal NameValueCollection options;
		internal string normalizedConnectionString;

		#endregion // Fields

		#region Constructors

		internal DbConnectionOptions ()
		{
		}

		[MonoTODO]
		protected internal DbConnectionOptions (DbConnectionOptions connectionOptions)
		{
			options = connectionOptions.options;
		}

		[MonoTODO]
		public DbConnectionOptions (string connectionString)
		{
			options = new NameValueCollection ();
			ParseConnectionString (connectionString);
		}
		
		[MonoTODO]
		public DbConnectionOptions (string connectionString, Hashtable synonyms, bool useFirstKeyValuePair)
			: this (connectionString)
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public bool IsEmpty {
			get { throw new NotImplementedException (); }
		}

		public string this [string keyword] {
			get { return options [keyword]; }
		}

		public ICollection Keys {
			get { return options.Keys; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected void BuildConnectionString (StringBuilder builder, string[] withoutOptions, string insertValue)
		{
			throw new NotImplementedException ();
		}

		public bool ContainsKey (string keyword)
		{
			return (options.Get (keyword) != null);
		}

		public bool ConvertValueToBoolean (string keyname, bool defaultvalue)
		{
			if (ContainsKey (keyname))
				return Boolean.Parse (this [keyname].Trim ());
			return defaultvalue;
		}

		public int ConvertValueToInt32 (string keyname, int defaultvalue)
		{
			if (ContainsKey (keyname))
				return Int32.Parse (this [keyname].Trim ());
			return defaultvalue;
		}

		[MonoTODO]
		public bool ConvertValueToIntegratedSecurity ()
		{
			throw new NotImplementedException ();
		}

		public string ConvertValueToString (string keyname, string defaultValue)
		{
			if (ContainsKey (keyname))
				return this [keyname];
			return defaultValue;
		}

		[MonoTODO]
		protected internal virtual PermissionSet CreatePermissionSet ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual string Expand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string RemoveKeyValuePairs (string connectionString, string[] keynames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string UsersConnectionString (bool hisPasswordPwd)
		{
			throw new NotImplementedException ();
		}

		internal void ParseConnectionString (string connectionString)
		{
			if (connectionString.Length == 0)
				return;

			connectionString += ";";

			bool inQuote = false;
			bool inDQuote = false;
			bool inName = true;

			string name = String.Empty;
			string value = String.Empty;
			StringBuilder sb = new StringBuilder ();

			for (int i = 0; i < connectionString.Length; i += 1) {
				char c = connectionString [i];
				char peek;
				if (i == connectionString.Length - 1)
					peek = '\0';
				else 
					peek = connectionString [i + 1];

				switch (c) {
				case '\'':
					if (inDQuote) 
						sb.Append (c);
					else if (peek.Equals (c)) {
						sb.Append (c);
						i += 1;
					}
					else 
						inQuote = !inQuote;
					break;
				case '"':
					if (inQuote) 
						sb.Append (c);
					else if (peek.Equals (c)) {
						sb.Append (c);
						i += 1;
					}
					else 
						inDQuote = !inDQuote;
					break;
				case ';':
					if (inDQuote || inQuote)
						sb.Append (c);
					else {
						if (name != String.Empty && name != null) {
							value = sb.ToString ();
							// FIXME - KeywordLookup is an NOP
							// options [KeywordLookup (name.Trim ())] = value;
							options [name.Trim ()] = value;
						}
						inName = true;
						name = String.Empty;
						value = String.Empty;
						sb = new StringBuilder ();
					}
					break;
				case '=':
					if (inDQuote || inQuote || !inName)
						sb.Append (c);
					else if (peek.Equals (c)) {
						sb.Append (c);
						i += 1;
					} 
					else {
						name = sb.ToString ();
						sb = new StringBuilder ();
						inName = false;
					}
					break;
				case ' ':
					if (inQuote || inDQuote)
						sb.Append (c);
					else if (sb.Length > 0 && !peek.Equals (';'))
						sb.Append (c);
					break;
				default:
					sb.Append (c);
					break;
				}
			}	
			
			StringBuilder normalized = new StringBuilder ();
			ArrayList keys = new ArrayList ();
			keys.AddRange (Keys);
			keys.Sort ();
			foreach (string key in keys)
			{
				string entry = String.Format ("{0}=\"{1}\";", key, this [key].Replace ("\"", "\"\""));
				normalized.Append (entry);
			}
			normalizedConnectionString = normalized.ToString ();
		}

		#endregion // Methods
	}
}

#endif
