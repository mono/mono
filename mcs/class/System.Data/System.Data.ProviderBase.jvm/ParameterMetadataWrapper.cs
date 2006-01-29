//
// System.Data.ProviderBase.ParameterMetaDataWrapper
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

using java.sql;

namespace System.Data.ProviderBase
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
