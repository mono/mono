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
	public class SqlParameter : AbstractDbParameter, IDbDataParameter, ICloneable
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
				_isDbTypeSet = false;
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
				_isDbTypeSet = true;
            }
        }                 
        
		public override byte Precision
		{
			get { return _precision; }
			set { _precision = value; }
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

		protected internal override string Placeholder {
			get {
				return ParameterName;
			}
		}

        
		public override Object Value
		{
			get { return base.Value; }
			set { 
				if (!_isDbTypeSet && (value != null) && (value != DBNull.Value)) {
                    _sqlDbType = SqlConvert.ValueTypeToSqlDbType(value.GetType());
				}
				base.Value = value; 
			}
		}

		#endregion // Properties

		#region Methods

		public override String ToString()
		{
			return ParameterName;
		}

		public override object Clone()
		{
            SqlParameter clone = new SqlParameter();
			CopyTo(clone);

            clone._sqlDbType = _sqlDbType;
            return clone;
		}

		protected internal sealed override object ConvertValue(object value)
		{
			// can not convert null or DbNull to other types
			if (value == null || value == DBNull.Value) {
				return value;
			}
			// .NET throws an exception to the user.
			object convertedValue = Convert.ChangeType(value,SqlConvert.SqlDbTypeToValueType(SqlDbType));
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
			JdbcType = (DbTypes.JavaSqlTypes) dataType;
		}

		protected internal sealed override void SetSpecialFeatures(ResultSet res)
		{
			// do nothing
		}

		protected internal sealed override DbTypes.JavaSqlTypes JdbcTypeFromProviderType()
		{
			return (DbTypes.JavaSqlTypes)SqlConvert.SqlDbTypeToJdbcType(SqlDbType);
		}

		#endregion // Methods  

	}
}