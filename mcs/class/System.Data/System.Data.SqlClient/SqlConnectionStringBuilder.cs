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

using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient
{
	public sealed class SqlConnectionStringBuilder : DbConnectionStringBuilder
	{

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
		#endregion // Constructors

		#region Properties
		public string ApplicationName { 
			get { return _applicationName; }
			set { 
				base ["Application Name"] = value;
				_applicationName = value; 
			}
		}

		public bool AsynchronousProcessing { 
			get { return _asynchronousProcessing; }
			set { 
				base ["Asynchronous Processing"] = value;
				_asynchronousProcessing = value; 
			}
		}

		public string AttachDBFilename { 
			get { return _attachDBFilename; }
			set { 
				base ["AttachDbFilename"] = value;
				_attachDBFilename = value; 
			}
		}

		public bool ConnectionReset { 
			get { return _connectionReset; }
			set { 
				base ["Connection Reset"] = value;
				_connectionReset = value; 
			}
		}

		public int ConnectTimeout { 
			get { return _connectTimeout; }
			set { 
				base ["Connect Timeout"] = value;
				_connectTimeout = value; 
			}
		}

		public string CurrentLanguage { 
			get { return _currentLanguage; }
			set { 
				base ["Current Language"] = value;
				_currentLanguage = value; 
			}
		}

		public string DataSource { 
			get { return _dataSource; }
			set { 
				base ["Data Source"] = value;
				_dataSource = value; 
			}
		}

		public bool Encrypt { 
			get { return _encrypt; }
			set { 
				base ["Encrypt"] = value;
				_encrypt = value; 
			}
		}

		public bool Enlist { 
			get { return _enlist; }
			set { 
				base ["Enlist"] = value;
				_enlist = value; 
			}
		}

		public string FailoverPartner { 
			get { return _failoverPartner; }
			set { 
				base ["Failover Partner"] = value;
				_failoverPartner = value; 
			}
		}

		public string InitialCatalog { 
			get { return _initialCatalog; }
			set { 
				base ["Initial Catalog"] = value;
				_initialCatalog = value; 
			}
		}

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
			get { return base [keyword]; }
			set { SetValue (keyword, value); }
		}

		public override ICollection Keys { 
			get { return base.Keys; }
		}

		public int LoadBalanceTimeout { 
			get { return _loadBalanceTimeout; }
			set { 
				base ["Load Balance Timeout"] = value;
				_loadBalanceTimeout = value; 
			}
		}

		public int MaxPoolSize { 
			get { return _maxPoolSize; }
			set { 
				base ["Max Pool Size"] = value;
				_maxPoolSize = value; 
			}
		}

		public int MinPoolSize { 
			get { return _minPoolSize; }
			set {
				base ["Min Pool Size"] = value;
				_minPoolSize = value; 
			}
		}

		public bool MultipleActiveResultSets { 
			get { return _multipleActiveResultSets; }
			set { 
				base ["Multiple Active Resultsets"] = value;
				_multipleActiveResultSets = value; 
			}
		}

		public string NetworkLibrary { 
			get { return _networkLibrary; }
			set { 
				base ["Network Library"] = value;
				_networkLibrary = value; 
			}
		}

		public int PacketSize { 
			get { return _packetSize; }
			set { 
				base ["Packet Size"] = value;
				_packetSize = value; 
			}
		}

		public string Password { 
			get { return _password; }
			set { 
				base ["Password"] = value;
				_password = value; 
			}
		}

		public bool PersistSecurityInfo { 
			get { return _persistSecurityInfo; }
			set {
				base ["Persist Security Info"] = value;
				_persistSecurityInfo = value; 
			}
		}

		public bool Pooling { 
			get { return _pooling; }
			set {
				base ["Pooling"] = value;
				_pooling = value; 
			}
		}

		public bool Replication { 
			get { return _replication; }
			set { 
				base ["Replication"] = value;
				_replication = value; 
			}
		}

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
			_applicationName 	= ".NET SqlClient Data Provider";
			_asynchronousProcessing	= false;
			_attachDBFilename	= String.Empty;
			_connectionReset	= true;
			_connectTimeout		= 15;
			_currentLanguage	= String.Empty;
			_dataSource		= String.Empty;
			_encrypt		= false;
			_enlist			= true;
			_failoverPartner	= String.Empty;
			_initialCatalog		= String.Empty;
			_integratedSecurity	= false;
			_loadBalanceTimeout	= 0;
			_maxPoolSize		= 100;
			_minPoolSize		= 0;
			_multipleActiveResultSets= true;
			_networkLibrary		= String.Empty;
			_packetSize		= 8000;
			_password		= String.Empty;
			_persistSecurityInfo	= false;
			_pooling		= true;
			_replication		= false;
			_userID			= String.Empty;
			_workstationID		= String.Empty;
		}

		public override void Clear ()
		{
			base.Clear ();
			Init ();
		}

		public override bool ContainsKey (string keyword)
		{
			return base.ContainsKey (keyword);
		}

		public override bool Remove (string keyword)
		{
			return base.Remove (keyword);
		}

		public override bool ShouldSerialize (string keyword)
		{
			return base.ShouldSerialize (keyword);
		}

		public override bool TryGetValue (string keyword, out object value)
		{
			return base.TryGetValue (keyword, out value);
		}

		#endregion // Methods

		#region Private Methods
		private void SetValue (string key, object value)
		{
			if (key == null)
				throw new ArgumentNullException ("key cannot be null!");

			switch (key.ToUpper ().Trim ()) {
			case "APP" :
			case "APPLICATION NAME" :
				this.ApplicationName = value.ToString ();
				break;
			case "ATTACHDBFILENAME" :
			case "EXTENDED PROPERTIES" :
			case "INITIAL FILE NAME" :
				throw new NotImplementedException ("Attachable database support is " +
								   "not implemented.");
			case "TIMEOUT" :
			case "CONNECT TIMEOUT" :
			case "CONNECTION TIMEOUT" :
				this.ConnectTimeout = DbConnectionStringBuilderHelper.ConvertToInt32 (value);
				break;
			case "CONNECTION LIFETIME" :
				break;
			case "CONNECTION RESET" :
				this.ConnectionReset = DbConnectionStringBuilderHelper.ConvertToBoolean (value);
				break;
			case "LANGUAGE" :
			case "CURRENT LANGUAGE" :
				this.CurrentLanguage = value.ToString ();
				break;
			case "DATA SOURCE" :
			case "SERVER" :
			case "ADDRESS" :
			case "ADDR" :
			case "NETWORK ADDRESS" :
				this.DataSource = value.ToString ();
				break;
			case "ENCRYPT":
				if (DbConnectionStringBuilderHelper.ConvertToBoolean(value))
					throw new NotImplementedException("SSL encryption for"
									  + " data sent between client and server is not"
									  + " implemented.");
				break;
			case "ENLIST" :
				if ( ! DbConnectionStringBuilderHelper.ConvertToBoolean(value))
					throw new NotImplementedException("Disabling the automatic"
									  + " enlistment of connections in the thread's current"
									  + " transaction context is not implemented.");
				break;
			case "INITIAL CATALOG" :
			case "DATABASE" :
				this.InitialCatalog = value.ToString ();
				break;
			case "INTEGRATED SECURITY" :
			case "TRUSTED_CONNECTION" :
				this.IntegratedSecurity = DbConnectionStringBuilderHelper.ConvertToBoolean (value);
				break;
			case "MAX POOL SIZE" :
				this.MaxPoolSize = DbConnectionStringBuilderHelper.ConvertToInt32 (value);
				break;
			case "MIN POOL SIZE" :
				this.MinPoolSize = DbConnectionStringBuilderHelper.ConvertToInt32 (value);
				break;
			case "MULTIPLEACTIVERESULTSETS":
				if ( DbConnectionStringBuilderHelper.ConvertToBoolean (value))
					throw new NotImplementedException ("MARS is not yet implemented!");
				break;
			case "ASYNCHRONOUS PROCESSING" :
			case "ASYNC" :
				this.AsynchronousProcessing = DbConnectionStringBuilderHelper.ConvertToBoolean (value);
				break;
			case "NET" :
			case "NETWORK" :
			case "NETWORK LIBRARY" :
				if (!value.ToString ().ToUpper ().Equals ("DBMSSOCN"))
					throw new ArgumentException ("Unsupported network library.");
				this.NetworkLibrary = value.ToString ().ToLower ();
				break;
			case "PACKET SIZE" :
				this.PacketSize = DbConnectionStringBuilderHelper.ConvertToInt32 (value);
				break;
			case "PASSWORD" :
			case "PWD" :
				this.Password = value.ToString ();
				break;
			case "PERSISTSECURITYINFO" :
			case "PERSIST SECURITY INFO" :
				if (DbConnectionStringBuilderHelper.ConvertToBoolean (value))
					throw new NotImplementedException ("Persisting security info" +
									   " is not yet implemented");
				break;
			case "POOLING" :
				this.Pooling = DbConnectionStringBuilderHelper.ConvertToBoolean (value);
				break;
			case "UID" :
			case "USER" :
			case "USER ID" :
				this.UserID = value.ToString ();
				break;
			case "WSID" :
			case "WORKSTATION ID" :
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
