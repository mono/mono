//
// System.Data.Odbc.OdbcConnection
//
// Authors:
//  Brian Ritchie (brianlritchie@hotmail.com) 
//
// Copyright (C) Brian Ritchie, 2002
//

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

using System.ComponentModel;
using System.Data;
using System.Data.Common;
#if NET_2_0
using System.Data.ProviderBase;
#endif // NET_2_0
using System.EnterpriseServices;

namespace System.Data.Odbc
{
	[DefaultEvent("InfoMessage")]
#if NET_2_0
        public sealed class OdbcConnection : DbConnectionBase, ICloneable
#else
	public sealed class OdbcConnection : Component, ICloneable, IDbConnection
#endif //NET_2_0
	{
		#region Fields

#if ONLY_1_1
		string connectionString;
#endif //ONLY_1_1

		int connectionTimeout;
		internal OdbcTransaction transaction;
		IntPtr henv=IntPtr.Zero, hdbc=IntPtr.Zero;
		bool disposed = false;			
		
		#endregion

		#region Constructors
		
		public OdbcConnection () : this (String.Empty)
		{
		}

		public OdbcConnection (string connectionString)
		{
                        Init (connectionString);
		}

                public void Init (string connectionString)
                {
                        connectionTimeout = 15;
                        ConnectionString = connectionString;
                }

#if NET_2_0
                internal OdbcConnection (OdbcConnectionFactory factory) 
                        : base ( (DbConnectionFactory) factory)
                {
                        Init (String.Empty);
                }
                
#endif //NET_2_0
                

		#endregion // Constructors

		#region Properties

		internal IntPtr hDbc
		{
			get { return hdbc; }
		}

#if ONLY_1_1
		[OdbcCategoryAttribute ("DataCategory_Data")]		
		[DefaultValue ("")]
		[OdbcDescriptionAttribute ("Information used to connect to a Data Source")]	
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Odbc.Design.OdbcConnectionStringEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
                [RecommendedAsConfigurableAttribute (true)]
		public string ConnectionString {
			get {
				return connectionString;
			}
			set {
				connectionString = value;
			}
		}
#endif // ONLY_1_1
		
		[OdbcDescriptionAttribute ("Current connection timeout value, not settable  in the ConnectionString")]
		[DefaultValue (15)]	
		public
#if NET_2_0
                new
#endif // NET_2_0
                int ConnectionTimeout {
			get {
				return connectionTimeout;
			}
			set {
				if (value < 0) {
					throw new ArgumentException("Timout should not be less than zero.");
				}
				connectionTimeout = value;
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
                [OdbcDescriptionAttribute ("Current data source Catlog value, 'Database=X' in the ConnectionString")]
		public
#if NET_2_0
                override
#endif // NET_2_0
                string Database {
			get {
                                return GetInfo (OdbcInfo.DatabaseName);
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
                [OdbcDescriptionAttribute ("The ConnectionState indicating whether the connection is open or closed")]
                [BrowsableAttribute (false)]		
		public
#if NET_2_0
                override
#endif // NET_2_0
                ConnectionState State
		{
			get {
				if (hdbc!=IntPtr.Zero) {
					return ConnectionState.Open;
				}
				else
					return ConnectionState.Closed;
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
                [OdbcDescriptionAttribute ("Current data source, 'Server=X' in the ConnectionString")]
		public
#if NET_2_0
                override
#endif // NET_2_0
                string DataSource {
			get {
                                return GetInfo (OdbcInfo.DataSourceName);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
                [OdbcDescriptionAttribute ("Current ODBC Driver")]
                public string Driver {
                        get {
                                return GetInfo (OdbcInfo.DriverName);
                        }
                }
		
		[MonoTODO]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
                [OdbcDescriptionAttribute ("Version of the product accessed by the ODBC Driver")]
                [BrowsableAttribute (false)]
                public
#if NET_2_0
                override
#endif // NET_2_0
                string ServerVersion {
                        get {
                                return GetInfo (OdbcInfo.DbmsVersion);
                        }
                }

		
		#endregion // Properties
	
		#region Methods
	
		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcTransaction BeginTransaction ()
		{
			return BeginTransaction(IsolationLevel.Unspecified);
                }

#if ONLY_1_1              
		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return (IDbTransaction) BeginTransaction();
		}
#endif // ONLY_1_1
		
		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcTransaction BeginTransaction (IsolationLevel level)
		{
			if (transaction==null)
			{
				transaction=new OdbcTransaction(this,level);
				return transaction;
			}
			else
				throw new InvalidOperationException();
		}

#if ONLY_1_1
		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel level)
		{
			return (IDbTransaction) BeginTransaction(level);
		}
#endif // ONLY_1_1

		public
#if NET_2_0
                override
#endif // NET_2_0
                void Close ()
		{
			OdbcReturn ret = OdbcReturn.Error;
			if (State == ConnectionState.Open) {
				// disconnect
				ret = libodbc.SQLDisconnect (hdbc);
				if ( (ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException (new OdbcError ("SQLDisconnect", OdbcHandleType.Dbc,hdbc));

				// free handles
				if (hdbc != IntPtr.Zero) {
					ret = libodbc.SQLFreeHandle ( (ushort) OdbcHandleType.Dbc, hdbc);	
					if ( (ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
						throw new OdbcException (new OdbcError ("SQLFreeHandle", OdbcHandleType.Dbc,hdbc));
				}
				hdbc = IntPtr.Zero;

				if (henv != IntPtr.Zero) {
					ret = libodbc.SQLFreeHandle ( (ushort) OdbcHandleType.Env, henv);	
					if ( (ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
						throw new OdbcException (new OdbcError ("SQLFreeHandle", OdbcHandleType.Env,henv));
				}
				henv = IntPtr.Zero;

				transaction = null;
			}
		}

		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcCommand CreateCommand ()
		{
			return new OdbcCommand("", this, transaction); 
		}

		[MonoTODO]
		public
#if NET_2_0
                override
#endif // NET_2_0
                void ChangeDatabase(string Database)
		{
			throw new NotImplementedException ();
		}
		
		protected override void Dispose (bool disposing)
		{
			if (!this.disposed) {
				try 
				{
					// release the native unmananged resources
					this.Close();
					this.disposed = true;
				}
				finally 
				{
					// call Dispose on the base class
					base.Dispose(disposing);			
				}
			}
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException();
		}

#if ONLY_1_1
		IDbCommand IDbConnection.CreateCommand ()
		{
			return (IDbCommand) CreateCommand ();
		}
#endif //ONLY_1_1

		public
#if NET_2_0
                override
#endif // NET_2_0
                void Open ()
		{
			if (State == ConnectionState.Open)
				throw new InvalidOperationException ();

			OdbcReturn ret = OdbcReturn.Error;
		
			// allocate Environment handle	
			ret = libodbc.SQLAllocHandle (OdbcHandleType.Env, IntPtr.Zero, ref henv);
			if ( (ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException (new OdbcError ("SQLAllocHandle"));
		
			ret=libodbc.SQLSetEnvAttr (henv, OdbcEnv.OdbcVersion, (IntPtr) libodbc.SQL_OV_ODBC3 , 0); 
			if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException (new OdbcError ("SQLSetEnvAttr", OdbcHandleType.Env,henv));
		
			// allocate connection handle
			ret=libodbc.SQLAllocHandle (OdbcHandleType.Dbc, henv, ref hdbc);
			if ( (ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException (new OdbcError ("SQLAllocHandle",OdbcHandleType.Env,henv));
			
			// DSN connection
			if (ConnectionString.ToLower().IndexOf("dsn=")>=0)
			{
				string _uid="", _pwd="", _dsn="";
				string[] items=ConnectionString.Split(new char[1]{';'});
				foreach (string item in items)
				{
					string[] parts=item.Split(new char[1] {'='});
					switch (parts[0].Trim().ToLower())
					{
						case "dsn":
							_dsn=parts[1].Trim();
							break;
						case "uid":
							_uid=parts[1].Trim();
							break;
						case "pwd":
							_pwd=parts[1].Trim();
							break;
					}
				}
				ret=libodbc.SQLConnect(hdbc, _dsn, -3, _uid, -3, _pwd, -3);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLConnect",OdbcHandleType.Dbc,hdbc));
			}
			else 
			{
				// DSN-less Connection
				string OutConnectionString=new String(' ',1024);
				short OutLen=0;
				ret=libodbc.SQLDriverConnect(hdbc, IntPtr.Zero, ConnectionString, -3, 
					OutConnectionString, (short) OutConnectionString.Length, ref OutLen, 0);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLDriverConnect",OdbcHandleType.Dbc,hdbc));
			}

		}

		[MonoTODO]
		public static void ReleaseObjectPool ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public
#if NET_2_0
                override
#endif // NET_2_0
                void EnlistDistributedTransaction ( ITransaction transaction) {
			throw new NotImplementedException ();
		}

                internal string GetInfo (OdbcInfo info)
                {
                        if (State == ConnectionState.Closed)
                                throw new InvalidOperationException ("The connection is closed.");
                        
                        OdbcReturn ret = OdbcReturn.Error;
                        short max_length = 256;
                        byte [] buffer = new byte [max_length];
                        short actualLength = 0;
                        
                        ret = libodbc.SQLGetInfo (hdbc, info, buffer, max_length, ref actualLength);
                        if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
                                throw new OdbcException (new OdbcError ("SQLGetInfo",
                                                                        OdbcHandleType.Dbc,
                                                                        hdbc));

                        return System.Text.Encoding.Default.GetString (buffer);
                }


		#endregion

		#region Events and Delegates

#if ONLY_1_1
		[OdbcDescription ("DbConnection_StateChange")]
                [OdbcCategory ("DataCategory_StateChange")]
		public event StateChangeEventHandler StateChange;
#endif // ONLY_1_1

 		[OdbcDescription ("DbConnection_InfoMessage")]
                [OdbcCategory ("DataCategory_InfoMessage")]
		public event OdbcInfoMessageEventHandler InfoMessage;

		#endregion
	}
}
