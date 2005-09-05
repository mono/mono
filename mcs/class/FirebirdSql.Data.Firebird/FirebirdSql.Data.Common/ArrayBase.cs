/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Text;
using System.Globalization;

namespace FirebirdSql.Data.Common
{
	internal abstract class ArrayBase
	{
		#region Fields

		private ArrayDesc	descriptor;
		private string		tableName;
		private string		fieldName;
		private string		rdbFieldName;

		#endregion

		#region Properties

		public ArrayDesc Descriptor
		{
			get { return this.descriptor; }
		}

		#endregion

		#region Abstract Properties

		public abstract long Handle
		{
			get;
			set;
		}

		public abstract IDatabase DB
		{
			get;
			set;
		}

		public abstract ITransaction Transaction
		{
			get;
			set;
		}

		#endregion

		#region Constructors

		protected ArrayBase(ArrayDesc descriptor)
		{
			this.tableName	= descriptor.RelationName;
			this.fieldName	= descriptor.FieldName;
			this.descriptor = descriptor;
		}

		protected ArrayBase(string tableName, string fieldName)
		{
			this.tableName		= tableName;
			this.fieldName		= fieldName;
			this.rdbFieldName	= String.Empty;
		}

		#endregion

		#region Abstract Methods

		public abstract byte[] GetSlice(int slice_length);
		public abstract void PutSlice(System.Array source_array, int slice_length);

		#endregion

		#region Protected Abstract Methods

		protected abstract System.Array DecodeSlice(byte[] slice);

		#endregion

		#region Methods

		public Array Read()
		{
			byte[] slice = this.GetSlice(this.GetSliceLength(true));

			return this.DecodeSlice(slice);
		}

		public void Write(System.Array sourceArray)
		{
			this.SetDesc(sourceArray);
			this.PutSlice(sourceArray, this.GetSliceLength(false));
		}

		public void SetDesc(System.Array sourceArray)
		{
			this.descriptor.Dimensions = (short)sourceArray.Rank;

			for (int i = 0; i < sourceArray.Rank; i++)
			{
				int lb = this.descriptor.Bounds[i].LowerBound;
				int ub = sourceArray.GetLength(i) - 1 + lb;

				this.descriptor.Bounds[i].UpperBound = ub;
			}
		}

		public void LookupBounds()
		{
			this.LookupDesc();

			StatementBase lookup = this.DB.CreateStatement(this.Transaction);

			lookup.Prepare(this.GetArrayBounds());
			lookup.Execute();

			int i = 0;
			this.descriptor.Bounds = new ArrayBound[16];
			DbValue[] values;

			while ((values = lookup.Fetch()) != null)
			{
				this.descriptor.Bounds[i].LowerBound = values[0].GetInt32();
				this.descriptor.Bounds[i].UpperBound = values[1].GetInt32();

				i++;
			}

			lookup.Release();
			lookup = null;
		}

		public void LookupDesc()
		{
			// Initializa array descriptor information
			this.descriptor = new ArrayDesc();

			// Create statement for retrieve information
			StatementBase lookup = this.DB.CreateStatement(this.Transaction);

			lookup.Prepare(this.GetArrayDesc());
			lookup.Execute();

			DbValue[] values = lookup.Fetch();
			if (values != null && values.Length > 0)
			{
				this.descriptor.RelationName	= tableName;
				this.descriptor.FieldName		= fieldName;
				this.descriptor.DataType		= values[0].GetByte();
				this.descriptor.Scale			= values[1].GetInt16();
				this.descriptor.Length			= values[2].GetInt16();
				this.descriptor.Dimensions		= values[3].GetInt16();
				this.descriptor.Flags			= 0;

				this.rdbFieldName = values[4].GetString().Trim();
			}
			else
			{
				throw new InvalidOperationException();
			}

			lookup.Release();
			lookup = null;
		}

		#endregion

		#region Protected Methods

		protected int GetSliceLength(bool read)
		{
			int length = 0;
			int elements = 0;

			for (int i = 0; i < this.descriptor.Dimensions; i++)
			{
				ArrayBound bound = this.descriptor.Bounds[i];

				elements += (bound.UpperBound - bound.LowerBound) + 1;
			}

			length = elements * this.descriptor.Length;

			switch (this.descriptor.DataType)
			{
				case IscCodes.blr_varying:
				case IscCodes.blr_varying2:
					length += elements * 2;
					break;
			}

			return length;
		}

		protected Type GetSystemType()
		{
			Type systemType;

			switch (this.descriptor.DataType)
			{
				case IscCodes.blr_text:
				case IscCodes.blr_text2:
				case IscCodes.blr_cstring:
				case IscCodes.blr_cstring2:
					// Char
					systemType = typeof(System.String);
					break;

				case IscCodes.blr_varying:
				case IscCodes.blr_varying2:
					// VarChar
					systemType = typeof(System.String);
					break;

				case IscCodes.blr_short:
					// Short/Smallint
					if (this.descriptor.Scale < 0)
					{
						systemType = typeof(System.Decimal);
					}
					else
					{
						systemType = typeof(System.Int16);
					}
					break;

				case IscCodes.blr_long:
					// Integer
					if (this.descriptor.Scale < 0)
					{
						systemType = typeof(System.Decimal);
					}
					else
					{
						systemType = typeof(System.Int32);
					}
					break;

				case IscCodes.blr_float:
					// Float
					systemType = typeof(System.Single);
					break;

				case IscCodes.blr_double:
				case IscCodes.blr_d_float:
					// Double
					systemType = typeof(System.Double);
					break;

				case IscCodes.blr_quad:
				case IscCodes.blr_int64:
					// Long/Quad
					if (this.descriptor.Scale < 0)
					{
						systemType = typeof(System.Decimal);
					}
					else
					{
						systemType = typeof(System.Int64);
					}
					break;

				case IscCodes.blr_timestamp:
					// Timestamp
					systemType = typeof(System.DateTime);
					break;

				case IscCodes.blr_sql_time:
					// Time
					systemType = typeof(System.DateTime);
					break;

				case IscCodes.blr_sql_date:
					// Date
					systemType = typeof(System.DateTime);
					break;

				default:
					throw new NotSupportedException("Unknown data type");
			}

			return systemType;
		}

		#endregion

		#region Private Methods

		private string GetArrayDesc()
		{
			StringBuilder sql = new StringBuilder();

			sql.Append(
				"SELECT Y.RDB$FIELD_TYPE, Y.RDB$FIELD_SCALE, Y.RDB$FIELD_LENGTH, Y.RDB$DIMENSIONS, X.RDB$FIELD_SOURCE " +
				"FROM RDB$RELATION_FIELDS X, RDB$FIELDS Y " +
				"WHERE X.RDB$FIELD_SOURCE = Y.RDB$FIELD_NAME ");

			if (this.tableName != null && this.tableName.Length != 0)
			{
				sql.AppendFormat(
					CultureInfo.CurrentCulture, " AND X.RDB$RELATION_NAME = '{0}'", tableName);
			}

			if (this.fieldName != null && this.fieldName.Length != 0)
			{
				sql.AppendFormat(
					CultureInfo.CurrentCulture, " AND X.RDB$FIELD_NAME = '{0}'", fieldName);
			}

			return sql.ToString();
		}

		private string GetArrayBounds()
		{
			StringBuilder sql = new StringBuilder();

			sql.Append("SELECT X.RDB$LOWER_BOUND, X.RDB$UPPER_BOUND FROM RDB$FIELD_DIMENSIONS X ");

			if (this.fieldName != null && this.fieldName.Length != 0)
			{
				sql.AppendFormat(
					CultureInfo.CurrentCulture, "WHERE X.RDB$FIELD_NAME = '{0}'", rdbFieldName);
			}

			sql.Append(" ORDER BY X.RDB$DIMENSION");

			return sql.ToString();
		}

		#endregion
	}
}