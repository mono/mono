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
using System.Text;

namespace System.Data.Common {

	public class DbConnectionOptions {

		#region Fields

		internal NameValueCollection options;

		#endregion // Fields

		#region Constructors

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
		public DbConnectionString (string connectionString, Hashtable synonyms, bool useFirstKeyValuePair)
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

		#endregion // Methods
	}
}

#endif
