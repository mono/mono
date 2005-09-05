/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/overview/*'/>
#if	(!NETCF)
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(FbConnection), "Resources.FbConnection.bmp")]
	[DefaultEvent("InfoMessage")]
#endif
	public sealed class FbConnection : Component, IDbConnection, ICloneable
	{
		#region Events

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/event[@name="StateChange"]/*'/>
		public event StateChangeEventHandler StateChange;

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/event[@name="InfoMessage"]/*'/>
		public event FbInfoMessageEventHandler InfoMessage;

		#endregion

		#region Fields

		private FbConnectionInternal innerConnection;
		private ConnectionState		state;
		private FbConnectionString	options;
		private bool				disposed;
		private string				connectionString;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ConnectionString"]/*'/>
#if	(NET)
		[Category("Data"), RecommendedAsConfigurableAttribute(true), RefreshProperties(RefreshProperties.All), DefaultValue("")]
		[Editor(typeof(Design.FbConnectionStringUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
#endif
		public string ConnectionString
		{
			get { return this.connectionString; }
			set
			{
				lock (this)
				{
					if (this.state == ConnectionState.Closed)
					{
						if (value == null)
						{
							value = "";
						}

						this.options.Load(value);
						this.options.Validate();
						this.connectionString = value;
					}
				}
			}
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ConnectionTimeout"]/*'/>
#if	(!NETCF)
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
		public int ConnectionTimeout
		{
			get { return this.options.ConnectionTimeout; }
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="Database"]/*'/>
#if	(!NETCF)
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
		public string Database
		{
			get { return this.options.Database; }
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="DataSource"]/*'/>
#if	(!NETCF)
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
		public string DataSource
		{
			get { return this.options.DataSource; }
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ServerVersion"]/*'/>
#if	(!NETCF)
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
		public string ServerVersion
		{
			get
			{
				if (this.state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("The connection is closed.");
				}

				if (this.innerConnection != null)
				{
					return this.innerConnection.Database.ServerVersion;
				}

				return String.Empty;
			}
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="State"]/*'/>
#if	(!NETCF)
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
		public ConnectionState State
		{
			get { return this.state; }
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="PacketSize"]/*'/>
#if	(!NETCF)
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
		public int PacketSize
		{
			get { return this.options.PacketSize; }
		}

		#endregion

		#region Internal Properties

		internal FbConnectionInternal InnerConnection
		{
			get { return this.innerConnection; }
		}

		internal FbConnectionString ConnectionOptions
		{
			get { return this.options; }
		}

		internal bool IsClosed
		{
			get { return this.state == ConnectionState.Closed; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/constructor[@name="ctor"]/*'/>
		public FbConnection() : this(null)
		{
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/constructor[@name="ctor(System.String)"]/*'/>	
		public FbConnection(string connectionString) : base()
		{
			this.options = new FbConnectionString();
			this.state = ConnectionState.Closed;
			this.connectionString = "";

			if (connectionString != null)
			{
				this.ConnectionString = connectionString;
			}
		}

		#endregion

		#region IDisposable	Methods

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Dispose(System.Boolean)"]/*'/>
		protected override void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.disposed)
				{
					try
					{
						// release any unmanaged resources
						this.Close();

						if (disposing)
						{
							// release any managed resources
							this.innerConnection = null;
						}

						this.disposed = true;
					}
					catch
					{
					}
					finally
					{
						base.Dispose(disposing);
					}
				}
			}
		}

		#endregion

		#region ICloneable Methods

		object ICloneable.Clone()
		{
			return new FbConnection(this.ConnectionString);
		}

		#endregion

		#region Static Properties

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ConnectionPoolsCount"]/*'/>
		public static int ConnectionPoolsCount
		{
			get { return FbPoolManager.Instance.PoolsCount; }
		}

		#endregion

		#region Static Methods

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetPooledConnectionCount(FbConnection)"]/*'/>
		public static int GetPooledConnectionCount(FbConnection connection)
		{
			FbPoolManager manager = FbPoolManager.Instance;
			FbConnectionPool pool = manager.FindPool(connection.ConnectionString);

			if (pool != null)
			{
				return pool.Count;
			}

			return 0;
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="ClearAllPools"]/*'/>
		public static void ClearAllPools()
		{
			FbPoolManager manager = FbPoolManager.Instance;

			manager.ClearAllPools();
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="ClearPool(FbConnection)"]/*'/>
		public static void ClearPool(FbConnection connection)
		{
			FbPoolManager manager = FbPoolManager.Instance;

			manager.ClearPool(connection.ConnectionString);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateDatabase(System.String)"]/*'/>
		public static void CreateDatabase(string connectionString)
		{
			FbConnection.CreateDatabase(connectionString, 4096, true, false);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateDatabase(System.String, System.Boolean)"]/*'/>
		public static void CreateDatabase(string connectionString, bool overwrite)
		{
			FbConnection.CreateDatabase(connectionString, 4096, true, overwrite);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateDatabase(System.String,System.Int32,System.Boolean,System.Boolean)"]/*'/>
		public static void CreateDatabase(
			string connectionString, int pageSize, bool forcedWrites, bool overwrite)
		{
			FbConnectionString options = new FbConnectionString(connectionString);
			options.Validate();

			try
			{
				// DPB configuration
				DatabaseParameterBuffer dpb = new DatabaseParameterBuffer();

				// Dpb version
				dpb.Append(IscCodes.isc_dpb_version1);

				// Dummy packet	interval
				dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });

				// User	name
				dpb.Append(IscCodes.isc_dpb_user_name, options.UserID);

				// User	password
				dpb.Append(IscCodes.isc_dpb_password, options.Password);

				// Database	dialect
				dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { options.Dialect, 0, 0, 0 });

				// Database overwrite
				dpb.Append(IscCodes.isc_dpb_overwrite, (short)(overwrite ? 1 : 0));

				// Character set
				if (options.Charset.Length > 0)
				{
					int index = Charset.SupportedCharsets.IndexOf(options.Charset);

					if (index == -1)
					{
						throw new ArgumentException("Character set is not valid.");
					}
					else
					{
						dpb.Append(
							IscCodes.isc_dpb_set_db_charset,
							Charset.SupportedCharsets[index].Name);
					}
				}

				// Page	Size
				if (pageSize > 0)
				{
					dpb.Append(IscCodes.isc_dpb_page_size, pageSize);
				}

				// Forced writes
				dpb.Append(IscCodes.isc_dpb_force_write, (short)(forcedWrites ? 1 : 0));

				if (!overwrite)
				{
					// Check if	the	database exists
					FbConnectionInternal c = new FbConnectionInternal(options);

					try
					{
						c.Connect();
						c.Disconnect();

						IscException ex = new IscException(IscCodes.isc_db_or_file_exists);
						throw new FbException(ex.Message, ex);
					}
					catch (FbException ex)
					{
						if (ex.ErrorCode != 335544344)
						{
							throw;
						}
					}
				}

				// Create the new database
				FbConnectionInternal db = new FbConnectionInternal(options);
				db.CreateDatabase(dpb);
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="DropDatabase(System.String)"]/*'/>
		public static void DropDatabase(string connectionString)
		{
			// Configure Attachment
			FbConnectionString options = new FbConnectionString(connectionString);
			options.Validate();

			try
			{
				// Drop	the	database	
				FbConnectionInternal db = new FbConnectionInternal(options);
				db.DropDatabase();
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateDatabase(System.Collections.Hashtable)"]/*'/>
		[Obsolete("Use CreateDatabase(string connectionString) instead")]
		public static void CreateDatabase(Hashtable values)
		{
			bool overwrite = false;
			int index = 0;
			byte dialect = 3;
			int serverType = 0;

			if (!values.ContainsKey("User") ||
				!values.ContainsKey("Password") ||
				!values.ContainsKey("Database"))
			{
				throw new ArgumentException("CreateDatabase requires a user name, password and database path.");
			}

			if (values.ContainsKey("ServerType"))
			{
				serverType = Convert.ToInt32(values["ServerType"], CultureInfo.InvariantCulture);
			}

			if (!values.ContainsKey("DataSource"))
			{
				values.Add("DataSource", "localhost");
			}

			if (!values.ContainsKey("Port"))
			{
				values.Add("Port", 3050);
			}

			if (values.ContainsKey("Dialect"))
			{
				dialect = Convert.ToByte(values["Dialect"], CultureInfo.InvariantCulture);
			}

			if (dialect < 1 || dialect > 3)
			{
				throw new ArgumentException("Incorrect database dialect it should be 1, 2, or 3.");
			}

			if (values.ContainsKey("Overwrite"))
			{
				overwrite = (bool)values["Overwrite"];
			}

			try
			{
				// Configure Attachment
				FbConnectionStringBuilder csb = new FbConnectionStringBuilder();

				csb.DataSource	= values["DataSource"].ToString();
				csb.UserID		= values["User"].ToString();
				csb.Password	= values["Password"].ToString();
				csb.Database	= values["Database"].ToString();
				csb.Port		= Convert.ToInt32(values["Port"], CultureInfo.InvariantCulture);
				csb.ServerType	= serverType;

				FbConnectionString options = new FbConnectionString(csb);

				// DPB configuration
				DatabaseParameterBuffer dpb = new DatabaseParameterBuffer();

				// Dpb version
				dpb.Append(IscCodes.isc_dpb_version1);

				// Dummy packet	interval
				dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });

				// User	name
				dpb.Append(IscCodes.isc_dpb_user_name, values["User"].ToString());

				// User	password
				dpb.Append(IscCodes.isc_dpb_password, values["Password"].ToString());

				// Database	dialect
				dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { dialect, 0, 0, 0 });

				// Database overwrite
				dpb.Append(IscCodes.isc_dpb_overwrite, (short)(overwrite ? 1 : 0));

				// Character set
				if (values.ContainsKey("Charset"))
				{
					index = Charset.SupportedCharsets.IndexOf(values["Charset"].ToString());

					if (index == -1)
					{
						throw new ArgumentException("Character set is not valid.");
					}
					else
					{
						dpb.Append(
							IscCodes.isc_dpb_set_db_charset,
							Charset.SupportedCharsets[index].Name);
					}
				}

				// Page	Size
				if (values.ContainsKey("PageSize"))
				{
					dpb.Append(IscCodes.isc_dpb_page_size, Convert.ToInt32(values["PageSize"], CultureInfo.InvariantCulture));
				}

				// Forced writes
				if (values.ContainsKey("ForcedWrite"))
				{
					dpb.Append(IscCodes.isc_dpb_force_write,
						(short)((bool)values["ForcedWrite"] ? 1 : 0));
				}

				if (!overwrite)
				{
					try
					{
						// Check if	the	database exists
						FbConnectionInternal check = new FbConnectionInternal(options);

						check.Connect();
						check.Disconnect();

						IscException ex = new IscException(IscCodes.isc_db_or_file_exists);

						throw new FbException(ex.Message, ex);
					}
					catch (Exception)
					{
						throw;
					}
				}

				// Create the new database
				FbConnectionInternal c = new FbConnectionInternal(options);
				c.CreateDatabase(dpb);
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="DropDatabase(System.Collections.Hashtable)"]/*'/>
		[Obsolete("Use DropDatabase(string connectionString) instead")]
		public static void DropDatabase(Hashtable values)
		{
			int serverType = 0;

			if (!values.ContainsKey("User") ||
				!values.ContainsKey("Password") ||
				!values.ContainsKey("Database"))
			{
				throw new ArgumentException("CreateDatabase requires a user name, password and database path.");
			}

			if (!values.ContainsKey("DataSource"))
			{
				values.Add("DataSource", "localhost");
			}

			if (!values.ContainsKey("Port"))
			{
				values.Add("Port", 3050);
			}

			if (values.ContainsKey("ServerType"))
			{
				serverType = Convert.ToInt32(values["ServerType"], CultureInfo.InvariantCulture);
			}

			try
			{
				// Configure Attachment
				FbConnectionStringBuilder csb = new FbConnectionStringBuilder();

				csb.DataSource = values["DataSource"].ToString();
				csb.Port = Convert.ToInt32(values["Port"], CultureInfo.InvariantCulture);
				csb.Database = values["Database"].ToString();
				csb.UserID = values["User"].ToString();
				csb.Password = values["Password"].ToString();
				csb.ServerType = serverType;

				FbConnectionString options = new FbConnectionString(csb);

				// Drop	the	database
				FbConnectionInternal db = new FbConnectionInternal(options);
				db.DropDatabase();
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region Methods

		IDbTransaction IDbConnection.BeginTransaction()
		{
			return this.BeginTransaction();
		}

		IDbTransaction IDbConnection.BeginTransaction(IsolationLevel level)
		{
			return this.BeginTransaction(level);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction"]/*'/>
		public FbTransaction BeginTransaction()
		{
			return this.BeginTransaction(IsolationLevel.ReadCommitted, null);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(System.String)"]/*'/>
		public FbTransaction BeginTransaction(string transactionName)
		{
			return this.BeginTransaction(IsolationLevel.ReadCommitted, transactionName);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(System.Data.IsolationLevel)"]/*'/>
		public FbTransaction BeginTransaction(IsolationLevel level)
		{
			return this.BeginTransaction(level, null);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(System.Data.IsolationLevel,System.String)"]/*'/>
		public FbTransaction BeginTransaction(IsolationLevel level, string transactionName)
		{
			if (this.IsClosed)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
			}

			return this.innerConnection.BeginTransaction(level, transactionName);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(FbTransactionOptions)"]/*'/>
		public FbTransaction BeginTransaction(FbTransactionOptions options)
		{
			return this.BeginTransaction(options, null);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(FbTransactionOptions, System.String)"]/*'/>
		public FbTransaction BeginTransaction(FbTransactionOptions options, string transactionName)
		{
			if (this.IsClosed)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
			}

			return this.innerConnection.BeginTransaction(options, transactionName);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="ChangeDatabase(System.String)"]/*'/>
		public void ChangeDatabase(string db)
		{
			lock (this)
			{
				if (this.IsClosed)
				{
					throw new InvalidOperationException("ChangeDatabase requires an open and available Connection.");
				}

				if (db == null || db.Trim().Length == 0)
				{
					throw new InvalidOperationException("Database name is not valid.");
				}

				string cs = this.connectionString;

				try
				{
					FbConnectionStringBuilder csb = new FbConnectionStringBuilder(this.connectionString);

					// Close current connection
					this.Close();

					// Set up the new Database
					csb.Database = db;

					// Open	new	connection
					this.Open();
				}
				catch (IscException ex)
				{
					this.ConnectionString = cs;
					throw new FbException(ex.Message, ex);
				}
			}
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Open"]/*'/>
		public void Open()
		{
			lock (this)
			{
				if (this.connectionString == null || this.connectionString.Length == 0)
				{
					throw new InvalidOperationException("Connection String is not initialized.");
				}
				if (!this.IsClosed && this.state != ConnectionState.Connecting)
				{
					throw new InvalidOperationException("Connection already Open.");
				}

				try
				{
					this.OnStateChange(this.state, ConnectionState.Connecting);

					if (this.options.Pooling)
					{
						// Use Connection Pooling
						FbConnectionPool pool = FbPoolManager.Instance.CreatePool(this.connectionString);
						this.innerConnection = pool.CheckOut();
						this.innerConnection.OwningConnection = this;
					}
					else
					{
						// Do not use Connection Pooling
						this.innerConnection = new FbConnectionInternal(this.options, this);
						this.innerConnection.Connect();
					}

					// Bind	Warning	messages event
					this.innerConnection.Database.WarningMessage = new WarningMessageCallback(this.OnWarningMessage);

					// Update the connection state
					this.OnStateChange(this.state, ConnectionState.Open);
				}
				catch (IscException ex)
				{
					this.OnStateChange(this.state, ConnectionState.Closed);
					throw new FbException(ex.Message, ex);
				}
				catch (Exception)
				{
					this.OnStateChange(this.state, ConnectionState.Closed);
					throw;
				}
			}
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Close"]/*'/>
		public void Close()
		{
			if (this.IsClosed)
			{
				return;
			}

			lock (this)
			{
				try
				{
					lock (this.innerConnection)
					{
						// Close the Remote	Event Manager
						this.innerConnection.CloseEventManager();

						// Unbind Warning messages event
						this.innerConnection.Database.WarningMessage = null;

						// Dispose Transaction
						this.innerConnection.DisposeTransaction();

						// Dispose all active statemenets
						this.innerConnection.DisposePreparedCommands();

						// Close connection	or send	it back	to the pool
						if (this.innerConnection.Pooled)
						{
							// Get Connection Pool
							FbConnectionPool pool = FbPoolManager.Instance.FindPool(this.connectionString);

							// Send	connection to the Pool
							pool.CheckIn(this.innerConnection);
						}
						else
						{
							this.innerConnection.Disconnect();
						}
					}

					// Update connection state
					this.OnStateChange(this.state, ConnectionState.Closed);
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		IDbCommand IDbConnection.CreateCommand()
		{
			return this.CreateCommand();
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateCommand"]/*'/>
		public FbCommand CreateCommand()
		{
			FbCommand command = new FbCommand();

			lock (this)
			{
				command.Connection = this;
			}

			return command;
		}

		#endregion

		#region Database Schema

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetSchema"]/*'/>
		public DataTable GetSchema()
		{
			return this.GetSchema("MetaDataCollections");
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetSchema(System.String)"]/*'/>
		public DataTable GetSchema(string collectionName)
		{
			return this.GetSchema(collectionName, null);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetSchema(System.String, System.String[])"]/*'/>
		public DataTable GetSchema(string collectionName, string[] restrictions)
		{
			if (this.IsClosed)
			{
				throw new InvalidOperationException("The connection is closed.");
			}

			return this.innerConnection.GetSchema(collectionName, restrictions);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetDbSchemaTable"]/*'/>
		[Obsolete("Use GetSchema methods instead")]
		public DataTable GetDbSchemaTable(FbDbSchemaType schema, object[] restrictions)
		{
			if (this.state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("The conneciton is closed.");
			}

			return innerConnection.GetSchema(schema.ToString(), restrictions);
		}

		#endregion

		#region Private	Methods

		private void OnWarningMessage(IscException warning)
		{
			if (this.InfoMessage != null)
			{
				this.InfoMessage(this, new FbInfoMessageEventArgs(warning));
			}
		}

		private void OnStateChange(ConnectionState originalState, ConnectionState currentState)
		{
			this.state = currentState;
			if (this.StateChange != null)
			{
				this.StateChange(this, new StateChangeEventArgs(originalState, currentState));
			}
		}

		#endregion
	}
}
