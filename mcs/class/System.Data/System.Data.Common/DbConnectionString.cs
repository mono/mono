//
// System.Data.Common.DbConnectionString
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.Serialization;
using System.Text;

namespace System.Data.Common {
	public class DbConnectionString : ISerializable
	{
		#region Fields

		KeyRestrictionBehavior behavior;
		string normalizedConnectionString;
		internal NameValueCollection options;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		protected internal DbConnectionString (DbConnectionString constr)
		{
			options = constr.options;
		}

		[MonoTODO]
		public DbConnectionString (string connectionString)
		{
			options = new NameValueCollection ();
			ParseConnectionString (connectionString);
		}
		
		[MonoTODO]
		protected DbConnectionString (SerializationInfo si, StreamingContext sc)
		{
		}

		[MonoTODO]
		public DbConnectionString (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
			: this (connectionString)
		{
			this.behavior = behavior;
		}

		#endregion // Constructors

		#region Properties

		public KeyRestrictionBehavior Behavior {
			get { return behavior; }
		}

		[MonoTODO]
		protected virtual string CacheConnectionString {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool IsEmpty {
			get { throw new NotImplementedException (); }
		}

		public string this [string x] {
			get { return options [x]; }
		}

		public ICollection Keys {
			get { return options.Keys; }
		}

		public string NormalizedConnectionString {
			get { return normalizedConnectionString; }
		}

		[MonoTODO]
		public string Restrictions {
			get { throw new NotImplementedException (); }
		}
		
		#endregion // Properties

		#region Methods

		public static void AppendKeyValuePairBuilder (StringBuilder builder, string keyname, string keyvalue)
		{
			throw new NotImplementedException ();
		}

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

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		protected virtual string KeywordLookup (string keyname)
		{
			return keyname;
		}

		protected void ParseConnectionString (string connectionString)
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
							options [KeywordLookup (name.Trim ())] = value;
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

		public virtual void PermissionDemand ()
		{
			throw new NotImplementedException ();
		}

		public static string RemoveKeyValuePairs (string connectionString, string[] keynames)
		{
			throw new NotImplementedException ();
		}

		public string UsersConnectionString (bool hisPasswordPwd)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods

	}
}

#endif
