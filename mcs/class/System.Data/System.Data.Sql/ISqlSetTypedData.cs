//
// System.Data.Sql.ISqlSetTypedData
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Data.SqlTypes;

namespace System.Data.Sql {
	public interface ISqlSetTypedData : ISetTypedData
	{
		#region Methods

		void SetSqlBinary (int i, SqlBinary value);
		void SetSqlBoolean (int i, SqlBoolean value);
		void SetSqlByte (int i, SqlByte value);
		void SetSqlBytes (int i, SqlBytes buffer);
		void SetSqlBytesRef (int i, SqlBytes value);
		void SetSqlChars (int i, SqlChars value);
		void SetSqlCharsRef (int i, SqlChars buffer);
		void SetSqlDate (int i, SqlDate value);
		void SetSqlDateTime (int i, SqlDateTime value);
		void SetSqlDecimal (int i, SqlDecimal value);
		void SetSqlDouble (int i, SqlDouble value);
		void SetSqlGuid (int i, SqlGuid value);
		void SetSqlInt16 (int i, SqlInt16 value);
		void SetSqlInt32 (int i, SqlInt32 value);
		void SetSqlInt64 (int i, SqlInt64 value);
		void SetSqlMoney (int i, SqlMoney value);
		void SetSqlSingle (int i, SqlSingle value);
		void SetSqlString (int i, SqlString value);
		void SetSqlTime (int i, SqlTime value);
		void SetSqlUtcDateTime (int i, SqlUtcDateTime value);
		void SetSqlXmlReader (int i, SqlXmlReader value);

		#endregion // Methods
	}
}

#endif
