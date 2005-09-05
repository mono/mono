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
using System.Data;
using System.Text;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Firebird.DbSchema;

namespace FirebirdSql.Data.Firebird
{
	internal class FbConnectionInternal : MarshalByRefObject
	{
		#region Fields

		private IDatabase			db;
		private FbTransaction		activeTransaction;
		private ArrayList			preparedCommands;
		private FbConnectionString	options;
		private FbConnection		owningConnection;
		private long				created;
		private long				lifetime;
		private bool				pooled;

		#endregion

		#region Properties

		public IDatabase Database
		{
			get { return this.db; }
		}

		public long Lifetime
		{
			get { return this.lifetime; }
			set { this.lifetime = value; }
		}

		public long Created
		{
			get { return this.created; }
			set { this.created = value; }
		}

		public bool Pooled
		{
			get { return this.pooled; }
			set { this.pooled = value; }
		}

		public bool HasActiveTransaction
		{
			get
			{
				return this.activeTransaction != null && !this.activeTransaction.IsUpdated;
			}
		}

		public ArrayList PreparedCommands
		{
			get
			{
				if (this.preparedCommands == null)
				{
					this.preparedCommands = new ArrayList();
				}

				return this.preparedCommands;
			}
		}

		public FbTransaction ActiveTransaction
		{
			get { return this.activeTransaction; }
		}

		public FbConnectionString ConnectionOptions
		{
			get { return this.options; }
		}

		public FbConnection OwningConnection
		{
			get { return this.owningConnection; }
			set { this.owningConnection = value; }
		}

		#endregion

		#region Constructors

		public FbConnectionInternal(FbConnectionString options) : this(options, null)
		{
		}

		public FbConnectionInternal(FbConnectionString options, FbConnection owningConnection)
		{
			this.options = options;
			this.owningConnection = owningConnection;
		}

		#endregion

		#region Create and Drop	database methods

		public void CreateDatabase(DatabaseParameterBuffer dpb)
		{
			IDatabase db = ClientFactory.CreateDatabase(this.options.ServerType);
			db.CreateDatabase(dpb, this.options.DataSource, this.options.Port, this.options.Database);
		}

		public void DropDatabase()
		{
			IDatabase db = ClientFactory.CreateDatabase(this.options.ServerType);
			db.Attach(this.BuildDpb(db, this.options), this.options.DataSource, this.options.Port, this.options.Database);
			db.DropDatabase();
		}

		#endregion

		#region Connect	and	Disconenct methods

		public void Connect()
		{
			try
			{
				this.db = ClientFactory.CreateDatabase(this.options.ServerType);
				this.db.Charset = Charset.SupportedCharsets[this.options.Charset];
				this.db.Dialect = this.options.Dialect;
				this.db.PacketSize = this.options.PacketSize;

				DatabaseParameterBuffer dpb = this.BuildDpb(this.db, options);

				this.db.Attach(dpb, this.options.DataSource, this.options.Port, this.options.Database);
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void Disconnect()
		{
			try
			{
				this.db.Dispose();

				this.owningConnection	= null;
				this.options			= null;
				this.lifetime			= 0;
				this.pooled				= false;
				this.db					= null;

				this.DisposePreparedCommands();
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region Transaction	Methods

		public FbTransaction BeginTransaction(IsolationLevel level, string transactionName)
		{
			lock (this)
			{
				if (this.HasActiveTransaction)
				{
					throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
				}

				try
				{
					this.activeTransaction = new FbTransaction(this.owningConnection, level);
					this.activeTransaction.BeginTransaction();

					if (transactionName != null)
					{
						this.activeTransaction.Save(transactionName);
					}
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}

			return this.activeTransaction;
		}

		public FbTransaction BeginTransaction(FbTransactionOptions options, string transactionName)
		{
			lock (this)
			{
				if (this.HasActiveTransaction)
				{
					throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
				}

				try
				{
					this.activeTransaction = new FbTransaction(
						this.owningConnection, IsolationLevel.Unspecified);

					this.activeTransaction.BeginTransaction(options);

					if (transactionName != null)
					{
						this.activeTransaction.Save(transactionName);
					}
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}

			return this.activeTransaction;
		}

		public void DisposeTransaction()
		{
			if (this.activeTransaction != null)
			{
				this.activeTransaction.Dispose();
				this.activeTransaction = null;
			}
		}

		public void TransactionUpdated()
		{
			for (int i = 0; i < this.PreparedCommands.Count; i++)
			{
				FbCommand command = (FbCommand)this.PreparedCommands[i];

				if (command.Transaction != null)
				{
					command.CloseReader();
					command.Transaction = null;
				}
			}
		}

		#endregion

		#region Schema Methods

		public DataTable GetSchema(string collectionName, string[] restrictions)
		{
			return FbDbSchemaFactory.GetSchema(this.owningConnection, collectionName, restrictions);
		}

		[Obsolete]
		public DataTable GetSchema(string collectionName, object[] restrictions)
		{
			return FbDbSchemaFactory.GetSchema(this.owningConnection, collectionName, restrictions);
		}

		#endregion

		#region Prepared Commands Methods

		public void AddPreparedCommand(FbCommand command)
		{
			if (!this.PreparedCommands.Contains(command))
			{
				this.PreparedCommands.Add(command);
			}
		}

		public void RemovePreparedCommand(FbCommand command)
		{
			this.PreparedCommands.Remove(command);
		}

		public void DisposePreparedCommands()
		{
			if (this.preparedCommands != null)
			{
				if (this.PreparedCommands.Count > 0)
				{
					FbCommand[] commands = (FbCommand[])this.PreparedCommands.ToArray(typeof(FbCommand));

					for (int i = 0; i < commands.Length; i++)
					{
						// Release statement handle
						commands[i].Release();
					}
				}

				this.PreparedCommands.Clear();
				this.preparedCommands = null;
			}
		}

		#endregion

		#region Firebird Events	Methods

		public void CloseEventManager()
		{
			if (this.db.HasRemoteEventSupport)
			{
				lock (this.db)
				{
					this.db.CloseEventManager();
				}
			}
		}

		#endregion

		#region Connection Verification

		public bool Verify()
		{
			// Do not actually ask for any information
			byte[] items = new byte[]
			{
				IscCodes.isc_info_end
			};

			try
			{
				this.db.GetDatabaseInfo(items, 16);

				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		#region Private	Methods

		private DatabaseParameterBuffer BuildDpb(IDatabase db, FbConnectionString options)
		{
			DatabaseParameterBuffer dpb = db.CreateDatabaseParameterBuffer();

			dpb.Append(IscCodes.isc_dpb_version1);
			dpb.Append(IscCodes.isc_dpb_dummy_packet_interval,
				new byte[] { 120, 10, 0, 0 });
			dpb.Append(IscCodes.isc_dpb_sql_dialect,
				new byte[] { Convert.ToByte(options.Dialect), 0, 0, 0 });
			dpb.Append(IscCodes.isc_dpb_lc_ctype, options.Charset);
			if (options.Role != null && options.Role.Length > 0)
			{
				dpb.Append(IscCodes.isc_dpb_sql_role_name, options.Role);
			}
			dpb.Append(IscCodes.isc_dpb_connect_timeout, options.ConnectionTimeout);
			dpb.Append(IscCodes.isc_dpb_user_name, options.UserID);
			dpb.Append(IscCodes.isc_dpb_password, options.Password);

			return dpb;
		}

		#endregion
	}
}
