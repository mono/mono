//
// System.Data.Sql.ISqlGetTypedData
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
using System.Xml;

namespace System.Data.Sql {
	public interface ISqlGetTypedData : IGetTypedData
	{
		#region Methods

		SqlBinary GetSqlBinary (int i);
		SqlBoolean GetSqlBoolean (int i);
		SqlByte GetSqlByte (int i);
		SqlBytes GetSqlBytes (int i);
		SqlBytes GetSqlBytesRef (int i);
		SqlChars GetSqlChars (int i);
		SqlChars GetSqlCharsRef (int i);
		SqlDate GetSqlDate (int i);
		SqlDateTime GetSqlDateTime (int i);
		SqlDecimal GetSqlDecimal (int i);
		SqlDouble GetSqlDouble (int i);
		SqlGuid GetSqlGuid (int i);
		SqlInt16 GetSqlInt16 (int i);
		SqlInt32 GetSqlInt32 (int i);
		SqlInt64 GetSqlInt64 (int i);
		SqlMetaData GetSqlMetaData (int i);
		SqlMoney GetSqlMoney (int i);
		SqlSingle GetSqlSingle (int i);
		SqlString GetSqlString (int i);
		SqlTime GetSqlTime (int i);
		SqlUtcDateTime GetSqlUtcDateTime (int i);
		object GetSqlValue (int i);
		SqlXmlReader GetSqlXmlReader (int i);

		#endregion // Methods
	}
}

#endif
