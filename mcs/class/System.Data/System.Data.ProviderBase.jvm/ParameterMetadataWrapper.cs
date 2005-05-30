using System;

using java.sql;

namespace System.Data.Common
{
	public class ParameterMetadataWrapper : java.sql.ResultSetMetaData
	{
		#region Fields 

		ParameterMetaData _parameterMetaData;

		#endregion // Fields

		#region Constructors

		public ParameterMetadataWrapper(ParameterMetaData parameterMetaData)
		{
			_parameterMetaData = parameterMetaData;
		}

		#endregion // Constructors

		#region Methods

		public int getColumnCount() { throw new NotImplementedException(); }

		public int getColumnDisplaySize(int i) { throw new NotImplementedException(); }

		public int getColumnType(int i) { throw new NotImplementedException(); }

		public int getPrecision(int i) { throw new NotImplementedException(); }

		public int getScale(int i) { throw new NotImplementedException(); }

		public int isNullable(int i) { throw new NotImplementedException(); }

		public bool isAutoIncrement(int i) { throw new NotImplementedException(); }

		public bool isCaseSensitive(int i) { throw new NotImplementedException(); }

		public bool isCurrency(int i) { throw new NotImplementedException(); }

		public bool isDefinitelyWritable(int i) { throw new NotImplementedException(); }

		public bool isReadOnly(int i) { throw new NotImplementedException(); }

		public bool isSearchable(int i) { throw new NotImplementedException(); }

		public bool isSigned(int i) { throw new NotImplementedException(); }

		public bool isWritable(int i) { throw new NotImplementedException(); }

		public String getCatalogName(int i) { throw new NotImplementedException(); }

		public String getColumnClassName(int i) { throw new NotImplementedException(); }

		public String getColumnLabel(int i) { throw new NotImplementedException(); }

		public String getColumnName(int i) { throw new NotImplementedException(); }

		public String getColumnTypeName(int i) { return _parameterMetaData.getParameterTypeName(i); }

		public String getSchemaName(int i) { throw new NotImplementedException(); }

		public String getTableName(int i) { throw new NotImplementedException(); }

		#endregion // Methods
	}
}
