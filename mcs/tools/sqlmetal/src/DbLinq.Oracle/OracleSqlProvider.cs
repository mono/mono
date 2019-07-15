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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbLinq.Data.Linq.Sql;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Oracle
{
#if !MONO_STRICT
    public
#endif
    class OracleSqlProvider : SqlProvider
    {
        //public override string  GetInsert(string table, IList<string> inputColumns, IList<string> inputValues)
        //{
        //     return "BEGIN " + base.GetInsert(table, inputColumns, inputValues);
        //}

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

        protected override SqlStatement GetLiteralModulo(SqlStatement a, SqlStatement b)
        {
            return string.Format("MOD({0}, {1})", a, b);
        }

        protected override SqlStatement GetLiteralCount(SqlStatement a)
        {
            return "COUNT(*)";
        }

        protected override SqlStatement GetLiteralStringLength(SqlStatement a)
        {
            return SqlStatement.Format("LENGTH({0})", a);
        }

        protected override SqlStatement GetLiteralStringToLower(SqlStatement a)
        {
            return SqlStatement.Format("LOWER({0})", a);
        }

        protected override SqlStatement GetLiteralStringToUpper(SqlStatement a)
        {
            return SqlStatement.Format("UPPER({0})", a);
        }

        //          SELECT * FROM (
        //          SELECT a.*, rownum RN FROM (
        //          select c$.CustomerID, c$.CompanyName from [Customers] c$ order by c$.CUSTOMERID
        //          ) a  WHERE rownum <=5
        //          ) WHERE rn >2

        protected const string LimitedTableName = "LimitedTable___";
        protected const string LimitedRownum = "Limit___";

        public override SqlStatement GetLiteralLimit(SqlStatement select, SqlStatement limit)
        {
            return SqlStatement.Format(
                @"SELECT {2}.*, rownum {3} FROM ({4}{0}{4}) {2} WHERE rownum <= {1}",
                select, limit, LimitedTableName, LimitedRownum, NewLine);
        }

        public override SqlStatement GetLiteralLimit(SqlStatement select, SqlStatement limit, SqlStatement offset, SqlStatement offsetAndLimit)
        {
            return SqlStatement.Format(
                @"SELECT * FROM ({3}{0}{3}) WHERE {2} > {1}",
                GetLiteralLimit(select, offsetAndLimit), offset, LimitedRownum, NewLine);
        }

        protected override SqlStatement GetLiteralStringIndexOf(SqlStatement baseString, SqlStatement searchString, SqlStatement startIndex, SqlStatement count)
        {
            // SUBSTR(baseString, StartIndex)
            var substring = GetLiteralSubString(baseString, startIndex, count);

            // INSTR(SUBSTR(baseString, StartIndex), searchString) ---> range 1:n , 0 => doesn't exist
            return SqlStatement.Format("INSTR({0},{1})", substring, searchString);
        }

        protected override SqlStatement GetLiteralStringIndexOf(SqlStatement baseString, SqlStatement searchString, SqlStatement startIndex)
        {
            // SUBSTR(baseString,StartIndex)
            var substring = GetLiteralSubString(baseString, startIndex);

            // INSTR(SUBSTR(baseString, StartIndex), searchString) ---> range 1:n , 0 => doesn't exist
            return SqlStatement.Format("INSTR({0},{1})", substring, searchString);
        }

        protected override SqlStatement GetLiteralStringIndexOf(SqlStatement baseString, SqlStatement searchString)
        {
            return GetLiteralSubtract(SqlStatement.Format("INSTR({0},{1})", baseString, searchString), "1");
        }

    }
}
