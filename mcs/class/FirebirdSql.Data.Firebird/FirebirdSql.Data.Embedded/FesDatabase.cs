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
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.Runtime.InteropServices;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Embedded
{
	internal sealed class FesDatabase : IDatabase
	{
		#region Callbacks

		public WarningMessageCallback WarningMessage
		{
			get { return this.warningMessage; }
			set { this.warningMessage = value; }
		}

		#endregion

		#region Fields

		private WarningMessageCallback warningMessage;

		private int		handle;
		private int		transactionCount;
		private string	serverVersion;
		private Charset charset;
		private short	packetSize;
		private short	dialect;
		private bool	disposed;

		#endregion

		#region Properties

		public int Handle
		{
			get { return this.handle; }
		}

		public int TransactionCount
		{
			get { return this.transactionCount; }
			set { this.transactionCount = value; }
		}

		public string ServerVersion
		{
			get { return this.serverVersion; }
		}

		public Charset Charset
		{
			get { return this.charset; }
			set { this.charset = value; }
		}

		public short PacketSize
		{
			get { return this.packetSize; }
			set { this.packetSize = value; }
		}

		public short Dialect
		{
			get { return this.dialect; }
			set { this.dialect = value; }
		}

		public bool HasRemoteEventSupport
		{
			get { return false; }
		}

		#endregion

		#region Constructors

		public FesDatabase()
		{
			this.charset	= Charset.DefaultCharset;
			this.dialect	= 3;
			this.packetSize = 8192;

			GC.SuppressFinalize(this);
		}

		#endregion

		#region Finalizer

		~FesDatabase()
		{
			this.Dispose(false);
		}

		#endregion

		#region IDisposable	methods

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.disposed)
				{
					try
					{
						// release any unmanaged resources
						this.Detach();

						// release any managed resources
						if (disposing)
						{
							this.warningMessage = null;
							this.charset		= null;
							this.serverVersion	= null;
							this.transactionCount = 0;
							this.dialect		= 0;
							this.handle			= 0;
							this.packetSize		= 0;
						}
					}
					finally
					{
						this.disposed = true;
					}
				}
			}
		}

		#endregion

		#region Database Methods

		public void CreateDatabase(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			lock (this)
			{
				int[] statusVector = FesConnection.GetNewStatusVector();
				int dbHandle = this.Handle;

				FbClient.isc_create_database(
					statusVector,
					(short)database.Length,
					database,
					ref	dbHandle,
					(short)dpb.Length,
					dpb.ToArray(),
					0);

				this.ParseStatusVector(statusVector);

				this.handle = dbHandle;

				this.Detach();
			}
		}

		public void DropDatabase()
		{
			lock (this)
			{
				int[] statusVector = FesConnection.GetNewStatusVector();
				int	dbHandle = this.Handle;

				FbClient.isc_drop_database(statusVector, ref dbHandle);

				this.ParseStatusVector(statusVector);

				this.handle = 0;
			}
		}

		#endregion

		#region Remote Events Methods

		void IDatabase.CloseEventManager()
		{
			throw new NotSupportedException();
		}

		RemoteEvent IDatabase.CreateEvent()
		{
			throw new NotSupportedException();
		}

		void IDatabase.QueueEvents(RemoteEvent events)
		{
			throw new NotSupportedException();
		}

		void IDatabase.CancelEvents(RemoteEvent events)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Methods

		public void Attach(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			lock (this)
			{
				int[] statusVector = FesConnection.GetNewStatusVector();
				int dbHandle = 0;

				FbClient.isc_attach_database(
					statusVector,
					(short)database.Length,
					database,
					ref	dbHandle,
					(short)dpb.Length,
					dpb.ToArray());

				this.handle = dbHandle;

				this.ParseStatusVector(statusVector);

				// Get server version
				this.serverVersion = this.GetServerVersion();
			}
		}

		public void Detach()
		{
			lock (this)
			{
				if (this.TransactionCount > 0)
				{
					throw new IscException(IscCodes.isc_open_trans, this.TransactionCount);
				}

				int[] statusVector = FesConnection.GetNewStatusVector();
				int dbHandle = this.Handle;

				FbClient.isc_detach_database(statusVector, ref dbHandle);

				this.handle = dbHandle;

				FesConnection.ParseStatusVector(statusVector);
			}
		}

		#endregion

		#region Transaction	methods

		public ITransaction BeginTransaction(TransactionParameterBuffer tpb)
		{
			FesTransaction transaction = new FesTransaction(this);
			transaction.BeginTransaction(tpb);

			return transaction;
		}

		#endregion

		#region Statement creation methods

		public StatementBase CreateStatement()
		{
			return new FesStatement(this);
		}

		public StatementBase CreateStatement(ITransaction transaction)
		{
			return new FesStatement(this, transaction as FesTransaction);
		}

		#endregion

		#region Parameter Buffer Creation methods

		public BlobParameterBuffer CreateBlobParameterBuffer()
		{
			return new BlobParameterBuffer(BitConverter.IsLittleEndian);
		}

		public DatabaseParameterBuffer CreateDatabaseParameterBuffer()
		{
			return new DatabaseParameterBuffer(BitConverter.IsLittleEndian);
		}

		public EventParameterBuffer CreateEventParameterBuffer()
		{
			return new EventParameterBuffer();
		}

		public TransactionParameterBuffer CreateTransactionParameterBuffer()
		{
			return new TransactionParameterBuffer(BitConverter.IsLittleEndian);
		}

		#endregion

		#region Database information methods

		public string GetServerVersion()
		{
			byte[] items = new byte[]
			{
				IscCodes.isc_info_isc_version,
				IscCodes.isc_info_end
			};

			return this.GetDatabaseInfo(items, 50)[0].ToString();
		}

		public ArrayList GetDatabaseInfo(byte[] items)
		{
			return this.GetDatabaseInfo(items, IscCodes.MAX_BUFFER_SIZE);
		}

		public ArrayList GetDatabaseInfo(byte[] items, int bufferLength)
		{
			byte[] buffer = new byte[bufferLength];

			this.DatabaseInfo(items, buffer, buffer.Length);

			return IscHelper.ParseDatabaseInfo(buffer);
		}

		#endregion

		#region Internal methods

		internal void ParseStatusVector(int[] statusVector)
		{
			IscException ex = FesConnection.ParseStatusVector(statusVector);

			if (ex != null)
			{
				if (ex.IsWarning)
				{
					this.warningMessage(ex);
				}
				else
				{
					throw ex;
				}
			}
		}

		#endregion

		#region Private	Methods

		private void DatabaseInfo(byte[] items, byte[] buffer, int bufferLength)
		{
			lock (this)
			{
				int[] statusVector = FesConnection.GetNewStatusVector();
				int dbHandle = this.Handle;

				FbClient.isc_database_info(
					statusVector,
					ref	dbHandle,
					(short)items.Length,
					items,
					(short)bufferLength,
					buffer);

				this.ParseStatusVector(statusVector);
			}
		}

		#endregion
	}
}