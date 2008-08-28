//
// System.Data.OracleClient.OracleConnectionStringBuilder.cs
//
// Author:
//   Sureshkumar T (tsureshkumar@novell.com)
//   Daniel Morgan <monodanmorg@yahoo.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2008 Daniel Morgan
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
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Data;
using System.Data.Common;
using System.ComponentModel;
using System.Globalization;

namespace System.Data.OracleClient
{
	[DefaultPropertyAttribute ("DataSource")]
	public sealed class OracleConnectionStringBuilder : DbConnectionStringBuilder
	{
		private const string 	DEF_DATASOURCE 			= "";
		private const bool 	DEF_INTEGRATEDSECURITY 		= false;
		private const int 	DEF_LOADBALANCETIMEOUT 		= 0;
		private const int 	DEF_MAXPOOLSIZE 		= 100;
		private const int 	DEF_MINPOOLSIZE 		= 0;
		private const string 	DEF_PASSWORD 			= "";
		private const bool 	DEF_PERSISTSECURITYINFO		= false;
		private const bool 	DEF_POOLING 			= true;
		private const string 	DEF_USERID 			= "";
		private const bool 	DEF_ENLIST 			= false;
		private const bool 	DEF_UNICODE 			= false;
		private const bool 	DEF_OMITORACLECONNECTIONNAME	= false;

		#region // Fields
		private string 	_dataSource;
		private bool 	_enlist;
		private bool 	_integratedSecurity;
		private int 	_loadBalanceTimeout;
		private int 	_maxPoolSize;
		private int 	_minPoolSize;
		private string 	_password;
		private bool 	_persistSecurityInfo;
		private bool 	_pooling;
		private string 	_userID;
		private bool	_unicode;
		private bool	_omitOracleConnectionName;

		private static Dictionary <string, string> _keywords; // for mapping duplicate keywords
		private static Dictionary <string, object> _defaults;
		#endregion // Fields

		#region Constructors
		public OracleConnectionStringBuilder () : this (String.Empty)
		{
			Init ();
		}

		public OracleConnectionStringBuilder (string connectionString)
		{
			Init ();
			base.ConnectionString = connectionString;
		}

		static OracleConnectionStringBuilder ()
		{
			_keywords = new Dictionary <string, string> ();
			_keywords ["DATA SOURCE"] 		= "Data Source";
			_keywords ["SERVER"] 			= "Data Source";
			_keywords ["ADDRESS"] 			= "Data Source";
			_keywords ["ADDR"] 			= "Data Source";
			_keywords ["NETWORK ADDRESS"] 		= "Data Source";
			_keywords ["ENLIST"] 			= "Enlist";
			_keywords ["INTEGRATED SECURITY"]	= "Integrated Security";
			_keywords ["TRUSTED_CONNECTION"] 	= "Integrated Security";
			_keywords ["MAX POOL SIZE"] 		= "Max Pool Size";
			_keywords ["MIN POOL SIZE"] 		= "Min Pool Size";
			_keywords ["PASSWORD"] 			= "Password";
			_keywords ["PWD"] 			= "Password";
			_keywords ["PERSISTSECURITYINFO"]	= "Persist Security Info";
			_keywords ["PERSIST SECURITY INFO"] 	= "Persist Security Info";
			_keywords ["POOLING"] 			= "Pooling";
			_keywords ["UID"] 			= "User ID";
			_keywords ["USER"] 			= "User ID";
			_keywords ["USER ID"] 			= "User ID";
			_keywords ["UNICODE"] 			= "Unicode";
			_keywords ["LOAD BALANCE TIMEOUT"]	= "Load Balance Timeout";
			_keywords ["OMIT ORACLE CONNECTION NAME"] = "Omit Oracle Connection Name";

			_defaults = new Dictionary <string, object> ();
			_defaults.Add("Data Source", DEF_DATASOURCE);
			_defaults.Add("Persist Security Info", DEF_PERSISTSECURITYINFO);
			_defaults.Add("Integrated Security", DEF_INTEGRATEDSECURITY);
			_defaults.Add("User ID", DEF_USERID);
			_defaults.Add("Password", DEF_PASSWORD);
			_defaults.Add("Enlist", DEF_ENLIST);
			_defaults.Add("Pooling", DEF_POOLING);
			_defaults.Add("Min Pool Size", DEF_MINPOOLSIZE);
			_defaults.Add("Max Pool Size", DEF_MAXPOOLSIZE);
			_defaults.Add("Unicode", DEF_UNICODE);
			_defaults.Add("Load Balance Timeout", DEF_LOADBALANCETIMEOUT);
			_defaults.Add("Omit Oracle Connection Name", DEF_OMITORACLECONNECTIONNAME);
		}
		#endregion // Constructors

		#region Properties	
		[DisplayNameAttribute ("Data Source")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string DataSource { 
			get { return _dataSource; }
			set { 
				base ["Data Source"] = value;
				_dataSource = value; 
			}
		}

		[DisplayNameAttribute ("Enlist")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public bool Enlist { 
			get { return _enlist; }
			set { 
				base ["Enlist"] = value;
				_enlist = value; 
			}
		}

		[DisplayNameAttribute ("Integrated Security")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public bool IntegratedSecurity { 
			get { return _integratedSecurity; }
			set { 
				base ["Integrated Security"] = value;
				_integratedSecurity = value; 
			}
		}

		public override bool IsFixedSize { 
			get { return true; }
		}

		public override object this [string keyword] { 
			get { 
				string mapped = MapKeyword (keyword);
				if (base.ContainsKey (mapped)) 
					return base [mapped];
				else
					return _defaults [mapped];
			}
			set {SetValue (keyword, value);}
		}

		public override ICollection Keys { 
			get { 
				List<string> keys = new List<string>();
                                keys.Add("Data Source");
                                keys.Add("Persist Security Info");
                                keys.Add("Integrated Security");
                                keys.Add("User ID");
                                keys.Add("Password");
                                keys.Add("Enlist");
                                keys.Add("Pooling");
                                keys.Add("Min Pool Size");
                                keys.Add("Max Pool Size");
                                keys.Add("Unicode");
                                keys.Add("Load Balance Timeout");
                                keys.Add("Omit Oracle Connection Name");
				ReadOnlyCollection<string> coll = new ReadOnlyCollection<string>(keys);
				return coll;
			}
		}

		[DisplayNameAttribute ("Load Balance Timeout")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public int LoadBalanceTimeout { 
			get { return _loadBalanceTimeout; }
			set { 
				base ["Load Balance Timeout"] = value;
				_loadBalanceTimeout = value; 
			}
		}

		[DisplayNameAttribute ("Max Pool Size")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public int MaxPoolSize { 
			get { return _maxPoolSize; }
			set { 
				base ["Max Pool Size"] = value;
				_maxPoolSize = value; 
			}
		}

		[DisplayNameAttribute ("Min Pool Size")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public int MinPoolSize { 
			get { return _minPoolSize; }
			set {
				base ["Min Pool Size"] = value;
				_minPoolSize = value; 
			}
		}

		[DisplayNameAttribute ("Omit Oracle Connection Name")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public bool OmitOracleConnectionName { 
			get { return _omitOracleConnectionName; }
			set {
				base ["Omit Oracle Connection Name"] = value;
				_omitOracleConnectionName = value; 
			}
		}

		[DisplayNameAttribute ("Password")]
		[PasswordPropertyTextAttribute (true)]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string Password { 
			get { return _password; }
			set { 
				base ["Password"] = value;
				_password = value; 
			}
		}

		[DisplayNameAttribute ("Persist Security Info")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public bool PersistSecurityInfo { 
			get { return _persistSecurityInfo; }
			set {
				base ["Persist Security Info"] = value;
				_persistSecurityInfo = value; 
			}
		}
		
		[DisplayNameAttribute ("Pooling")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public bool Pooling { 
			get { return _pooling; }
			set {
				base ["Pooling"] = value;
				_pooling = value; 
			}
		}

		[DisplayNameAttribute ("User ID")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string UserID { 
			get { return _userID; }
			set { 
				base ["User Id"]= value;
				_userID = value; 
			}
		}

		[DisplayNameAttribute ("Unicode")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public bool Unicode { 
			get { return _unicode; }
			set { 
				base ["Unicode"]= value;
				_unicode = value; 
			}
		}
		
		public override ICollection Values { 
			get {
				List<object> values = new List<object>();
                                values.Add(_dataSource);
                                values.Add(_persistSecurityInfo);
                                values.Add(_integratedSecurity);
                                values.Add(_userID);
                                values.Add(_password);
                                values.Add(_enlist);
                                values.Add(_pooling);
                                values.Add(_minPoolSize);
                                values.Add(_maxPoolSize);
                                values.Add(_unicode);
                                values.Add(_loadBalanceTimeout);
                                values.Add(_omitOracleConnectionName);
				ReadOnlyCollection<object> coll = new ReadOnlyCollection<object>(values);
				return coll;		 
			}
		}

		#endregion // Properties

		#region Methods
		private void Init ()
		{
			_dataSource		= DEF_DATASOURCE;
			_enlist			= DEF_ENLIST;
			_integratedSecurity	= DEF_INTEGRATEDSECURITY;
			_loadBalanceTimeout	= DEF_LOADBALANCETIMEOUT;
			_maxPoolSize		= DEF_MAXPOOLSIZE;
			_minPoolSize		= DEF_MINPOOLSIZE;
			_password		= DEF_PASSWORD;
			_persistSecurityInfo	= DEF_PERSISTSECURITYINFO;
			_pooling		= DEF_POOLING;
			_userID			= DEF_USERID;
			_unicode		= DEF_UNICODE;
			_omitOracleConnectionName = DEF_OMITORACLECONNECTIONNAME;
		}

		public override void Clear ()
		{
			base.Clear ();
			Init ();
		}

		public override bool ContainsKey (string keyword)
		{
			keyword = keyword.ToUpper ().Trim ();
			if (_keywords.ContainsKey (keyword))
				return base.ContainsKey (_keywords [keyword]);
			return false;
		}

		public override bool Remove (string keyword)
		{
			if (!ContainsKey (keyword))
				return false;
			this [keyword] = null;
			return true;
		}

		[MonoNotSupported ("")] // Note that base.ShouldSerialize() is called but not implemented
		public override bool ShouldSerialize (string keyword)
		{
			if (!ContainsKey (keyword))
				return false;
			keyword = keyword.ToUpper ().Trim ();
			// Assuming passwords cannot be serialized.
			if (_keywords [keyword] == "Password")
				return false;
			return base.ShouldSerialize (_keywords [keyword]);
		}

		public override bool TryGetValue (string keyword, out object value)
		{
			if (! ContainsKey (keyword)) {
				value = String.Empty;
				return false;
			}
			return base.TryGetValue (_keywords [keyword.ToUpper ().Trim ()], out value);
		}

		#endregion // Methods

		#region Private Methods
		private string MapKeyword (string keyword)
		{
			keyword = keyword.ToUpper ().Trim ();
			if (! _keywords.ContainsKey (keyword))
				throw new ArgumentException("Keyword not supported :" + keyword);
			return _keywords [keyword];
		}
		
		private void SetValue (string key, object value)
		{
			if (key == null)
				throw new ArgumentNullException ("key cannot be null!");

			string mappedKey = MapKeyword (key);

			switch (mappedKey.ToUpper ().Trim ()) {
			case "DATA SOURCE" :
				if (value == null) {
					_dataSource = DEF_DATASOURCE;
					base.Remove (mappedKey);
				} else 
					this.DataSource = value.ToString ();
				break;
			case "ENLIST" :
				if (value == null) {
					_enlist = DEF_ENLIST;
					base.Remove (mappedKey);
				} else if ( ! ConvertToBoolean(value))
					throw new NotImplementedException("Disabling the automatic"
									  + " enlistment of connections in the thread's current"
									  + " transaction context is not implemented.");
				break;
			case "INTEGRATED SECURITY" :
				if (value == null) {
					_integratedSecurity = DEF_INTEGRATEDSECURITY;
					base.Remove (mappedKey);
				} else 
					this.IntegratedSecurity = ConvertToBoolean (value);
				break;
			case "MAX POOL SIZE" :
				if (value == null) {
					_maxPoolSize = DEF_MAXPOOLSIZE;
					base.Remove (mappedKey);
				} else 
					this.MaxPoolSize = ConvertToInt32 (value);
				break;
			case "MIN POOL SIZE" :
				if (value == null) {
					_minPoolSize = DEF_MINPOOLSIZE;
					base.Remove (mappedKey);
				} else 
					this.MinPoolSize = ConvertToInt32 (value);
				break;
			case "PASSWORD" :
				if (value == null) {
					_password = DEF_PASSWORD;
					base.Remove (mappedKey);
				} else 
					this.Password = value.ToString ();
				break;
			case "PERSIST SECURITY INFO" :
				if (value == null) {
					_persistSecurityInfo = DEF_PERSISTSECURITYINFO;
					base.Remove (mappedKey);
				} else if (ConvertToBoolean (value))
					throw new NotImplementedException ("Persisting security info" +
									   " is not yet implemented");
				break;
			case "POOLING" :
				if (value == null) {
					_pooling = DEF_POOLING;
					base.Remove (mappedKey);
				} else 
					this.Pooling = ConvertToBoolean (value);
				break;
			case "USER ID" :
				if (value == null) {
					_userID = DEF_USERID;
					base.Remove (mappedKey);
				} else 
					this.UserID = value.ToString ();
				break;
			case "UNICODE" :
				if (value == null) {
					_unicode = DEF_UNICODE;
					base.Remove (mappedKey);
				} else 
					this.Unicode = ConvertToBoolean (value);
				break;
			case "OMIT ORACLE CONNECTION NAME" :
				if (value == null) {
					_pooling = DEF_OMITORACLECONNECTIONNAME;
					base.Remove (mappedKey);
				} else 
					this.OmitOracleConnectionName = ConvertToBoolean (value);
				break;

			default :
				throw new ArgumentException("Keyword not supported :" + key);
			}
		}

		private static int ConvertToInt32 (object value) 
		{
			return Int32.Parse (value.ToString (), CultureInfo.InvariantCulture);
		}

		private static bool ConvertToBoolean (object value) 
		{
			if (value == null)
				throw new ArgumentNullException ("null value cannot be converted" +
								 " to boolean");
			string upper = value.ToString ().ToUpper ().Trim ();
			if (upper == "YES" || upper == "TRUE")
				return true;
			if (upper == "NO" || upper == "FALSE")
				return false;
			throw new ArgumentException (String.Format ("Invalid boolean value: {0}",
								    value.ToString ()));
		}

		#endregion // Private Methods
	}
 
	
}
#endif // NET_2_0


