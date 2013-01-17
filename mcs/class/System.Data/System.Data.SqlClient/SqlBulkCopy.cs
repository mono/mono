//
// System.Data.SqlClient.SqlBulkCopy.cs
//
// Author:
//   Nagappan A (anagappan@novell.com)
//
// (C) Novell, Inc 2007

//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Data;
using System.Data.Common;
using Mono.Data.Tds;
using Mono.Data.Tds.Protocol;

namespace System.Data.SqlClient {
	/// <summary>Efficient way to bulk load SQL Server table with several data rows at once</summary>
	public sealed class SqlBulkCopy : IDisposable 
	{
		#region Constants
		private const string transConflictMessage = "Must not specify SqlBulkCopyOptions.UseInternalTransaction " +
			"and pass an external Transaction at the same time.";
		
		private const SqlBulkCopyOptions insertModifiers =
			SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.TableLock |
			SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.FireTriggers;
		#endregion
		
		#region Fields

		private int _batchSize = 0;
		private int _notifyAfter = 0;
		private int _bulkCopyTimeout = 0;
		private SqlBulkCopyColumnMappingCollection _columnMappingCollection = new SqlBulkCopyColumnMappingCollection ();
		private string _destinationTableName = null;
		private bool ordinalMapping = false;
		private bool sqlRowsCopied = false;
		private bool isLocalConnection = false;
		private SqlConnection connection;
		private SqlTransaction externalTransaction;
		private SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default;

		#endregion

		#region Constructors
		public SqlBulkCopy (SqlConnection connection)
		{
			if (connection == null) {
				throw new ArgumentNullException("connection");
			}
			
			this.connection = connection;
		}

		public SqlBulkCopy (string connectionString)
		{
			if (connectionString == null) {
				throw new ArgumentNullException("connectionString");
			}
			
			this.connection = new SqlConnection (connectionString);
			isLocalConnection = true;
		}

		[MonoTODO]
		public SqlBulkCopy (string connectionString, SqlBulkCopyOptions copyOptions)
		{
			if (connectionString == null) {
				throw new ArgumentNullException ("connectionString");
			}
			
			this.connection = new SqlConnection (connectionString);
			isLocalConnection = true;
			
			if ((copyOptions & SqlBulkCopyOptions.UseInternalTransaction) == SqlBulkCopyOptions.UseInternalTransaction)
				throw new NotImplementedException ("We don't know how to process UseInternalTransaction option.");
			
			this.copyOptions = copyOptions;
		}

		[MonoTODO]
		public SqlBulkCopy (SqlConnection connection, SqlBulkCopyOptions copyOptions, SqlTransaction externalTransaction)
		{
			if (connection == null) {
				throw new ArgumentNullException ("connection");
			}
			
			this.connection = connection;
			this.copyOptions = copyOptions;
			
			if ((copyOptions & SqlBulkCopyOptions.UseInternalTransaction) == SqlBulkCopyOptions.UseInternalTransaction) {
				if (externalTransaction != null)
					throw new ArgumentException (transConflictMessage);
			}
			else
				this.externalTransaction = externalTransaction;
			
			if ((copyOptions & SqlBulkCopyOptions.UseInternalTransaction) == SqlBulkCopyOptions.UseInternalTransaction)
				throw new NotImplementedException ("We don't know how to process UseInternalTransaction option.");
			
			this.copyOptions = copyOptions;
		}

		#endregion

		#region Properties

		public int BatchSize {
			get { return _batchSize; }
			set { _batchSize = value; }
		}

		public int BulkCopyTimeout {
			get { return _bulkCopyTimeout; }
			set { _bulkCopyTimeout = value; }
		}

		public SqlBulkCopyColumnMappingCollection ColumnMappings  {
			get { return _columnMappingCollection; }
		}

		public string DestinationTableName {
			get { return _destinationTableName; }
			set { _destinationTableName = value; }
		}

		public int NotifyAfter {
			get { return _notifyAfter; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("NotifyAfter should be greater than or equal to 0");
				_notifyAfter = value;
			}
		}

		#endregion

		#region Methods

		public void Close ()
		{
			if (sqlRowsCopied == true) {
				throw new InvalidOperationException ("Close should not be called from SqlRowsCopied event");
			}
			if (connection == null || connection.State == ConnectionState.Closed) {
				return;
			}
			connection.Close ();
		}

		private DataTable [] GetColumnMetaData ()
		{
			DataTable [] columnMetaDataTables = new DataTable [2];
			SqlCommand cmd = new SqlCommand ("select @@trancount; " +
							 "set fmtonly on select * from " +
							 DestinationTableName + " set fmtonly off;" +
							 "exec sp_tablecollations_90 '" +
							 DestinationTableName + "'",
							 connection);
			SqlDataReader reader = cmd.ExecuteReader ();
			int i = 0; // Skipping 1st result
			do {
				  if (i == 1) {
					columnMetaDataTables [i - 1] = reader.GetSchemaTable ();
				  } else if (i == 2) {
					SqlDataAdapter adapter = new SqlDataAdapter ();
					adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
					columnMetaDataTables [i - 1] = new DataTable ();
					adapter.FillInternal (columnMetaDataTables [i - 1], reader);
				}
				i++;
			} while (reader.IsClosed == false && reader.NextResult());
			reader.Close ();
			return columnMetaDataTables;
		}

		private string GenerateColumnMetaData (SqlCommand tmpCmd, DataTable colMetaData, DataTable tableCollations)
		{
			bool flag = false;
			string statement = "";
			int i = 0;
			foreach (DataRow row in colMetaData.Rows) {
				flag = false;
				foreach (DataColumn col in colMetaData.Columns) { // FIXME: This line not required, remove later
					object value = null;
					if (_columnMappingCollection.Count > 0) {
						if (ordinalMapping) {
							foreach (SqlBulkCopyColumnMapping mapping
								 in _columnMappingCollection) {
								if (mapping.DestinationOrdinal == i) {
									flag = true;
									break;
								}
							}
						} else {
							foreach (SqlBulkCopyColumnMapping mapping
								 in _columnMappingCollection) {
								if (mapping.DestinationColumn == (string) row ["ColumnName"]) {
									flag = true;
									break;
								}
							}
						}
						if (flag == false)
							break;
					}
					if ((bool)row ["IsReadOnly"]) {
						if (ordinalMapping)
							value = false;
						else
							break;
					}
					SqlParameter param = new SqlParameter ((string) row ["ColumnName"],
									       ((SqlDbType) row ["ProviderType"]));
					param.Value = value;
					if ((int)row ["ColumnSize"] != -1) {
						param.Size = (int) row ["ColumnSize"];
					}
					tmpCmd.Parameters.Add (param);
					break;
				}
				i++;
			}
			flag = false;
			bool insertSt = false;
			foreach (DataRow row in colMetaData.Rows) {
				if (_columnMappingCollection.Count > 0) {
					i = 0;
					insertSt = false;
					foreach (SqlParameter param in tmpCmd.Parameters) {
						if (ordinalMapping) {
							foreach (SqlBulkCopyColumnMapping mapping
								 in _columnMappingCollection) {
								if (mapping.DestinationOrdinal == i && param.Value == null) {
									insertSt = true;
								}
							}
						} else {
							foreach (SqlBulkCopyColumnMapping mapping
								 in _columnMappingCollection) {
								if (mapping.DestinationColumn == param.ParameterName &&
								    (string)row ["ColumnName"] == param.ParameterName) {
									insertSt = true;
									param.Value = null;
								}
							}
						}
						i++;
						if (insertSt == true)
							break;
					}
					if (insertSt == false)
						continue;
				}
				if ((bool)row ["IsReadOnly"]) {
					continue;
				}
				string columnInfo = "";
				if ((int)row ["ColumnSize"] != -1) {
					columnInfo = string.Format ("{0}({1})",
								    (SqlDbType) row ["ProviderType"],
								    row ["ColumnSize"]);
				} else {
					columnInfo = string.Format ("{0}", (SqlDbType) row ["ProviderType"]);
				}
				if (flag)
					statement += ", ";
				string columnName = (string) row ["ColumnName"];
				statement += string.Format ("[{0}] {1}", columnName, columnInfo);
				if (flag == false)
					flag = true;
				if (tableCollations != null) {
					foreach (DataRow collationRow in tableCollations.Rows) {
						if ((string)collationRow ["name"] == columnName) {
							statement += string.Format (" COLLATE {0}", collationRow ["collation"]);
							break;
						}
					}
				}
			}
			return statement;
		}

		private void ValidateColumnMapping (DataTable table, DataTable tableCollations)
		{
			foreach (SqlBulkCopyColumnMapping _columnMapping in _columnMappingCollection) {
				if (ordinalMapping == false &&
				    (_columnMapping.DestinationColumn == String.Empty ||
				     _columnMapping.SourceColumn == String.Empty))
					throw new InvalidOperationException ("Mappings must be either all null or ordinal");
				if (ordinalMapping &&
				    (_columnMapping.DestinationOrdinal == -1 ||
				     _columnMapping.SourceOrdinal == -1))
					throw new InvalidOperationException ("Mappings must be either all null or ordinal");
				bool flag = false;
				if (ordinalMapping == false) {
					foreach (DataRow row in tableCollations.Rows) {
						if ((string)row ["name"] == _columnMapping.DestinationColumn) {
							flag = true;
							break;
						}
					}
					if (flag == false)
						throw new InvalidOperationException ("ColumnMapping does not match");
					flag = false;
					foreach (DataColumn col in table.Columns) {
						if (col.ColumnName == _columnMapping.SourceColumn) {
							flag = true;
							break;
						}
					}
					if (flag == false)
						throw new InvalidOperationException ("ColumnName " +
										     _columnMapping.SourceColumn +
										     " does not match");
				} else {
					if (_columnMapping.DestinationOrdinal >= tableCollations.Rows.Count)
						throw new InvalidOperationException ("ColumnMapping does not match");
				}
			}
		}

		private void BulkCopyToServer (DataTable table, DataRowState state)
		{
			if (connection == null || connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ("This method should not be called on a closed connection");
			if (_destinationTableName == null)
				throw new ArgumentNullException ("DestinationTableName");
			if (isLocalConnection && connection.State != ConnectionState.Open)
				connection.Open();
			
			if ((copyOptions & SqlBulkCopyOptions.KeepIdentity) == SqlBulkCopyOptions.KeepIdentity) {
				SqlCommand cmd = new SqlCommand ("set identity_insert " +
								 table.TableName + " on",
								 connection);
				cmd.ExecuteScalar ();
			}
			DataTable [] columnMetaDataTables = GetColumnMetaData ();
			DataTable colMetaData = columnMetaDataTables [0];
			DataTable tableCollations = columnMetaDataTables [1];

			if (_columnMappingCollection.Count > 0) {
				if (_columnMappingCollection [0].SourceOrdinal != -1)
					ordinalMapping = true;
				ValidateColumnMapping (table, tableCollations);
			}

			SqlCommand tmpCmd = new SqlCommand ();
			TdsBulkCopy blkCopy = new TdsBulkCopy ((Tds)connection.Tds);
			if (((Tds)connection.Tds).TdsVersion >= TdsVersion.tds70) {
				string statement = "insert bulk " + DestinationTableName + " (";
				statement += GenerateColumnMetaData (tmpCmd, colMetaData, tableCollations);
				statement += ")";
				
				#region Check requested options and add corresponding modifiers to the statement
				if ((copyOptions & insertModifiers) != SqlBulkCopyOptions.Default) {
					statement += " WITH (";
					bool commaRequired = false;
					
					if ((copyOptions & SqlBulkCopyOptions.CheckConstraints) == SqlBulkCopyOptions.CheckConstraints) {
						if (commaRequired)
							statement += ", ";
						statement += "CHECK_CONSTRAINTS";
						commaRequired = true;
					}
					
					if ((copyOptions & SqlBulkCopyOptions.TableLock) == SqlBulkCopyOptions.TableLock) {
						if (commaRequired)
							statement += ", ";
						statement += "TABLOCK";
						commaRequired = true;
					}
					
					if ((copyOptions & SqlBulkCopyOptions.KeepNulls) == SqlBulkCopyOptions.KeepNulls) {
						if (commaRequired)
							statement += ", ";
						statement += "KEEP_NULLS";
						commaRequired = true;
					}
					
					if ((copyOptions & SqlBulkCopyOptions.FireTriggers) == SqlBulkCopyOptions.FireTriggers) {
						if (commaRequired)
							statement += ", ";
						statement += "FIRE_TRIGGERS";
						commaRequired = true;
					}
					
					statement += ")";
				}
				#endregion Check requested options and add corresponding modifiers to the statement
				
				blkCopy.SendColumnMetaData (statement);
			}
			blkCopy.BulkCopyStart (tmpCmd.Parameters.MetaParameters);
			long noRowsCopied = 0;
			foreach (DataRow row in table.Rows) {
				if (row.RowState == DataRowState.Deleted)
					continue; // Don't copy the row that's in deleted state
				if (state != 0 && row.RowState != state)
					continue;
				bool isNewRow = true;
				int i = 0;
				foreach (SqlParameter param in tmpCmd.Parameters) {
					int size = 0;
					object rowToCopy = null;
					if (_columnMappingCollection.Count > 0) {
						if (ordinalMapping) {
							foreach (SqlBulkCopyColumnMapping mapping
								 in _columnMappingCollection) {
								if (mapping.DestinationOrdinal == i && param.Value == null) {
									rowToCopy = row [mapping.SourceOrdinal];
									SqlParameter parameter = new SqlParameter (mapping.SourceOrdinal.ToString (),
														   rowToCopy);
									if (param.MetaParameter.TypeName != parameter.MetaParameter.TypeName) {
										parameter.SqlDbType = param.SqlDbType;
										rowToCopy = parameter.Value = parameter.ConvertToFrameworkType (rowToCopy);
									}
									string colType = string.Format ("{0}", parameter.MetaParameter.TypeName);
									if (colType == "nvarchar") {
										if (row [i] != null) {
											size = ((string) parameter.Value).Length;
											size <<= 1;
										}
									} else {
										size = parameter.Size;
									}
									break;
								}
							}
						} else {
							foreach (SqlBulkCopyColumnMapping mapping
								 in _columnMappingCollection) {
								if (mapping.DestinationColumn == param.ParameterName) {
									rowToCopy = row [mapping.SourceColumn];
									SqlParameter parameter = new SqlParameter (mapping.SourceColumn, rowToCopy);
									if (param.MetaParameter.TypeName != parameter.MetaParameter.TypeName) {
										parameter.SqlDbType = param.SqlDbType;
										rowToCopy = parameter.Value = parameter.ConvertToFrameworkType (rowToCopy);
									}
									string colType = string.Format ("{0}", parameter.MetaParameter.TypeName);
									if (colType == "nvarchar") {
										if (row [mapping.SourceColumn] != null) {
											size = ((string) rowToCopy).Length;
											size <<= 1;
										}
									} else {
										size = parameter.Size;
									}
									break;
								}
							}
						}
						i++;
					} else {
						rowToCopy = row [param.ParameterName];
						string colType = param.MetaParameter.TypeName;
						/*
						  If column type is SqlDbType.NVarChar the size of parameter is multiplied by 2
						  FIXME: Need to check for other types
						*/
						if (colType == "nvarchar") {
							size = ((string) row [param.ParameterName]).Length;
							size <<= 1;
						} else {
							size = param.Size;
						}
					}
					if (rowToCopy == null)
						continue;
					blkCopy.BulkCopyData (rowToCopy, size, isNewRow);
					if (isNewRow)
						isNewRow = false;
				} // foreach (SqlParameter)
				if (_notifyAfter > 0) {
					noRowsCopied ++;
					if (noRowsCopied >= _notifyAfter) {
						RowsCopied (noRowsCopied);
						noRowsCopied = 0;
					}
				}
			} // foreach (DataRow)
			blkCopy.BulkCopyEnd ();
		}

		public void WriteToServer (DataRow [] rows)
		{
			if (rows == null)
				throw new ArgumentNullException ("rows");
			if (rows.Length == 0)
				return;
			DataTable table = new DataTable (rows [0].Table.TableName);
			foreach (DataColumn col in rows [0].Table.Columns) {
				DataColumn tmpCol = new DataColumn (col.ColumnName, col.DataType);
				table.Columns.Add (tmpCol);
			}
			foreach (DataRow row in rows) {
				DataRow tmpRow = table.NewRow ();
				for (int i = 0; i < table.Columns.Count; i++) {
					tmpRow [i] = row [i];
				}
				table.Rows.Add (tmpRow);
			}
			BulkCopyToServer (table, 0);
		}

		public void WriteToServer (DataTable table)
		{
			BulkCopyToServer (table, 0);
		}

		public void WriteToServer (IDataReader reader)
		{
			DataTable table = new DataTable ();
			SqlDataAdapter adapter = new SqlDataAdapter ();
			adapter.FillInternal (table, reader);
			BulkCopyToServer (table, 0);
		}

		public void WriteToServer (DataTable table, DataRowState rowState)
		{
			BulkCopyToServer (table, rowState);
		}

		private void RowsCopied (long rowsCopied)
		{
			SqlRowsCopiedEventArgs e = new SqlRowsCopiedEventArgs (rowsCopied);
			if (null != SqlRowsCopied) {
				SqlRowsCopied (this, e);
			}
		}

		#endregion

		#region Events

		public event SqlRowsCopiedEventHandler SqlRowsCopied;

		#endregion

		void IDisposable.Dispose ()
		{
			//throw new NotImplementedException ();
			if (isLocalConnection) {
				Close ();
				connection = null;
			}
		}

	}
}

#endif
