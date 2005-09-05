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
using System.Text;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal class FbDataTypes
	{
		#region Constructors

		private FbDataTypes()
		{
		}

		#endregion

		public static DataTable GetSchema()
		{
			DataTable table = new DataTable("DataTypes");

			table.Columns.Add("TypeName", typeof(System.String));
			table.Columns.Add("DbType", typeof(System.Int32));
			table.Columns.Add("ProviderDbType", typeof(System.Int32));
			table.Columns.Add("ColumnSize", typeof(System.Int64));
			table.Columns.Add("CreateFormat", typeof(System.String));
			table.Columns.Add("CreateParameters", typeof(System.String));
			table.Columns.Add("DataType", typeof(System.String));
			table.Columns.Add("IsAutoIncrementable", typeof(System.Boolean));
			table.Columns.Add("IsBestMatch", typeof(System.Boolean));
			table.Columns.Add("IsCaseSensitive", typeof(System.Boolean));
			table.Columns.Add("IsFixedLength", typeof(System.Boolean));
			table.Columns.Add("IsFixedPrecisionScale", typeof(System.Boolean));
			table.Columns.Add("IsLong", typeof(System.Boolean));
			table.Columns.Add("IsNullable", typeof(System.Boolean));
			table.Columns.Add("IsSearchable", typeof(System.Boolean));
			table.Columns.Add("IsSearchableWithLike", typeof(System.Boolean));
			table.Columns.Add("IsUnsigned", typeof(System.Boolean));
			table.Columns.Add("MaximumScale", typeof(System.Int16));
			table.Columns.Add("MinimumScale", typeof(System.Int16));
			table.Columns.Add("IsConcurrencyType", typeof(System.Boolean));
			table.Columns.Add("IsLiteralSupported", typeof(System.Boolean));
			table.Columns.Add("LiteralPrefix", typeof(System.String));
			table.Columns.Add("LiteralSuffix", typeof(System.String));

			DataRowCollection r = table.Rows;

			r.Add(new object[] { "array", DbType.Object, FbDbType.Array, Int32.MaxValue, "", "", "System.Array", false, false, false, false, false, true, true, false, false, false, 0, 0, false, false, null, null });
			r.Add(new object[] { "bigint", DbType.Int64, FbDbType.BigInt, 8, "", "", "System.Int64", false, false, false, true, true, false, true, true, true, false, 0, 0, false, false, null, null });
			r.Add(new object[] { "blob", DbType.Binary, FbDbType.Binary, Int32.MaxValue, "", "", "System.Byte[]", false, false, false, false, false, true, true, false, false, false, 0, 0, false, false, null, null });
			r.Add(new object[] { "blob sub_type 1", DbType.String, FbDbType.Text, Int32.MaxValue, "", "", "System.String", false, false, true, true, false, true, true, true, true, false, 0, 0, false, true, "'", "'" });
			r.Add(new object[] { "char", DbType.StringFixedLength, FbDbType.Char, Int16.MaxValue, "", "length", "System.String", false, false, true, false, false, false, true, true, true, false, 0, 0, false, true, "'", "'" });
			r.Add(new object[] { "date", DbType.Date, FbDbType.Date, 4, "", "", "System.DateTime", false, false, false, true, false, false, true, true, true, false, 0, 0, false, null, null });
			r.Add(new object[] { "decimal", DbType.Decimal, FbDbType.Decimal, 0, "", "precision,scale", "System.Decimal", false, false, false, true, false, false, true, true, true, false, 18, 0, false, null, null });
			r.Add(new object[] { "double precision", DbType.Double, FbDbType.Double, 8, "", "", "System.Double", false, false, false, true, true, false, true, true, true, false, 0, 0, false, null, null });
			r.Add(new object[] { "float", DbType.Single, FbDbType.Float, 4, "", "", "System.Float", false, false, false, true, true, false, true, true, true, false, 0, 0, false, null, null });
			r.Add(new object[] { "integer", DbType.Int32, FbDbType.Integer, 4, "", "", "System.Int32", false, false, false, true, true, false, true, true, true, false, 0, 0, false, null, null });
			r.Add(new object[] { "numeric", DbType.Decimal, FbDbType.Numeric, 0, "", "precision,scale", "System.Decimal", false, false, false, true, false, false, true, true, true, false, 18, 0, false, null, null });
			r.Add(new object[] { "smallint", DbType.Int16, FbDbType.SmallInt, 2, "", "", "System.Int16", false, false, false, true, true, false, true, true, true, false, 0, 0, false, null, null });
			r.Add(new object[] { "time", DbType.Time, FbDbType.Time, 4, "", "", "System.DateTime", false, false, false, true, false, false, true, true, true, false, 0, 0, false, null, null });
			r.Add(new object[] { "timestamp", DbType.DateTime, FbDbType.TimeStamp, 8, "", "", "System.DateTime", false, false, false, true, false, false, true, true, true, false, 0, 0, false, null, null });
			r.Add(new object[] { "varchar", DbType.String, FbDbType.VarChar, Int16.MaxValue, "", "length", "System.String", false, false, true, false, false, false, true, true, true, false, 0, 0, false, true, "'", "'" });

			return table;
		}
	}
}