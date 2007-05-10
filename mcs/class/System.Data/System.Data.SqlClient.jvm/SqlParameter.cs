//
// System.Data.SqlClient.SqlParameter
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

using System;
using System.Collections;
using System.Data;
using System.Data.ProviderBase;
using System.Data.Common;

using java.sql;

namespace System.Data.SqlClient
{
	public sealed class SqlParameter : AbstractDbParameter
	{
		#region Fields

		private SqlDbType _sqlDbType;

		#endregion // Fields

		#region Constructors

		public SqlParameter()
		{
		}

		public SqlParameter(String parameterName, Object value)
			: this(parameterName, SqlDbType.NVarChar, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, value,false)
		{
		}

		public SqlParameter(String parameterName, SqlDbType dbType)
			: this(parameterName, dbType, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null, true)
		{
		}

        
		public SqlParameter(String parameterName, SqlDbType dbType, int size)
			: this(parameterName, dbType, size, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null, true)
		{
		}


		public SqlParameter(String parameterName, SqlDbType dbType, int size, String sourceColumn)
			: this(parameterName, dbType, size, ParameterDirection.Input, false, 0, 0, sourceColumn, DataRowVersion.Current, null, true)
		{
		}

        
		public SqlParameter(
			String parameterName,
			SqlDbType dbType,
			int size,
			ParameterDirection direction,
			bool isNullable,
			byte precision,
			byte scale,
			String sourceColumn,
			DataRowVersion sourceVersion,
			Object value) : this(parameterName,dbType,size,direction,isNullable,precision,scale,sourceColumn,sourceVersion,value,true)
		{
		}

#if NET_2_0
		public SqlParameter (
			string parameterName,
			SqlDbType dbType,
			int size,
			ParameterDirection direction,
			byte precision,
			byte scale,
			string sourceColumn,
			DataRowVersion sourceVersion,
			bool sourceColumnNullMapping,
			Object value,
			string xmlSchemaCollectionDatabase,
			string xmlSchemaCollectionOwningSchema,
			string xmlSchemaCollectionName
		) : this (parameterName, dbType, size, direction, sourceColumnNullMapping, precision, scale, sourceColumn, sourceVersion, value, true)
		{
		}
#endif

		SqlParameter(
			String parameterName,
			SqlDbType dbType,
			int size,
			ParameterDirection direction,
			bool isNullable,
			byte precision,
			byte scale,
			String sourceColumn,
			DataRowVersion sourceVersion,
			Object value,
			bool dbTypeExplicit)
		{
			ParameterName = parameterName;
			SqlDbType = dbType;
			Size = size;
			Direction = direction;
			IsNullable = isNullable;
			Precision = precision;
			Scale = scale;
			SourceColumn = sourceColumn;
			SourceVersion = sourceVersion;
			if (!dbTypeExplicit) {
				IsDbTypeSet = false;
			}
			Value = value;
		}

		#endregion // Constructors

		#region Properties

		public override DbType DbType
        {
            get { return SqlConvert.SqlDbTypeToDbType(_sqlDbType); }           
			set { SqlDbType = SqlConvert.DbTypeToSqlDbType(value); }
        }                
        
        public SqlDbType SqlDbType
        {
            get { return _sqlDbType; }            
			set {
                _sqlDbType = value;
				IsDbTypeSet = true;
            }
        }                 

		public override int Size
		{
			get {
				int retVal = base.Size;
				return retVal;
			}
			set {
				if (value < 0) {
					throw ExceptionHelper.InvalidSizeValue(value);
				}

				if (value != 0) {
					base.Size = value;
				}
				else {
					base.Size = -1;
				}
			}
		}

#if NET_2_0
		public new byte Precision 
		{ 
			get { return base.Precision; }
			set { base.Precision = value; } 
		}

		public new byte Scale 
		{ 
			get { return base.Scale; }
			set { base.Scale = value; } 
		}
#endif

		protected internal override string Placeholder {
			get {
				if (ParameterName.Length == 0 || ParameterName[0] == '@')
					return ParameterName;

				return String.Concat("@", ParameterName);	
			}
		}

        
		public override Object Value
		{
			get { return base.Value; }
			set { 
				if (!IsDbTypeSet && (value != null) && (value != DBNull.Value)) {
                    _sqlDbType = SqlConvert.ValueTypeToSqlDbType(value.GetType());
				}
				base.Value = value; 
			}
		}

		#endregion // Properties

		#region Methods

		protected internal sealed override object ConvertValue(object value)
		{
			// can not convert null or DbNull to other types
			if (value == null || value == DBNull.Value) {
				return value;
			}
			// .NET throws an exception to the user.
			object convertedValue = value is IConvertible ? Convert.ChangeType(value,SqlConvert.SqlDbTypeToValueType(SqlDbType)) : value;
			return convertedValue;
		}

		protected internal sealed override void SetParameterName(ResultSet res)
		{
			string name = res.getString("COLUMN_NAME");
			if (name != null && name.Length > 0 && name[0] != '@')
				name = String.Concat("@", name);
			ParameterName = name;
		}

		protected internal sealed override void SetParameterDbType(ResultSet res)
		{
			int dataType = res.getInt("DATA_TYPE");
			SqlDbType = SqlConvert.JdbcTypeToSqlDbType(dataType);
			JdbcType = dataType;
		}

#if NET_2_0
		public void ResetSqlDbType ()
		{
			IsDbTypeSet = false;
		}

		public override void ResetDbType ()
		{
			ResetSqlDbType ();
		}
#endif

		protected internal sealed override void SetSpecialFeatures (ResultSet res)
		{
			// do nothing
		}

		protected internal sealed override int JdbcTypeFromProviderType()
		{
			return SqlConvert.SqlDbTypeToJdbcType(SqlDbType);
		}

		#endregion // Methods  

	}
}
