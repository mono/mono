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
using System.Data;
using System.Collections;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbTransaction"]/overview/*'/>
	public sealed class FbTransaction : MarshalByRefObject, IDbTransaction, IDisposable
	{
		#region Fields

		private ITransaction	transaction;
		private FbConnection	connection;
		private IsolationLevel	isolationLevel;
		private bool			disposed;
		private bool			isUpdated;

		#endregion

		#region Properties

		IDbConnection IDbTransaction.Connection
		{
			get { return this.Connection; }
		}

		/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbTransaction"]/property[@name="Connection"]/*'/>
		public FbConnection Connection
		{
			get
			{
				if (!this.isUpdated)
				{
					return this.connection;
				}
				else
				{
					return null;
				}
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbTransaction"]/property[@name="IsolationLevel"]/*'/>
		public IsolationLevel IsolationLevel
		{
			get { return this.isolationLevel; }
		}

		#endregion

		#region Internal Properties

		internal ITransaction Transaction
		{
			get { return this.transaction; }
		}

		internal bool IsUpdated
		{
			get { return this.isUpdated; }
		}

		#endregion

		#region Constructors

		internal FbTransaction(FbConnection connection) : this(connection, IsolationLevel.ReadCommitted)
		{
		}

		internal FbTransaction(FbConnection connection, IsolationLevel il)
		{
			this.isolationLevel = il;
			this.connection		= connection;
		}

		#endregion

		#region Finalizer

		/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbCommandBuilder"]/destructor[@name="Finalize"]/*'/>
		~FbTransaction()
		{
			this.Dispose(false);
		}

		#endregion

		#region IDisposable	Methods

		/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbTransaction"]/method[@name="Dispose"]/*'/>
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
						if (this.transaction != null)
						{
							if ((this.transaction.State == TransactionState.TransactionStarted
								|| this.transaction.State == TransactionState.TransactionPrepared)
								&& !this.isUpdated)
							{
								this.transaction.Dispose();
								this.transaction = null;
							}
						}

						// release any managed resources
						if (disposing)
						{
							this.connection = null;
							this.transaction = null;
						}
					}
					finally
					{
						this.isUpdated = true;
						this.disposed = true;
					}
				}
			}
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbTransaction"]/method[@name="Commit"]/*'/>
		public void Commit()
		{
			lock (this)
			{
				if (this.isUpdated)
				{
					throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
				}

				try
				{
					this.transaction.Commit();
					this.UpdateTransaction();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbTransaction"]/method[@name="Rollback"]/*'/>
		public void Rollback()
		{
			lock (this)
			{
				if (this.isUpdated)
				{
					throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
				}

				try
				{
					this.transaction.Rollback();
					this.UpdateTransaction();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbTransaction"]/method[@name="Save(System.String)"]/*'/>
		public void Save(string savePointName)
		{
			lock (this)
			{
				if (savePointName == null)
				{
					throw new ArgumentException("No transaction name was be specified.");
				}
				else
				{
					if (savePointName.Length == 0)
					{
						throw new ArgumentException("No transaction name was be specified.");
					}
				}
				if (this.isUpdated)
				{
					throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
				}

				try
				{
					FbCommand command = new FbCommand(
						"SAVEPOINT " + savePointName,
						this.connection,
						this);
					command.ExecuteNonQuery();
					command.Dispose();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbTransaction"]/method[@name="Commit(System.String)"]/*'/>
		public void Commit(string savePointName)
		{
			lock (this)
			{
				if (savePointName == null)
				{
					throw new ArgumentException("No transaction name was be specified.");
				}
				else
				{
					if (savePointName.Length == 0)
					{
						throw new ArgumentException("No transaction name was be specified.");
					}
				}
				if (this.isUpdated)
				{
					throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
				}

				try
				{
					FbCommand command = new FbCommand(
						"RELEASE SAVEPOINT " + savePointName,
						this.connection,
						this);
					command.ExecuteNonQuery();
					command.Dispose();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbTransaction"]/method[@name="Rollback(System.String)"]/*'/>
		public void Rollback(string savePointName)
		{
			lock (this)
			{
				if (savePointName == null)
				{
					throw new ArgumentException("No transaction name was be specified.");
				}
				else
				{
					if (savePointName.Length == 0)
					{
						throw new ArgumentException("No transaction name was be specified.");
					}
				}
				if (this.isUpdated)
				{
					throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
				}

				try
				{
					FbCommand command = new FbCommand(
						"ROLLBACK WORK TO SAVEPOINT " + savePointName,
						this.connection,
						this);
					command.ExecuteNonQuery();
					command.Dispose();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbTransaction"]/method[@name="CommitRetaining"]/*'/>
		public void CommitRetaining()
		{
			lock (this)
			{
				if (this.isUpdated)
				{
					throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
				}

				try
				{
					this.transaction.CommitRetaining();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml'	path='doc/class[@name="FbTransaction"]/method[@name="RollbackRetaining"]/*'/>
		public void RollbackRetaining()
		{
			lock (this)
			{
				if (this.isUpdated)
				{
					throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
				}

				try
				{
					this.transaction.RollbackRetaining();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		#endregion

		#region InternalMethods

		internal void BeginTransaction()
		{
			lock (this)
			{
				try
				{
					IDatabase database = this.connection.InnerConnection.Database;
					this.transaction = database.BeginTransaction(this.BuildTpb());
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		internal void BeginTransaction(FbTransactionOptions options)
		{
			lock (this)
			{
				try
				{
					IDatabase database = this.connection.InnerConnection.Database;
					this.transaction = database.BeginTransaction(this.BuildTpb(options));
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		#endregion

		#region Private	Methods

		private void UpdateTransaction()
		{
			if (this.connection != null)
			{
				this.connection.InnerConnection.TransactionUpdated();
			}

			this.isUpdated	= true;
			this.connection = null;
			this.transaction = null;
		}

		private TransactionParameterBuffer BuildTpb()
		{
			FbTransactionOptions options = FbTransactionOptions.Write;

			options |= FbTransactionOptions.Wait;

			/* Isolation level */
			switch (this.isolationLevel)
			{
				case IsolationLevel.Serializable:
					options |= FbTransactionOptions.Consistency;
					break;

				case IsolationLevel.RepeatableRead:
					options |= FbTransactionOptions.Concurrency;
					break;

				case IsolationLevel.ReadUncommitted:
					options |= FbTransactionOptions.ReadCommitted;
					options |= FbTransactionOptions.RecVersion;
					break;

				case IsolationLevel.ReadCommitted:
				default:
					options |= FbTransactionOptions.ReadCommitted;
					options |= FbTransactionOptions.NoRecVersion;
					break;
			}

			return this.BuildTpb(options);
		}

#if	(!NETCF)

		private TransactionParameterBuffer BuildTpb(FbTransactionOptions options)
		{
			TransactionParameterBuffer tpb = new TransactionParameterBuffer();

			tpb.Append(IscCodes.isc_tpb_version3);

			FbTransactionOptions[] o = (FbTransactionOptions[])Enum.GetValues(options.GetType());
			for (int i = 0; i < o.Length; i++)
			{
				FbTransactionOptions option = ((FbTransactionOptions)(o[i]));
				if ((options & option) == option)
				{
					switch (option)
					{
						case FbTransactionOptions.Consistency:
							tpb.Append(IscCodes.isc_tpb_consistency);
							break;

						case FbTransactionOptions.Concurrency:
							tpb.Append(IscCodes.isc_tpb_concurrency);
							break;

						case FbTransactionOptions.Shared:
							tpb.Append(IscCodes.isc_tpb_shared);
							break;

						case FbTransactionOptions.Protected:
							tpb.Append(IscCodes.isc_tpb_protected);
							break;

						case FbTransactionOptions.Exclusive:
							tpb.Append(IscCodes.isc_tpb_exclusive);
							break;

						case FbTransactionOptions.Wait:
							tpb.Append(IscCodes.isc_tpb_wait);
							break;

						case FbTransactionOptions.NoWait:
							tpb.Append(IscCodes.isc_tpb_nowait);
							break;

						case FbTransactionOptions.Read:
							tpb.Append(IscCodes.isc_tpb_read);
							break;

						case FbTransactionOptions.Write:
							tpb.Append(IscCodes.isc_tpb_write);
							break;

						case FbTransactionOptions.LockRead:
							tpb.Append(IscCodes.isc_tpb_lock_read);
							break;

						case FbTransactionOptions.LockWrite:
							tpb.Append(IscCodes.isc_tpb_lock_write);
							break;

						case FbTransactionOptions.ReadCommitted:
							tpb.Append(IscCodes.isc_tpb_read_committed);
							break;

						case FbTransactionOptions.Autocommit:
							tpb.Append(IscCodes.isc_tpb_autocommit);
							break;

						case FbTransactionOptions.RecVersion:
							tpb.Append(IscCodes.isc_tpb_rec_version);
							break;

						case FbTransactionOptions.NoRecVersion:
							tpb.Append(IscCodes.isc_tpb_no_rec_version);
							break;

						case FbTransactionOptions.RestartRequests:
							tpb.Append(IscCodes.isc_tpb_restart_requests);
							break;

						case FbTransactionOptions.NoAutoUndo:
							tpb.Append(IscCodes.isc_tpb_no_auto_undo);
							break;
					}
				}
			}

			return tpb;
		}

#else

		private	TransactionParameterBuffer BuildTpb(FbTransactionOptions options)
		{
			TransactionParameterBuffer tpb = new TransactionParameterBuffer();

			tpb.Append(IscCodes.isc_tpb_version3);

			if ((options & FbTransactionOptions.Consistency) ==	FbTransactionOptions.Consistency)
			{
				tpb.Append(IscCodes.isc_tpb_consistency);
			}
			if ((options & FbTransactionOptions.Concurrency) ==	FbTransactionOptions.Concurrency)
			{
				tpb.Append(IscCodes.isc_tpb_concurrency);
			}
			if ((options & FbTransactionOptions.Shared) == FbTransactionOptions.Shared)
			{
				tpb.Append(IscCodes.isc_tpb_shared);
			}
			if ((options & FbTransactionOptions.Protected) == FbTransactionOptions.Protected)
			{
				tpb.Append(IscCodes.isc_tpb_protected);
			}
			if ((options & FbTransactionOptions.Exclusive) == FbTransactionOptions.Exclusive)
			{
				tpb.Append(IscCodes.isc_tpb_exclusive);
			}
			if ((options & FbTransactionOptions.Wait) == FbTransactionOptions.Wait)
			{
				tpb.Append(IscCodes.isc_tpb_wait);
			}
			if ((options & FbTransactionOptions.NoWait) == FbTransactionOptions.NoWait)
			{
				tpb.Append(IscCodes.isc_tpb_nowait);
			}
			if ((options & FbTransactionOptions.Read) == FbTransactionOptions.Read)
			{
				tpb.Append(IscCodes.isc_tpb_read);
			}
			if ((options & FbTransactionOptions.Write) == FbTransactionOptions.Write)
			{
				tpb.Append(IscCodes.isc_tpb_write);
			}
			if ((options & FbTransactionOptions.LockRead) == FbTransactionOptions.LockRead)
			{
				tpb.Append(IscCodes.isc_tpb_lock_read);
			}
			if ((options & FbTransactionOptions.LockWrite) == FbTransactionOptions.LockWrite)
			{
				tpb.Append(IscCodes.isc_tpb_lock_write);
			}
			if ((options & FbTransactionOptions.ReadCommitted) == FbTransactionOptions.ReadCommitted)
			{
				tpb.Append(IscCodes.isc_tpb_read_committed);
			}
			if ((options & FbTransactionOptions.Autocommit) == FbTransactionOptions.Autocommit)
			{
				tpb.Append(IscCodes.isc_tpb_autocommit);
			}
			if ((options & FbTransactionOptions.RecVersion) == FbTransactionOptions.RecVersion)
			{
				tpb.Append(IscCodes.isc_tpb_rec_version);
			}
			if ((options & FbTransactionOptions.NoRecVersion) == FbTransactionOptions.NoRecVersion)
			{
				tpb.Append(IscCodes.isc_tpb_no_rec_version);
			}
			if ((options & FbTransactionOptions.RestartRequests) ==	FbTransactionOptions.RestartRequests)
			{
				tpb.Append(IscCodes.isc_tpb_restart_requests);
			}
			if ((options & FbTransactionOptions.NoAutoUndo) == FbTransactionOptions.NoAutoUndo)
			{
				tpb.Append(IscCodes.isc_tpb_no_auto_undo);
			}
			
			return tpb;
		}

#endif

		#endregion
	}
}
