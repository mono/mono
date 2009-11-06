#region MIT license
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

using System;
using System.Linq;
using System.Collections.Generic;

using DbLinq.Vendor.Implementation;

using DbLinq.Data.Linq.Sql;
using DbLinq.Data.Linq.Sugar.Expressions;


namespace DbLinq.Firebird
{
#if !MONO_STRICT
    public
#endif

    class FirebirdSqlProvider : SqlProvider
    {
        public override ExpressionTranslator GetTranslator()
        {
            return new FirebirdExpressionTranslator();
        }

        public override string GetParameterName(string nameBase)
        {
            return "@" + nameBase;
        }

        protected override SqlStatement GetLiteralCount(SqlStatement a)
        {
            return "COUNT(*)";
        }

        protected override SqlStatement GetLiteralStringConcat(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} || {1}", a, b);
        }

        protected override SqlStatement GetLiteralStringToLower(SqlStatement a)
        {
            return string.Format("LOWER({0})", a);
        }

        protected override SqlStatement GetLiteralStringToUpper(SqlStatement a)
        {
            return string.Format("UPPER({0})", a);
        }

        protected override char SafeNameStartQuote { get { return ' '; } }
        protected override char SafeNameEndQuote { get { return ' '; } }

        /// <summary>
        /// MySQL is case insensitive, and names always specify a case (there is no default casing)
        /// However, tables appear to be full lowercase
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        protected override bool IsNameCaseSafe(string dbName)
        {
            return dbName == dbName.ToUpperInvariant();
        }

        /// <summary>
        /// Returns a table alias
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public override string GetTable(string table)
        {
            var parts = table.Split('.');
            return GetSafeName(parts[parts.Length - 1]);
        }

        /// <summary>
        /// Returns a LIMIT clause around a SELECT clause
        /// </summary>
        /// <param name="select">SELECT clause</param>
        /// <param name="limit">limit value (number of columns to be returned)</param>
        /// <returns></returns>
        public override SqlStatement GetLiteralLimit(SqlStatement select, SqlStatement limit)
        {
            string stmt = limit.Count == 2
                ? string.Format("SELECT FIRST {0}, LAST {1}", limit[0].Sql, limit[1].Sql)
                : string.Format("SELECT FIRST {0}", limit[0].Sql);
            return select.Replace("SELECT", stmt, true);
        }

        public override SqlStatement GetLiteralLimit(SqlStatement select, SqlStatement limit, SqlStatement offset, SqlStatement offsetAndLimit)
        {
            string stmt = (limit.Count == 2
                ? string.Format("SELECT FIRST {0}, LAST {1} SKIP {2}", limit[0].Sql, limit[1].Sql, offset)
                : string.Format("SELECT FIRST {0} SKIP {1}", limit[0].Sql, offset));
           //string stmt = string.Format("SELECT FIRST {0} SKIP {1}", limit, offset);
           
            return select.Replace("SELECT", stmt, true);
        }


        public override SqlStatement GetInsertIds(SqlStatement table, IList<SqlStatement> autoPKColumn, IList<SqlStatement> inputPKColumns, IList<SqlStatement> inputPKValues, IList<SqlStatement> outputColumns, IList<SqlStatement> outputParameters, IList<SqlStatement> outputExpressions)
        {
            // no parameters? no need to get them back
            if (outputParameters.Count == 0)
                return "";
            // otherwise we keep track of the new values
            return SqlStatement.Format("SELECT {0} INTO {1} FROM DUAL",
                SqlStatement.Join(", ", (from outputExpression in outputExpressions select outputExpression.Replace(".NextVal", ".CurrVal", true)).ToArray()),
                SqlStatement.Join(", ", outputParameters.ToArray()));
        }
    }
}
