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
using System.EnterpriseServices;

namespace System.Data.Odbc
{
	[DefaultEvent("InfoMessage")]
	public sealed class OdbcConnection : Component, ICloneable, IDbConnection
	{
		#region Fields

		string connectionString;
		int connectionTimeout;
		internal OdbcTransaction transaction;
		IntPtr henv=IntPtr.Zero, hdbc=IntPtr.Zero;
		bool disposed = false;
		
		#endregion

		#region Constructors
		
		public OdbcConnection ()
		{
			connectionTimeout = 15;
			connectionString = null;
		}

		public OdbcConnection (string connectionString) : this ()
		{
			ConnectionString = connectionString;
		}

		#endregion // Constructors

		#region Properties

		internal IntPtr hDbc
		{
			get { return hdbc; }
		}
	
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
		
		[OdbcDescriptionAttribute ("Current connection timeout value, not settable  in the ConnectionString")]
		[DefaultValue (15)]	
		public int ConnectionTimeout {
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

//		public string DataSource {
//			get {
//				if (State==ConnectionState.Open)
//					return _dsn;
//				else
//					return null;
//			}
//		}
		
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
                [OdbcDescriptionAttribute ("Current data source Catlog value, 'Database=X' in the ConnectionString")]
		public string Database {
			get {
				return "";
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
                [OdbcDescriptionAttribute ("The ConnectionState indicating whether the connection is open or closed")]
                [BrowsableAttribute (false)]		
		public ConnectionState State
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
		public string DataSource {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
                [OdbcDescriptionAttribute ("Current ODBC Driver")]
                public string Driver {
                        get {
                                throw new NotImplementedException ();
                        }
                }
		
		[MonoTODO]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
                [OdbcDescriptionAttribute ("Version of the product accessed by the ODBC Driver")]
                [BrowsableAttribute (false)]
                public string ServerVersion {
                        get {
                                throw new NotImplementedException ();
                        }
                }

		
		#endregion // Properties
	
		#region Methods
	
		public OdbcTransaction BeginTransaction ()
		{
			return BeginTransaction(IsolationLevel.Unspecified);
        }
              
		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return (IDbTransaction) BeginTransaction();
		}
		
		public OdbcTransaction BeginTransaction (IsolationLevel level)
		{
			if (transaction==null)
			{
				transaction=new OdbcTransaction(this,level);
				return transaction;
			}
			else
				throw new InvalidOperationException();
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel level)
		{
			return (IDbTransaction) BeginTransaction(level);
		}

		public void Close ()
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

		public OdbcCommand CreateCommand ()
		{
			return new OdbcCommand("", this, transaction); 
		}

		[MonoTODO]
		public void ChangeDatabase(string Database)
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

		IDbCommand IDbConnection.CreateCommand ()
		{
			return (IDbCommand) CreateCommand ();
		}

		public void Open ()
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
			if (connectionString.ToLower().IndexOf("dsn=")>=0)
			{
				string _uid="", _pwd="", _dsn="";
				string[] items=connectionString.Split(new char[1]{';'});
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
				ret=libodbc.SQLDriverConnect(hdbc, IntPtr.Zero, connectionString, -3, 
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
		public void EnlistDistributedTransaction ( ITransaction transaction) {
			throw new NotImplementedException ();
		}


		#endregion

		#region Events and Delegates

		[OdbcDescription ("DbConnection_StateChange")]
                [OdbcCategory ("DataCategory_StateChange")]
		public event StateChangeEventHandler StateChange;

 		[OdbcDescription ("DbConnection_InfoMessage")]
                [OdbcCategory ("DataCategory_InfoMessage")]
		public event OdbcInfoMessageEventHandler InfoMessage;

		#endregion
	}
}
