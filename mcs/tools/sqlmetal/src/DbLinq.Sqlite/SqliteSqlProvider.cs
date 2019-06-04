﻿#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System.Collections.Generic;
using DbLinq.Data.Linq.Sql;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Sqlite
{
#if !MONO_STRICT
    public
#endif
    class SqliteSqlProvider : SqlProvider
    {
        public override SqlStatement GetInsertIds(SqlStatement table, IList<SqlStatement> autoPKColumn, IList<SqlStatement> inputPKColumns, IList<SqlStatement> inputPKValues, IList<SqlStatement> outputColumns, IList<SqlStatement> outputParameters, IList<SqlStatement> outputExpressions)
        {
            return "SELECT last_insert_rowid()";
        }

        protected override SqlStatement GetLiteralStringLength(SqlStatement a)
        {
            return SqlStatement.Format("LENGTH({0})", a);
        }

        protected override SqlStatement GetLiteralStringToUpper(SqlStatement a)
        {
            return SqlStatement.Format("UPPER({0})", a);
        }

        protected override SqlStatement GetLiteralStringToLower(SqlStatement a)
        {
            return string.Format("LOWER({0})", a);
        }

        protected override SqlStatement GetLiteralCount(SqlStatement a)
        {
            return "COUNT(*)";
        }

        public override SqlStatement GetLiteral(bool literal)
        {
            if (literal)
                return "1";
            return "0";
        }
    }
}
