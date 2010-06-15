//
// Mono.Data.Tds.Protocol.Tds80.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//	 Veerapuram Varadhan  (vvaradhan@novell.com)
//
// Copyright (C) 2002 Tim Coleman
// Copyright (C) 2008,2009 Novell Inc.
//

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

using Mono.Data.Tds;
using System;

namespace Mono.Data.Tds.Protocol {
	public class Tds80 : Tds70
	{
		#region Fields

		public static readonly TdsVersion Version = TdsVersion.tds80;

		#endregion // Fields

		#region Constructors

		public Tds80 (string server, int port)
			: this (server, port, 512, 15)
		{
		}

		public Tds80 (string server, int port, int packetSize, int timeout)
			: base (server, port, packetSize, timeout, Version)
		{
		}

		#endregion // Constructors

		#region Properties
		
		protected override byte[] ClientVersion {
			get { return new byte[] {0x00, 0x0, 0x0, 0x71};}
		}
		#endregion // Properties
		
		#region Methods

		public override bool Connect (TdsConnectionParameters connectionParameters)
		{
			//Console.WriteLine ("Tds80::Connect");
			return base.Connect (connectionParameters);
		}

		protected override void ProcessColumnInfo ()
		{
			// We are connected to a Sql 7.0 server
			if (TdsVersion < TdsVersion.tds80) {
				base.ProcessColumnInfo ();
				return;
			}
			
			// VARADHAN: TDS 8 Debugging
			//Console.WriteLine ("Tds80.cs: In ProcessColumnInfo... entry");
			int numColumns = Comm.GetTdsShort ();
			//Console.WriteLine ("Column count={0}", numColumns); TDS 8 Debugging
			for (int i = 0; i < numColumns; i += 1) {
				byte[] flagData = new byte[4];
				for (int j = 0; j < 4; j += 1) 
					flagData[j] = Comm.GetByte ();

				bool nullable = (flagData[2] & 0x01) > 0;
				//bool caseSensitive = (flagData[2] & 0x02) > 0;
				bool writable = (flagData[2] & 0x0c) > 0;
				bool autoIncrement = (flagData[2] & 0x10) > 0;
				bool isIdentity = (flagData[2] & 0x10) > 0;

				TdsColumnType columnType = (TdsColumnType) (Comm.GetByte () & 0xff);
				//Console.WriteLine ("Actual ColumnType: {0}", columnType);  TDS 8 Debugging

				if ((byte) columnType == 0xef)
					columnType = TdsColumnType.NChar;

				TdsColumnType xColumnType = columnType;
				if (IsLargeType (columnType)) {
					if (columnType != TdsColumnType.NChar)
						columnType -= 128;
				}

				int columnSize;
				string tableName = null;
				byte[] collation = null;
				int lcid = 0, sortId = 0;

				if (IsBlobType (columnType)) {
					columnSize = Comm.GetTdsInt ();
				} else if (IsFixedSizeColumn (columnType)) {
					columnSize = LookupBufferSize (columnType);
				} else if (IsLargeType (xColumnType)) {
					columnSize = Comm.GetTdsShort ();
				} else  {
					columnSize = Comm.GetByte () & 0xff;
				}

				if (xColumnType == TdsColumnType.BigChar || xColumnType == TdsColumnType.BigNVarChar ||
				    xColumnType == TdsColumnType.BigVarChar || xColumnType == TdsColumnType.NChar ||
				    xColumnType == TdsColumnType.NVarChar ||   xColumnType == TdsColumnType.Text ||
				    xColumnType == TdsColumnType.NText) {
				    // Read collation for SqlServer 2000 and beyond
				    collation = Comm.GetBytes (5, true);
					lcid = TdsCollation.LCID (collation);
					sortId = TdsCollation.SortId (collation);
				}

				if (IsBlobType (columnType)) {
					tableName = Comm.GetString (Comm.GetTdsShort ());
					//Console.WriteLine ("Tablename: "+tableName);  TDS 8 Debugging
				}

				byte precision = 0;
				byte scale = 0;

				switch (columnType) {
				case TdsColumnType.NText:
				case TdsColumnType.NChar:
				case TdsColumnType.NVarChar:
					columnSize /= 2;
					break;
				case TdsColumnType.Decimal:
				case TdsColumnType.Numeric:
					//Comm.Skip (1);
					precision = Comm.GetByte ();
					//Console.WriteLine ("Precision: {0}", precision);  TDS 8 Debugging
					scale = Comm.GetByte ();
					//Console.WriteLine ("Scale: {0}", scale);  TDS 8 Debugging
					break;
				}

				string columnName = Comm.GetString (Comm.GetByte ());

				TdsDataColumn col = new TdsDataColumn ();
				Columns.Add (col);
#if NET_2_0
				col.ColumnType = columnType;
				col.ColumnName = columnName;
				col.IsAutoIncrement = autoIncrement;
				col.IsIdentity = isIdentity;
				col.ColumnSize = columnSize;
				col.NumericPrecision = precision;
				col.NumericScale = scale;
				col.IsReadOnly = !writable;
				col.AllowDBNull = nullable;
				col.BaseTableName = tableName;
				col.LCID = lcid;
				col.SortOrder = sortId;
#else
				col ["ColumnType"] = columnType;
				col ["ColumnName"] = columnName;
				col ["IsAutoIncrement"] = autoIncrement;
				col ["IsIdentity"] = isIdentity;
				col ["ColumnSize"] = columnSize;
				col ["NumericPrecision"] = precision;
				col ["NumericScale"] = scale;
				col ["IsReadOnly"] = !writable;
				col ["AllowDBNull"] = nullable;
				col ["BaseTableName"] = tableName;
				col ["LCID"] = lcid;
				col ["SortOrder"] = sortId;
#endif
			}
			//Console.WriteLine ("Tds80.cs: In ProcessColumnInfo... exit");  TDS 8 Debugging
		}

		protected override void ProcessOutputParam ()
		{
			// We are connected to a Sql 7.0 server
			if (TdsVersion < TdsVersion.tds80) {
				base.ProcessOutputParam ();
				return;
			}

			GetSubPacketLength ();
			
			Comm.Skip ((Comm.GetByte () & 0xff) <<1); // Parameter name
			Comm.Skip (1); 	// Status: 0x01 - in case of OUTPUT parameter
							// Status: 0x02 - in case of return value of UDF
			Comm.Skip (4);  // Usertype - sizeof (ULong)

			TdsColumnType colType = (TdsColumnType) Comm.GetByte ();
			object value = GetColumnValue (colType, true);
			OutputParameters.Add (value);
		}
		
		public override void Execute (string commandText, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
		{
			// We are connected to a Sql 7.0 server
			if (TdsVersion < TdsVersion.tds80) {
				base.Execute (commandText, parameters, timeout, wantResults);
				return;
			}

			Parameters = parameters;
			string sql = commandText;

			if (Parameters != null && Parameters.Count > 0) {
				ExecRPC (TdsRpcProcId.ExecuteSql, commandText, parameters, timeout, wantResults);
			} else {
				if (wantResults)
					sql = BuildExec (commandText);
				ExecuteQuery (sql, timeout, wantResults);
			}
		}

		#endregion // Methods
	}
}
