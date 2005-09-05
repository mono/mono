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
using System.Text;
using System.IO;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
	internal class GdsStatement : StatementBase
	{
		#region Fields

		private int				handle;
		private GdsDatabase		db;
		private GdsTransaction	transaction;
		private Descriptor		parameters;
		private Descriptor		fields;
		private StatementState	state;
		private DbStatementType statementType;
		private bool			allRowsFetched;
		private Queue			rows;
		private Queue			outputParams;
		private int				recordsAffected;
		private int				fetchSize;

		#endregion

		#region Properties

		public override IDatabase DB
		{
			get { return this.db; }
			set { this.db = (GdsDatabase)value; }
		}

		public override ITransaction Transaction
		{
			get { return this.transaction; }
			set
			{
				if (this.transaction != value)
				{
					if (this.TransactionUpdate != null && this.transaction != null)
					{
						this.transaction.Update -= this.TransactionUpdate;
						this.TransactionUpdate	= null;
					}

					if (value == null)
					{
						this.transaction = null;
					}
					else
					{
						this.transaction		= (GdsTransaction)value;
						this.TransactionUpdate	= new TransactionUpdateEventHandler(this.TransactionUpdated);
						this.transaction.Update += this.TransactionUpdate;
					}
				}
			}
		}

		public override Descriptor Parameters
		{
			get { return this.parameters; }
			set { this.parameters = value; }
		}

		public override Descriptor Fields
		{
			get { return this.fields; }
		}

		public override int RecordsAffected
		{
			get { return this.recordsAffected; }
		}

		public override bool IsPrepared
		{
			get
			{
				if (this.state == StatementState.Deallocated ||
					this.state == StatementState.Error)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public override DbStatementType StatementType
		{
			get { return this.statementType; }
			set { this.statementType = value; }
		}

		public override StatementState State
		{
			get { return this.state; }
			set { this.state = value; }
		}

		public override int FetchSize
		{
			get { return this.fetchSize; }
			set { this.fetchSize = value; }
		}

		#endregion

		#region Constructors

		public GdsStatement(IDatabase db)
			: this(db, null)
		{
		}

		public GdsStatement(IDatabase db, ITransaction transaction)
		{
			if (!(db is GdsDatabase))
			{
				throw new ArgumentException("Specified argument is not of GdsDatabase type.");
			}
			if (transaction != null && !(transaction is GdsTransaction))
			{
				throw new ArgumentException("Specified argument is not of GdsTransaction type.");
			}

			this.recordsAffected = -1;
			this.fetchSize		= 200;
			this.rows			= new Queue();
			this.outputParams	= new Queue();

			this.db = (GdsDatabase)db;
			if (transaction != null)
			{
				this.Transaction = transaction;
			}

			GC.SuppressFinalize(this);
		}

		#endregion

		#region IDisposable	Methods

		protected override void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.IsDisposed)
				{
					try
					{
						// release any unmanaged resources
						this.Release();

						// release any managed resources
						if (disposing)
						{
							this.Clear();

							this.rows			= null;
							this.outputParams	= null;
							this.db				= null;
							this.fields			= null;
							this.parameters		= null;
							this.transaction	= null;
							this.allRowsFetched = false;
							this.state			= StatementState.Deallocated;
							this.handle			= 0;
							this.fetchSize		= 0;
							this.recordsAffected = 0;
						}
					}
					finally
					{
						base.Dispose(disposing);
					}
				}
			}
		}

		#endregion

		#region Blob Creation Metods

		public override BlobBase CreateBlob()
		{
			return new GdsBlob(this.db, this.transaction);
		}

		public override BlobBase CreateBlob(long blobId)
		{
			return new GdsBlob(this.db, this.transaction, blobId);
		}

		#endregion

		#region Array Creation Methods

		public override ArrayBase CreateArray(ArrayDesc descriptor)
		{
			return new GdsArray(descriptor);
		}

		public override ArrayBase CreateArray(string tableName, string fieldName)
		{
			return new GdsArray(this.db, this.transaction, tableName, fieldName);
		}

		public override ArrayBase CreateArray(long handle, string tableName, string fieldName)
		{
			return new GdsArray(this.db, this.transaction, handle, tableName, fieldName);
		}

		#endregion

		#region Methods

		public override void Prepare(string commandText)
		{
			// Clear data
			this.Clear();
			this.parameters = null;
			this.fields = null;

			lock (this.db)
			{
				if (this.state == StatementState.Deallocated)
				{
					// Allocate	statement
					this.Allocate();
				}

				try
				{
					this.db.Send.Write(IscCodes.op_prepare_statement);
					this.db.Send.Write(this.transaction.Handle);
					this.db.Send.Write(this.handle);
					this.db.Send.Write((int)this.db.Dialect);
					this.db.Send.Write(commandText);
					this.db.Send.WriteBuffer(DescribeInfoItems, DescribeInfoItems.Length);
					this.db.Send.Write(IscCodes.MAX_BUFFER_SIZE);
					this.db.Send.Flush();

					GdsResponse r = this.db.ReadGenericResponse();
					this.fields = this.ParseSqlInfo(r.Data, DescribeInfoItems);

					// Determine the statement type
					this.statementType = this.GetStatementType();

					this.state = StatementState.Prepared;
				}
				catch (IOException)
				{
					this.state = StatementState.Error;
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public override void Execute()
		{
			if (this.state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statment is not correctly created.");
			}

			// Clear data
			this.Clear();

			lock (this.db)
			{
				try
				{
					byte[] descriptor = null;
					if (this.parameters != null)
					{
						XdrStream xdr = new XdrStream(this.db.Charset);
						xdr.Write(this.parameters);

						descriptor = xdr.ToArray();

						xdr.Close();
					}

					if (this.statementType == DbStatementType.StoredProcedure)
					{
						this.db.Send.Write(IscCodes.op_execute2);
					}
					else
					{
						this.db.Send.Write(IscCodes.op_execute);
					}

					this.db.Send.Write(this.handle);
					this.db.Send.Write(this.transaction.Handle);

					if (this.parameters != null)
					{
						this.db.Send.WriteBuffer(this.parameters.ToBlrArray());
						this.db.Send.Write(0);	// Message number
						this.db.Send.Write(1);	// Number of messages
						this.db.Send.Write(descriptor, 0, descriptor.Length);
					}
					else
					{
						this.db.Send.WriteBuffer(null);
						this.db.Send.Write(0);
						this.db.Send.Write(0);
					}

					if (this.statementType == DbStatementType.StoredProcedure)
					{
						this.db.Send.WriteBuffer(
							(this.fields == null) ? null : this.fields.ToBlrArray());
						this.db.Send.Write(0);	// Output message number
					}

					this.db.Send.Flush();

					if (this.db.NextOperation() == IscCodes.op_sql_response)
					{
						// This	would be an	Execute	procedure
						this.outputParams.Enqueue(this.ReceiveSqlResponse());
					}

					this.db.ReadGenericResponse();

					// Updated number of records affected by the statement execution			
					if (this.StatementType == DbStatementType.Insert ||
						this.StatementType == DbStatementType.Delete ||
						this.StatementType == DbStatementType.Update ||
                        this.StatementType == DbStatementType.StoredProcedure)
					{
						this.recordsAffected = this.GetRecordsAffected();
					}
					else
					{
						this.recordsAffected = -1;
					}

					this.state = StatementState.Executed;
				}
				catch (IOException)
				{
					this.state = StatementState.Error;
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public override DbValue[] Fetch()
		{
			if (this.state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statement is not correctly created.");
			}
			if (this.statementType != DbStatementType.Select &&
				this.statementType != DbStatementType.SelectForUpdate)
			{
				return null;
			}

			if (!this.allRowsFetched && this.rows.Count == 0)
			{
				// Fetch next batch	of rows
				lock (this.db)
				{
					try
					{
						this.db.Send.Write(IscCodes.op_fetch);
						this.db.Send.Write(this.handle);
						this.db.Send.WriteBuffer(this.fields.ToBlrArray());
						this.db.Send.Write(0);			// p_sqldata_message_number						
						this.db.Send.Write(fetchSize);	// p_sqldata_messages
						this.db.Send.Flush();

						if (this.db.NextOperation() == IscCodes.op_fetch_response)
						{
							int status	= 0;
							int count	= 1;
							int op		= 0;

							while (count > 0 && status == 0)
							{
								op = this.db.ReadOperation();
								status = this.db.Receive.ReadInt32();
								count = this.db.Receive.ReadInt32();

								if (count > 0 && status == 0)
								{
									this.rows.Enqueue(this.ReadDataRow());
								}
							}

							if (status == 100)
							{
								this.allRowsFetched = true;
							}
						}
						else
						{
							this.db.ReadGenericResponse();
						}
					}
					catch (IOException)
					{
						throw new IscException(IscCodes.isc_net_read_err);
					}
				}
			}

			if (this.rows != null && this.rows.Count > 0)
			{
				// return current row
				return (DbValue[])this.rows.Dequeue();
			}
			else
			{
				// All readed clear	rows and return	null
				this.rows.Clear();

				return null;
			}
		}

		public override DbValue[] GetOuputParameters()
		{
			if (this.outputParams.Count > 0)
			{
				return (DbValue[])this.outputParams.Dequeue();
			}

			return null;
		}

		public override void Describe()
		{
			try
			{
				byte[] buffer = this.GetSqlInfo(DescribeInfoItems);
				this.fields = this.ParseSqlInfo(buffer, DescribeInfoItems);
			}
			catch (IscException)
			{
				throw;
			}
		}

		public override void DescribeParameters()
		{
			try
			{
				byte[] buffer = this.GetSqlInfo(DescribeBindInfoItems);
				this.parameters = this.ParseSqlInfo(buffer, DescribeBindInfoItems);
			}
			catch (IscException)
			{
				throw;
			}
		}

		public override byte[] GetSqlInfo(byte[] items, int bufferLength)
		{
			lock (this.db)
			{
				try
				{
					this.db.Send.Write(IscCodes.op_info_sql);
					this.db.Send.Write(this.handle);
					this.db.Send.Write(0);
					this.db.Send.WriteBuffer(items, items.Length);
					this.db.Send.Write(bufferLength);
					this.db.Send.Flush();

					return this.db.ReadGenericResponse().Data;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region Protected Methods

		protected override void Free(int option)
		{
			// Does	not	seem to	be possible	or necessary to	close
			// an execute procedure	statement.
			if (this.StatementType == DbStatementType.StoredProcedure &&
				option == IscCodes.DSQL_close)
			{
				return;
			}

			lock (this.db)
			{
				try
				{
					this.db.Send.Write(IscCodes.op_free_statement);
					this.db.Send.Write(this.handle);
					this.db.Send.Write(option);
					this.db.Send.Flush();

					// Reset statement information
					if (option == IscCodes.DSQL_drop)
					{
						this.parameters = null;
						this.fields = null;
					}

					this.Clear();
					this.allRowsFetched = false;

					this.db.ReadGenericResponse();
				}
				catch (IOException)
				{
					this.state = StatementState.Error;
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		protected override void TransactionUpdated(object sender, EventArgs e)
		{
			lock (this)
			{
				if (this.Transaction != null && this.TransactionUpdate != null)
				{
					this.Transaction.Update -= this.TransactionUpdate;
				}

				this.State = StatementState.Closed;
				this.TransactionUpdate = null;
				this.allRowsFetched = false;
			}
		}

		#endregion

		#region Response Methods

		private DbValue[] ReceiveSqlResponse()
		{
			try
			{
				if (this.db.ReadOperation() == IscCodes.op_sql_response)
				{
					int messages = this.db.Receive.ReadInt32();
					if (messages > 0)
					{
						return this.ReadDataRow();
					}
					else
					{
						return null;
					}
				}
				else
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
			catch (IOException)
			{
				throw new IscException(IscCodes.isc_net_read_err);
			}
		}

		private DbValue[] ReadDataRow()
		{
			DbValue[] row = new DbValue[this.fields.Count];
			object value = null;

			lock (this.db)
			{
				// This	only works if not (port->port_flags	& PORT_symmetric)				
				for (int i = 0; i < this.fields.Count; i++)
				{
					try
					{
						value = this.db.Receive.ReadValue(this.fields[i]);
						row[i] = new DbValue(this, this.fields[i], value);
					}
					catch (IOException)
					{
						throw new IscException(IscCodes.isc_net_read_err);
					}
				}
			}

			return row;
		}

		#endregion

		#region Private	Methods

		private void Clear()
		{
			if (this.rows != null && this.rows.Count > 0)
			{
				this.rows.Clear();
			}
			if (this.outputParams != null && this.outputParams.Count > 0)
			{
				this.outputParams.Clear();
			}
		}

		private void Allocate()
		{
			lock (this.db)
			{
				try
				{
					this.db.Send.Write(IscCodes.op_allocate_statement);
					this.db.Send.Write(this.db.Handle);
					this.db.Send.Flush();

					this.handle = this.db.ReadGenericResponse().ObjectHandle;
					this.allRowsFetched = false;
					this.state = StatementState.Allocated;
					this.statementType = DbStatementType.None;
				}
				catch (IOException)
				{
					this.state = StatementState.Deallocated;
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		private Descriptor ParseSqlInfo(byte[] info, byte[] items)
		{
			Descriptor rowDesc = null;
			int lastindex = 0;

			while ((lastindex = this.ParseTruncSqlInfo(info, ref rowDesc, lastindex)) > 0)
			{
				lastindex--;			   // Is this OK ?

				byte[] new_items = new byte[4 + items.Length];

				new_items[0] = IscCodes.isc_info_sql_sqlda_start;
				new_items[1] = 2;
				new_items[2] = (byte)(lastindex & 255);
				new_items[3] = (byte)(lastindex >> 8);

				Array.Copy(items, 0, new_items, 4, items.Length);
				info = this.GetSqlInfo(new_items, info.Length);
			}

			return rowDesc;
		}

		private int ParseTruncSqlInfo(byte[] info, ref Descriptor rowDesc, int lastindex)
		{
			byte	item	= 0;
			int		index	= 0;
			int		i		= 2;

			int len = IscHelper.VaxInteger(info, i, 2);
			i += 2;
			int n = IscHelper.VaxInteger(info, i, len);
			i += len;

			if (rowDesc == null)
			{
				rowDesc = new Descriptor((short)n);
			}

			while (info[i] != IscCodes.isc_info_end)
			{
				while ((item = info[i++]) != IscCodes.isc_info_sql_describe_end)
				{
					switch (item)
					{
						case IscCodes.isc_info_sql_sqlda_seq:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							index = IscHelper.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_type:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].DataType = (short)IscHelper.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_sub_type:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].SubType = (short)IscHelper.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_scale:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].NumericScale = (short)IscHelper.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_length:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Length = (short)IscHelper.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_field:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Name = this.db.Charset.GetString(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_relation:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Relation = this.db.Charset.GetString(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_owner:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Owner = this.db.Charset.GetString(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_alias:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Alias = this.db.Charset.GetString(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_truncated:
							return lastindex;

						default:
							throw new IscException(IscCodes.isc_dsql_sqlda_err);
					}
				}

				lastindex = index;
			}

			return 0;
		}

		#endregion
	}
}
