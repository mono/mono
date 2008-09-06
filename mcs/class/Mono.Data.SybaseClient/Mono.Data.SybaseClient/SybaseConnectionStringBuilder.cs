//
// Mono.Data.SybaseClient.SybaseConnectionStringBuilder.cs
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

namespace Mono.Data.SybaseClient
{
	[DefaultPropertyAttribute ("DataSource")]
	public sealed class SybaseConnectionStringBuilder : DbConnectionStringBuilder
	{

		private const string 	DEF_APPLICATIONNAME 		= ".NET Sybase Data Provider";
		private const bool 	DEF_ASYNCHRONOUSPROCESSING 	= false;
		private const string 	DEF_ATTACHDBFILENAME 		= "";
		private const bool 	DEF_CONNECTIONRESET 		= true;
		private const int 	DEF_CONNECTTIMEOUT 		= 15;
		private const string 	DEF_CURRENTLANGUAGE 		= "";
		private const string 	DEF_DATASOURCE 			= "";
		private const bool 	DEF_ENCRYPT 			= false;
		private const bool 	DEF_ENLIST 			= false;
		private const string 	DEF_INITIALCATALOG 		= "";
		private const bool 	DEF_INTEGRATEDSECURITY 		= false;
		private const int 	DEF_LOADBALANCETIMEOUT 		= 0;
		private const int 	DEF_MAXPOOLSIZE 		= 100;
		private const int 	DEF_MINPOOLSIZE 		= 0;
		private const int 	DEF_PACKETSIZE 			= 8000;
		private const string 	DEF_PASSWORD 			= "";
		private const bool 	DEF_PERSISTSECURITYINFO		= false;
		private const bool 	DEF_POOLING 			= true;
		private const bool 	DEF_REPLICATION 		= false;
		private const string 	DEF_USERID 			= "";
		private const string 	DEF_WORKSTATIONID 		= "";

		#region // Fields
 		private string 	_applicationName;
		private bool 	_asynchronousProcessing;
		private string 	_attachDBFilename;
		private bool 	_connectionReset;
		private int 	_connectTimeout;
		private string 	_currentLanguage;
		private string 	_dataSource;
		private bool 	_encrypt;
		private bool 	_enlist;
		private string 	_initialCatalog;
		private bool 	_integratedSecurity;
		private int 	_loadBalanceTimeout;
		private int 	_maxPoolSize;
		private int 	_minPoolSize;
		private int 	_packetSize;
		private string 	_password;
		private bool 	_persistSecurityInfo;
		private bool 	_pooling;
		private string 	_userID;
		private string 	_workstationID;

		private static Dictionary <string, string> _keywords; // for mapping duplicate keywords
		private static Dictionary <string, object> _defaults; 
		#endregion // Fields

		#region Constructors
		public SybaseConnectionStringBuilder () : this (String.Empty)
		{
		}

		public SybaseConnectionStringBuilder (string connectionString)
		{
			Init ();
			base.ConnectionString = connectionString;
		}

		static SybaseConnectionStringBuilder ()
		{
			_keywords = new Dictionary <string, string> ();
			_keywords ["APP"] 			= "Application Name";
			_keywords ["APPLICATION NAME"] 		= "Application Name";
			_keywords ["ATTACHDBFILENAME"] 		= "AttachDbFilename";
			_keywords ["EXTENDED PROPERTIES"] 	= "Extended Properties";
			_keywords ["INITIAL FILE NAME"]		= "Initial File Name";
			_keywords ["TIMEOUT"] 			= "Connect Timeout";
			_keywords ["CONNECT TIMEOUT"] 		= "Connect Timeout";
			_keywords ["CONNECTION TIMEOUT"]	= "Connect Timeout";
			_keywords ["CONNECTION RESET"] 		= "Connection Reset";
			_keywords ["LANGUAGE"] 			= "Current Language";
			_keywords ["CURRENT LANGUAGE"] 		= "Current Language";
			_keywords ["DATA SOURCE"] 		= "Data Source";
			_keywords ["SERVER"] 			= "Data Source";
			_keywords ["ADDRESS"] 			= "Data Source";
			_keywords ["ADDR"] 			= "Data Source";
			_keywords ["NETWORK ADDRESS"] 		= "Data Source";
			_keywords ["ENCRYPT"] 			= "Encrypt";
			_keywords ["ENLIST"] 			= "Enlist";
			_keywords ["INITIAL CATALOG"] 		= "Initial Catalog";
			_keywords ["DATABASE"] 			= "Initial Catalog";
			_keywords ["INTEGRATED SECURITY"]	= "Integrated Security";
			_keywords ["TRUSTED_CONNECTION"] 	= "Integrated Security";
			_keywords ["MAX POOL SIZE"] 		= "Max Pool Size";
			_keywords ["MIN POOL SIZE"] 		= "Min Pool Size";
			_keywords ["ASYNCHRONOUS PROCESSING"] 	= "Asynchronous Processing";
			_keywords ["ASYNC"] 			= "Async";
			_keywords ["PACKET SIZE"] 		= "Packet Size";
			_keywords ["PASSWORD"] 			= "Password";
			_keywords ["PWD"] 			= "Password";
			_keywords ["PERSISTSECURITYINFO"]	= "Persist Security Info";
			_keywords ["PERSIST SECURITY INFO"] 	= "Persist Security Info";
			_keywords ["POOLING"] 			= "Pooling";
			_keywords ["UID"] 			= "User ID";
			_keywords ["USER"] 			= "User ID";
			_keywords ["USER ID"] 			= "User ID";
			_keywords ["WSID"] 			= "Workstation ID";
			_keywords ["WORKSTATION ID"]		= "Workstation ID";

			_defaults = new Dictionary<string, object> ();
			_defaults.Add("Data Source", DEF_DATASOURCE);
			_defaults.Add("AttachDbFilename", DEF_ATTACHDBFILENAME);
			_defaults.Add("Initial Catalog", DEF_INITIALCATALOG);
			_defaults.Add("Integrated Security", DEF_INTEGRATEDSECURITY);
			_defaults.Add("Persist Security Info", DEF_PERSISTSECURITYINFO);
			_defaults.Add("User ID", DEF_USERID);
			_defaults.Add("Password", DEF_PASSWORD);
			_defaults.Add("Enlist", DEF_ENLIST);
			_defaults.Add("Pooling", DEF_POOLING);
			_defaults.Add("Min Pool Size", DEF_MINPOOLSIZE);
			_defaults.Add("Max Pool Size", DEF_MAXPOOLSIZE);
			_defaults.Add("Asynchronous Processing", DEF_ASYNCHRONOUSPROCESSING);
			_defaults.Add("Connection Reset", DEF_CONNECTIONRESET);
			_defaults.Add("Connect Timeout", DEF_CONNECTTIMEOUT);
			_defaults.Add("Encrypt", DEF_ENCRYPT);
			_defaults.Add("Packet Size", DEF_PACKETSIZE);
			_defaults.Add("Application Name", DEF_APPLICATIONNAME);
			_defaults.Add("Current Language", DEF_CURRENTLANGUAGE);
			_defaults.Add("Workstation ID", DEF_WORKSTATIONID);
		}
		#endregion // Constructors

		#region Properties
		[DisplayNameAttribute ("Application Name")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string ApplicationName { 
			get { return _applicationName; }
			set { 
				base ["Application Name"] = value;
				_applicationName = value; 
			}
		}

		[DisplayNameAttribute ("Asynchronous Processing")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public bool AsynchronousProcessing { 
			get { return _asynchronousProcessing; }
			set { 
				base ["Asynchronous Processing"] = value;
				_asynchronousProcessing = value; 
			}
		}

		[DisplayNameAttribute ("AttachDbFilename")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string AttachDBFilename { 
			get { return _attachDBFilename; }
			set { 
				base ["AttachDbFilename"] = value;
				_attachDBFilename = value; 
			}
		}
		
		[DisplayNameAttribute ("Connection Reset")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public bool ConnectionReset { 
			get { return _connectionReset; }
			set { 
				base ["Connection Reset"] = value;
				_connectionReset = value; 
			}
		}
		
		[DisplayNameAttribute ("Connect Timeout")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public int ConnectTimeout { 
			get { return _connectTimeout; }
			set { 
				base ["Connect Timeout"] = value;
				_connectTimeout = value; 
			}
		}

		[DisplayNameAttribute ("Current Language")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string CurrentLanguage { 
			get { return _currentLanguage; }
			set { 
				base ["Current Language"] = value;
				_currentLanguage = value; 
			}
		}

		[DisplayNameAttribute ("Data Source")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string DataSource { 
			get { return _dataSource; }
			set { 
				base ["Data Source"] = value;
				_dataSource = value; 
			}
		}

		[DisplayNameAttribute ("Encrypt")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public bool Encrypt { 
			get { return _encrypt; }
			set { 
				base ["Encrypt"] = value;
				_encrypt = value; 
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

		[DisplayNameAttribute ("Initial Catalog")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string InitialCatalog { 
			get { return _initialCatalog; }
			set { 
				base ["Initial Catalog"] = value;
				_initialCatalog = value; 
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
				keys.Add("AttachDbFilename");
				keys.Add("Initial Catalog");
				keys.Add("Integrated Security");
				keys.Add("Persist Security Info");
				keys.Add("User ID");
				keys.Add("Password");
				keys.Add("Enlist");
				keys.Add("Pooling");
				keys.Add("Min Pool Size");
				keys.Add("Max Pool Size");
				keys.Add("Asynchronous Processing");
				keys.Add("Connection Reset");
				keys.Add("Connect Timeout");
				keys.Add("Encrypt");
				keys.Add("Packet Size");
				keys.Add("Application Name");
				keys.Add("Current Language");
				keys.Add("Workstation ID");
				ReadOnlyCollection<string> coll = new ReadOnlyCollection<string>(keys);
				return coll;
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

		[DisplayNameAttribute ("Packet Size")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public int PacketSize { 
			get { return _packetSize; }
			set { 
				base ["Packet Size"] = value;
				_packetSize = value; 
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
		
		public override ICollection Values { 
			get {
				List<object> values = new List<object>();
				values.Add(_dataSource);
				values.Add(_attachDBFilename);
				values.Add(_initialCatalog);
				values.Add(_integratedSecurity);
				values.Add(_persistSecurityInfo);
				values.Add(_userID);
				values.Add(_password);
				values.Add(_enlist);
				values.Add(_pooling);
				values.Add(_minPoolSize);
				values.Add(_maxPoolSize);
				values.Add(_asynchronousProcessing);
				values.Add(_connectionReset);
				values.Add(_connectTimeout);
				values.Add(_encrypt);
				values.Add(_loadBalanceTimeout);
				values.Add(_packetSize);
				values.Add(_applicationName);
				values.Add(_currentLanguage);
				values.Add(_workstationID);
				ReadOnlyCollection<object> coll = new ReadOnlyCollection<object>(values);
				return coll;		 
			}
		}

		[DisplayNameAttribute ("Workstation ID")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string WorkstationID { 
			get { return _workstationID; }
			set { 
				base ["Workstation Id"] = value;
				_workstationID = value; 
			}
		}

		#endregion // Properties

		#region Methods
		private void Init ()
		{
			_applicationName 	= DEF_APPLICATIONNAME;
			_asynchronousProcessing	= DEF_ASYNCHRONOUSPROCESSING;
			_attachDBFilename	= DEF_ATTACHDBFILENAME;
			_connectionReset	= DEF_CONNECTIONRESET;
			_connectTimeout		= DEF_CONNECTTIMEOUT;
			_currentLanguage	= DEF_CURRENTLANGUAGE;
			_dataSource		= DEF_DATASOURCE;
			_encrypt		= DEF_ENCRYPT;
			_enlist			= DEF_ENLIST;
			_initialCatalog		= DEF_INITIALCATALOG;
			_integratedSecurity	= DEF_INTEGRATEDSECURITY;
			_loadBalanceTimeout	= DEF_LOADBALANCETIMEOUT;
			_maxPoolSize		= DEF_MAXPOOLSIZE;
			_minPoolSize		= DEF_MINPOOLSIZE;
			_packetSize		= DEF_PACKETSIZE;
			_password		= DEF_PASSWORD;
			_persistSecurityInfo	= DEF_PERSISTSECURITYINFO;
			_pooling		= DEF_POOLING;
			_userID			= DEF_USERID;
			_workstationID		= DEF_WORKSTATIONID;
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
			case "APPLICATION NAME" :
				if (value == null) {
					_applicationName = DEF_APPLICATIONNAME;
					base.Remove (mappedKey);
				} else
					this.ApplicationName = value.ToString ();
				break;
			case "ATTACHDBFILENAME" :
				throw new NotImplementedException ("Attachable database support is " +
								   "not implemented.");
			case "CONNECT TIMEOUT" :
				if (value == null) {
					_connectTimeout = DEF_CONNECTTIMEOUT;
					base.Remove (mappedKey);
				} else 
					this.ConnectTimeout = ConvertToInt32 (value);
				break;
			case "CONNECTION LIFETIME" :
				break;
			case "CONNECTION RESET" :
				if (value == null) {
					_connectionReset = DEF_CONNECTIONRESET;
					base.Remove (mappedKey);
				} else 
					this.ConnectionReset = ConvertToBoolean (value);
				break;
			case "CURRENT LANGUAGE" :
				if (value == null) {
					_currentLanguage = DEF_CURRENTLANGUAGE;
					base.Remove (mappedKey);
				} else 
					this.CurrentLanguage = value.ToString ();
				break;
			case "DATA SOURCE" :
				if (value == null) {
					_dataSource = DEF_DATASOURCE;
					base.Remove (mappedKey);
				} else 
					this.DataSource = value.ToString ();
				break;
			case "ENCRYPT":
				if (value == null) {
					_encrypt = DEF_ENCRYPT;
					base.Remove (mappedKey);
				}else if (ConvertToBoolean(value))
					throw new NotImplementedException("SSL encryption for"
									  + " data sent between client and server is not"
									  + " implemented.");
				break;
			case "ENLIST" :
				if (value == null) {
					_enlist = DEF_ENLIST;
					base.Remove (mappedKey);
				} else if (!ConvertToBoolean(value))
					throw new NotImplementedException("Disabling the automatic"
									  + " enlistment of connections in the thread's current"
									  + " transaction context is not implemented.");
				break;
			case "INITIAL CATALOG" :
				if (value == null) {
					_initialCatalog = DEF_INITIALCATALOG;
					base.Remove (mappedKey);
				} else 
					this.InitialCatalog = value.ToString ();
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
			case "ASYNCHRONOUS PROCESSING" :
				if (value == null) {
					_asynchronousProcessing = DEF_ASYNCHRONOUSPROCESSING;
					base.Remove (mappedKey);
				} else 
					this.AsynchronousProcessing = ConvertToBoolean (value);
				break;
			case "PACKET SIZE" :
				if (value == null) {
					_packetSize = DEF_PACKETSIZE;
					base.Remove (mappedKey);
				} else 
					this.PacketSize = ConvertToInt32 (value);
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
			case "WORKSTATION ID" :
				if (value == null) {
					_workstationID = DEF_WORKSTATIONID;
					base.Remove (mappedKey);
				} else 
					this.WorkstationID = value.ToString ();
				break;
			default :
				throw new ArgumentException("Keyword not supported :" + key);
			}
		}

		private int ConvertToInt32 (object value) 
		{
			return Int32.Parse (value.ToString (), CultureInfo.InvariantCulture);
		}

		private bool ConvertToBoolean (object value) 
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


