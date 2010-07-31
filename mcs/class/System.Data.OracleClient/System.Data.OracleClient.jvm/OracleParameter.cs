//
// System.Data.OracleClient.OracleParameter
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
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Globalization;

using java.sql;
using java.lang;

namespace System.Data.OracleClient {
	public sealed class OracleParameter : AbstractDbParameter, ICloneable {

		#region Fields

		OracleType _oleDbType = OracleType.VarChar;

		#endregion // Fields
    
		#region Constructors

		public OracleParameter() {
		}
    
		public OracleParameter(String parameterName, Object value)
			: this (parameterName, OracleType.VarChar, 0, ParameterDirection.Input,
			false, 0, 0, String.Empty, DataRowVersion.Current, value) {
			IsDbTypeSet = false;
		}
    
		public OracleParameter(String parameterName, OracleType dbType)
			: this (parameterName, dbType, 0, ParameterDirection.Input,
			false, 0, 0, String.Empty, DataRowVersion.Current, null) {
		}
    
		public OracleParameter(String parameterName, OracleType dbType, int size)
			: this (parameterName, dbType, size, ParameterDirection.Input,
			false, 0, 0, String.Empty, DataRowVersion.Current, null) {
		}
    
		public OracleParameter(String parameterName, OracleType dbType, int size,
			String sourceColumn)
			: this (parameterName, dbType, size, ParameterDirection.Input,
			false, 0, 0, sourceColumn, DataRowVersion.Current, null) {
		}
    
        
		public OracleParameter(String parameterName, 
			OracleType dbType, 
			int size,
			ParameterDirection direction, 
			bool isNullable,
			byte precision, 
			byte scale, 
			String sourceColumn,
			DataRowVersion sourceVersion, 
			Object value) {
			ParameterName = parameterName;
			OracleType = dbType;
			Size = size;
			Direction = direction;
			IsNullable = isNullable;
			Precision = precision;
			Scale = scale;
			SourceColumn = sourceColumn;
			SourceVersion = sourceVersion;
			Value = value;
		}

		#endregion // Constructors

		#region Properties

		public override DbType DbType {
			get { return OracleConvert.OracleTypeToDbType(_oleDbType); }           
			set { OracleType = OracleConvert.DbTypeToOracleType(value); }
		}                
        
		public OracleType OracleType {
			get { return _oleDbType; }            
			set {
				_oleDbType = value;
				IsDbTypeSet = true;
			}
		}    
    
		public new Object Value {
			get { return base.Value; }
			set {
				if (!IsDbTypeSet && (value != null) && (value != DBNull.Value)) {
					_oleDbType = OracleConvert.ValueTypeToOracleType(value.GetType());
				}
				base.Value = value;
			}
		}

#if NET_2_0
		public new byte Precision {
			get { return base.Precision; }
			set { base.Precision = value; }
		}

		public new byte Scale {
			get { return base.Scale; }
			set { base.Scale = value; }
		}
#endif

		#endregion // Properties

		#region Methods

		public override String ToString() {
			return ParameterName;
		}

		protected override string Placeholder {
			get {
				if (ParameterName.Length == 0 || ParameterName[0] == ':')
					return ParameterName;

				return String.Concat(":", ParameterName);
			}
		}

		internal string InternalPlaceholder {
			get {
				return Placeholder;
			}
		}

		protected sealed override object ConvertValue(object value) {
			// can not convert null or DbNull to other types
			if (value == null || value == DBNull.Value) {
				return value;
			}

			// TBD : some other way to do this?
//			if (OracleType == OracleType.Binary) {
//				return value;
//			}
			// .NET throws an exception to the user.
			object convertedValue  = value;

			// note : if we set user parameter jdbc type inside prepare interbal, the db type is not set
			if (value is IConvertible && (IsDbTypeSet || IsJdbcTypeSet)) {
				OracleType oleDbType = (IsDbTypeSet) ? OracleType : OracleConvert.JdbcTypeToOracleType((int)JdbcType);
				Type to = OracleConvert.OracleTypeToValueType(oleDbType);
				if (!(value is DateTime && to == OracleConvert.TypeOfTimespan)) //anyway will go by jdbc type
					convertedValue = Convert.ChangeType(value,to);
			}
			return convertedValue;
		}

		protected sealed override void SetParameterName(ResultSet res) {
			ParameterName = res.getString("COLUMN_NAME");
		}

		protected sealed override void SetParameterDbType(ResultSet res) {
			int jdbcType = res.getInt("DATA_TYPE");			
			// FIXME : is that correct?
			if (jdbcType == Types.OTHER) {
				string typeName = res.getString("TYPE_NAME");
				if (String.Compare("REF CURSOR", typeName, true, CultureInfo.InvariantCulture) == 0) {
					jdbcType = (int)JavaSqlTypes.CURSOR;
				}
				else if (String.Compare("BLOB",typeName,true, CultureInfo.InvariantCulture) == 0) {
					jdbcType = (int)JavaSqlTypes.BLOB;
				}
				else if (String.Compare("CLOB",typeName,true, CultureInfo.InvariantCulture) == 0) {
					jdbcType = (int)JavaSqlTypes.CLOB;
				}
				else if(String.Compare("FLOAT",typeName,true, CultureInfo.InvariantCulture) == 0) {
					jdbcType = (int)JavaSqlTypes.FLOAT;
				}
				else if(String.Compare("NVARCHAR2",typeName,true, CultureInfo.InvariantCulture) == 0) {
					jdbcType = (int)JavaSqlTypes.VARCHAR;
				}
				else if(String.Compare("NCHAR",typeName,true, CultureInfo.InvariantCulture) == 0) {
					jdbcType = (int)JavaSqlTypes.VARCHAR;
				}
			}
			OracleType = OracleConvert.JdbcTypeToOracleType(jdbcType);
			JdbcType = jdbcType;
		}

		protected sealed override void SetSpecialFeatures(ResultSet res) {
			// do nothing
		}

		protected sealed override int JdbcTypeFromProviderType() {
			return OracleConvert.OracleTypeToJdbcType(OracleType);
		}

		#endregion // Methods
    
	}
}