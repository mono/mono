//
// System.Data.SqlClient.SqlConnectionStringBuilder.cs
//
// Author:
//   Sureshkumar T (tsureshkumar@novell.com)
//
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
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;


using System.Data;
using System.Data.Common;
using System.ComponentModel;

namespace System.Data.SqlClient
{
	[DefaultPropertyAttribute ("DataSource")]
	public sealed class SqlConnectionStringBuilder : DbConnectionStringBuilder
	{

		private const string 	DEF_APPLICATIONNAME 		= ".NET SqlClient Data Provider";
		private const bool 	DEF_ASYNCHRONOUSPROCESSING 	= false;
		private const string 	DEF_ATTACHDBFILENAME 		= "";
		private const bool 	DEF_CONNECTIONRESET 		= true;
		private const int 	DEF_CONNECTTIMEOUT 		= 15;
		private const string 	DEF_CURRENTLANGUAGE 		= "";
		private const string 	DEF_DATASOURCE 			= "";
		private const bool 	DEF_ENCRYPT 			= false;
		private const bool 	DEF_ENLIST 			= false;
		private const string 	DEF_FAILOVERPARTNER 		= "";
		private const string 	DEF_INITIALCATALOG 		= "";
		private const bool 	DEF_INTEGRATEDSECURITY 		= false;
		private const int 	DEF_LOADBALANCETIMEOUT 		= 0;
		private const int 	DEF_MAXPOOLSIZE 		= 100;
		private const int 	DEF_MINPOOLSIZE 		= 0;
		private const bool 	DEF_MULTIPLEACTIVERESULTSETS 	= false;
		private const string 	DEF_NETWORKLIBRARY 		= "";
		private const int 	DEF_PACKETSIZE 			= 8000;
		private const string 	DEF_PASSWORD 			= "";
		private const bool 	DEF_PERSISTSECURITYINFO		= false;
		private const bool 	DEF_POOLING 			= true;
		private const bool 	DEF_REPLICATION 		= false;
		private const string 	DEF_USERID 			= "";
		private const string 	DEF_WORKSTATIONID 		= "";
		private const string 	DEF_TYPESYSTEMVERSION 		= "Latest";
		private const bool	DEF_TRUSTSERVERCERTIFICATE	= false;
		private const bool	DEF_USERINSTANCE		= false;  
		private const bool	DEF_CONTEXTCONNECTION		= false;	


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
		private string 	_failoverPartner;
		private string 	_initialCatalog;
		private bool 	_integratedSecurity;
		private int 	_loadBalanceTimeout;
		private int 	_maxPoolSize;
		private int 	_minPoolSize;
		private bool 	_multipleActiveResultSets;
		private string 	_networkLibrary;
		private int 	_packetSize;
		private string 	_password;
		private bool 	_persistSecurityInfo;
		private bool 	_pooling;
		private bool 	_replication;
		private string 	_userID;
		private string 	_workstationID;
		private bool	_trustServerCertificate;
		private string	_typeSystemVersion;
		private bool	_userInstance;
		private bool	_contextConnection;

		private static Dictionary <string, string> _keywords; // for mapping duplicate keywords
		#endregion // Fields

		#region Constructors
		public SqlConnectionStringBuilder () : this (String.Empty)
		{
		}

		public SqlConnectionStringBuilder (string connectionString)
		{
			Init ();
			base.ConnectionString = connectionString;
		}

		static SqlConnectionStringBuilder ()
		{
			_keywords = new Dictionary <string, string> ();
			_keywords ["APP"] 			= "Application Name";
			_keywords ["APPLICATION NAME"] 		= "Application Name";
			_keywords ["ATTACHDBFILENAME"] 		= "AttachDBFilename";
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
			_keywords ["MULTIPLEACTIVERESULTSETS"] 	= "Multipleactiveresultset";
			_keywords ["ASYNCHRONOUS PROCESSING"] 	= "Asynchronous Processing";
			_keywords ["ASYNC"] 			= "Async";
			_keywords ["NET"] 			= "Network Library";
			_keywords ["NETWORK"] 			= "Network Library";
			_keywords ["NETWORK LIBRARY"] 		= "Network Library";
			_keywords ["PACKET SIZE"] 		= "Packet Size";
			_keywords ["PASSWORD"] 			= "Password";
			_keywords ["PWD"] 			= "Password";
			_keywords ["PERSISTSECURITYINFO"]	= "Persist Security Info";
			_keywords ["PERSIST SECURITY INFO"] 	= "Persist Security Info";
			_keywords ["POOLING"] 			= "Pooling";
			_keywords ["UID"] 			= "User Id";
			_keywords ["USER"] 			= "User Id";
			_keywords ["USER ID"] 			= "User Id";
			_keywords ["WSID"] 			= "Workstation Id";
			_keywords ["WORKSTATION ID"]		= "Workstation Id";
			_keywords ["USER INSTANCE"]		= "User Instance";
			_keywords ["CONTEXT CONNECTION"]	= "Context Connection";
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

		[DisplayNameAttribute ("Failover Partner")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string FailoverPartner { 
			get { return _failoverPartner; }
			set { 
				base ["Failover Partner"] = value;
				_failoverPartner = value; 
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
				return base [mapped]; 
			}
			set {SetValue (keyword, value);}
		}

		public override ICollection Keys { 
			get { return base.Keys; }
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

		[DisplayNameAttribute ("MultipleActiveResultSets")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public bool MultipleActiveResultSets { 
			get { return _multipleActiveResultSets; }
			set { 
				base ["Multiple Active Resultsets"] = value;
				_multipleActiveResultSets = value; 
			}
		}

		[DisplayNameAttribute ("Network Library")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string NetworkLibrary { 
			get { return _networkLibrary; }
			set { 
				base ["Network Library"] = value;
				_networkLibrary = value; 
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

		[DisplayNameAttribute ("Replication")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public bool Replication { 
			get { return _replication; }
			set { 
				base ["Replication"] = value;
				_replication = value; 
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
			get { return base.Values; }
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

		public bool TrustServerCertificate { 
			get { return _trustServerCertificate; } 
			set {
				base ["Trust Server Certificate"] = value;
				 _trustServerCertificate = value;
			}
		}

		public string TypeSystemVersion { 
			get { return _typeSystemVersion; } 
			set {
				base ["Type System Version"] = value;				
				_typeSystemVersion = value; 
			}
		}

		public bool UserInstance { 
			get { return _userInstance; }
			set { 
				base ["User Instance"] = value;
				_userInstance = value;
			}
		}

		public bool ContextConnection { 
			get { return _contextConnection; }
			set { 
				base ["Context Connection"] = value;
				_contextConnection = value;
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
			_failoverPartner	= DEF_FAILOVERPARTNER;
			_initialCatalog		= DEF_INITIALCATALOG;
			_integratedSecurity	= DEF_INTEGRATEDSECURITY;
			_loadBalanceTimeout	= DEF_LOADBALANCETIMEOUT;
			_maxPoolSize		= DEF_MAXPOOLSIZE;
			_minPoolSize		= DEF_MINPOOLSIZE;
			_multipleActiveResultSets= DEF_MULTIPLEACTIVERESULTSETS;
			_networkLibrary		= DEF_NETWORKLIBRARY;
			_packetSize		= DEF_PACKETSIZE;
			_password		= DEF_PASSWORD;
			_persistSecurityInfo	= DEF_PERSISTSECURITYINFO;
			_pooling		= DEF_POOLING;
			_replication		= DEF_REPLICATION;
			_userID			= DEF_USERID;
			_workstationID		= DEF_WORKSTATIONID;
			_trustServerCertificate	= DEF_TRUSTSERVERCERTIFICATE;
			_typeSystemVersion	= DEF_TYPESYSTEMVERSION;
			_userInstance		= DEF_USERINSTANCE;
			_contextConnection	= DEF_CONTEXTCONNECTION;
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
					this.ConnectTimeout = DbConnectionStringBuilderHelper.ConvertToInt32 (value);
				break;
			case "CONNECTION LIFETIME" :
				break;
			case "CONNECTION RESET" :
				if (value == null) {
					_connectionReset = DEF_CONNECTIONRESET;
					base.Remove (mappedKey);
				} else 
					this.ConnectionReset = DbConnectionStringBuilderHelper.ConvertToBoolean (value);
				break;
			case "CURRENT LANGUAGE" :
				if (value == null) {
					_currentLanguage = DEF_CURRENTLANGUAGE;
					base.Remove (mappedKey);
				} else 
					this.CurrentLanguage = value.ToString ();
				break;
			case "CONTEXT CONNECTION" :
				if (value == null) {
					_contextConnection = DEF_CONTEXTCONNECTION;
					base.Remove (mappedKey);
				} else
					this.ContextConnection = DbConnectionStringBuilderHelper.ConvertToBoolean (value);
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
				}else if (DbConnectionStringBuilderHelper.ConvertToBoolean(value))
					throw new NotImplementedException("SSL encryption for"
									  + " data sent between client and server is not"
									  + " implemented.");
				break;
			case "ENLIST" :
				if (value == null) {
					_enlist = DEF_ENLIST;
					base.Remove (mappedKey);
				} else if ( ! DbConnectionStringBuilderHelper.ConvertToBoolean(value))
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
					this.IntegratedSecurity = DbConnectionStringBuilderHelper.ConvertToBoolean (value);
				break;
			case "MAX POOL SIZE" :
				if (value == null) {
					_maxPoolSize = DEF_MAXPOOLSIZE;
					base.Remove (mappedKey);
				} else 
					this.MaxPoolSize = DbConnectionStringBuilderHelper.ConvertToInt32 (value);
				break;
			case "MIN POOL SIZE" :
				if (value == null) {
					_minPoolSize = DEF_MINPOOLSIZE;
					base.Remove (mappedKey);
				} else 
					this.MinPoolSize = DbConnectionStringBuilderHelper.ConvertToInt32 (value);
				break;
			case "MULTIPLEACTIVERESULTSETS":
				if (value == null) {
					_multipleActiveResultSets = DEF_MULTIPLEACTIVERESULTSETS;
					base.Remove (mappedKey);
				} else if ( DbConnectionStringBuilderHelper.ConvertToBoolean (value))
					throw new NotImplementedException ("MARS is not yet implemented!");
				break;
			case "ASYNCHRONOUS PROCESSING" :
				if (value == null) {
					_asynchronousProcessing = DEF_ASYNCHRONOUSPROCESSING;
					base.Remove (mappedKey);
				} else 
					this.AsynchronousProcessing = DbConnectionStringBuilderHelper.ConvertToBoolean (value);
				break;
			case "NETWORK LIBRARY" :
				if (value == null) {
					_networkLibrary = DEF_NETWORKLIBRARY;
					base.Remove (mappedKey);
				} else {
					if (!value.ToString ().ToUpper ().Equals ("DBMSSOCN"))
						throw new ArgumentException ("Unsupported network library.");
					this.NetworkLibrary = value.ToString ().ToLower ();
				}
				break;
			case "PACKET SIZE" :
				if (value == null) {
					_packetSize = DEF_PACKETSIZE;
					base.Remove (mappedKey);
				} else 
					this.PacketSize = DbConnectionStringBuilderHelper.ConvertToInt32 (value);
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
				} else if (DbConnectionStringBuilderHelper.ConvertToBoolean (value))
					throw new NotImplementedException ("Persisting security info" +
									   " is not yet implemented");
				break;
			case "POOLING" :
				if (value == null) {
					_pooling = DEF_POOLING;
					base.Remove (mappedKey);
				} else 
					this.Pooling = DbConnectionStringBuilderHelper.ConvertToBoolean (value);
				break;
			case "USER ID" :
				if (value == null) {
					_userID = DEF_USERID;
					base.Remove (mappedKey);
				} else 
					this.UserID = value.ToString ();
				break;
			case "USER INSTANCE" :
				if (value == null) {
					_userInstance = DEF_USERINSTANCE;
					base.Remove (mappedKey);
				} else
					this.UserInstance = DbConnectionStringBuilderHelper.ConvertToBoolean (value);
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
		#endregion // Private Methods
	}
 
	
}
#endif // NET_2_0
