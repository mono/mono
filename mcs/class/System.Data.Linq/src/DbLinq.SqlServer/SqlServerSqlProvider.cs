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

#if MONO_STRICT
using System.Data.Linq.Sql;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sql;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

namespace DbLinq.SqlServer
{
#if MONO_STRICT
    internal
#else
    public
#endif
 class SqlServerSqlProvider : SqlProvider
    {
        protected override char SafeNameStartQuote { get { return '['; } }
        protected override char SafeNameEndQuote { get { return ']'; } }

        /// <summary>
        /// Returns a table alias
        /// Ensures about the right case
        /// </summary>
        /// <param name="table"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public override string GetTableAsAlias(string table, string alias)
        {
            return string.Format("{0} AS {1}", GetTable(table), GetTableAlias(alias));
        }

        public override string GetParameterName(string nameBase)
        {
            return string.Format("@{0}", nameBase);
        }

        public override SqlStatement GetLiteralLimit(SqlStatement select, SqlStatement limit)
        {
            var trimSelect = "SELECT ";
            if (select.Count > 0 && select[0].Sql.StartsWith(trimSelect))
            {
                var selectBuilder = new SqlStatementBuilder(select);
                var remaining = select[0].Sql.Substring(trimSelect.Length);
                selectBuilder.Parts[0] = new SqlLiteralPart(remaining);
                return SqlStatement.Format("SELECT TOP ({0}) {1}", limit, selectBuilder.ToSqlStatement());
            }
            throw new ArgumentException("S0051: Unknown select format");
        }

        protected override SqlStatement GetLiteralDateDiff(SqlStatement dateA, SqlStatement dateB)
        {
            return SqlStatement.Format("(CONVERT(BigInt,DATEDIFF(DAY, {0}, {1}))) * 86400000 +" //diffierence in milliseconds regards days
                     + "DATEDIFF(MILLISECOND, "

                                // (DateA-DateB) in days +DateB = difference in time
                                + @"DATEADD(DAY, 
                                      DATEDIFF(DAY, {0}, {1})
                                      ,{0})"

                                + ",{1})", dateB, dateA);

            //this trick is needed in sqlserver since DATEDIFF(MILLISECONDS,{0},{1}) usually crhases in the database engine due an overflow:
            //System.Data.SqlClient.SqlException : Difference of two datetime columns caused overflow at runtime.
        }

        protected override SqlStatement GetLiteralDateTimePart(SqlStatement dateExpression, SpecialExpressionType operationType)
        {
            return SqlStatement.Format("DATEPART({0},{1})", operationType.ToString().ToUpper(), dateExpression);
        }

        protected override SqlStatement GetLiteralMathPow(SqlStatement p, SqlStatement p_2)
        {
            return SqlStatement.Format("POWER({0},{1})", p, p_2);
        }

        protected override SqlStatement GetLiteralMathLog(SqlStatement p, SqlStatement p_2)
        {
            return SqlStatement.Format("(LOG({0})/LOG({1}))", p, p_2);
        }

        protected override SqlStatement GetLiteralMathLn(SqlStatement p)
        {
            return GetLiteralMathLog(p, string.Format("{0}", Math.E));
        }

        protected override SqlStatement GetLiteralStringLength(SqlStatement a)
        {
            return SqlStatement.Format("LEN({0})", a);
        }

        protected override SqlStatement GetLiteralSubString(SqlStatement baseString, SqlStatement startIndex, SqlStatement count)
        {
            //in standard sql base string index is 1 instead 0
            return SqlStatement.Format("SUBSTRING({0}, {1}, {2})", baseString, startIndex, count);
        }

        protected override SqlStatement GetLiteralSubString(SqlStatement baseString, SqlStatement startIndex)
        {
            return GetLiteralSubString(baseString, startIndex, GetLiteralStringLength(baseString));
        }

        protected override SqlStatement GetLiteralTrim(SqlStatement a)
        {
            return SqlStatement.Format("RTRIM(LTRIM({0}))", a);
        }

        protected override SqlStatement GetLiteralStringConcat(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} + {1}", a, b);
        }

        protected override SqlStatement GetLiteralStringToLower(SqlStatement a)
        {
            return SqlStatement.Format("LOWER({0})", a);
        }

        protected override SqlStatement GetLiteralStringToUpper(SqlStatement a)
        {
            return SqlStatement.Format("UPPER({0})", a);
        }

        protected override SqlStatement GetLiteralStringIndexOf(SqlStatement baseString, SqlStatement searchString)
        {
            return GetLiteralSubtract(SqlStatement.Format("CHARINDEX({0},{1})", searchString, baseString), "1");
        }

        protected override SqlStatement GetLiteralStringIndexOf(SqlStatement baseString, SqlStatement searchString, SqlStatement startIndex)
        {
            return GetLiteralSubtract(SqlStatement.Format("CHARINDEX({0},{1},{2})", searchString, baseString, startIndex), "1");
        }

        protected override SqlStatement GetLiteralStringIndexOf(SqlStatement baseString, SqlStatement searchString, SqlStatement startIndex, SqlStatement count)
        {
            return GetLiteralSubtract(SqlStatement.Format("CHARINDEX({0},{1},{2})", searchString, GetLiteralSubString(baseString, "1", GetLiteralStringConcat(count, startIndex)), startIndex), "1");
        }

        //http://msdn.microsoft.com/en-us/library/4e5xt97a(VS.71).aspx
        public static readonly Dictionary<Type, string> typeMapping = new Dictionary<Type, string>
        {
            {typeof(int),"int"},
            {typeof(uint),"int"},

            {typeof(long),"bigint"},
            {typeof(ulong),"bigint"},

            {typeof(float),"float"}, //TODO: could be float or real. check ranges.
            {typeof(double),"float"}, //TODO: could be float or real. check ranges.
            
            {typeof(decimal),"numeric"},

            {typeof(short),"tinyint"},
            {typeof(ushort),"tinyint"},

            {typeof(bool),"bit"},

            // trunk? They could be: varchar, char,nchar, ntext,text... it should be the most flexible string type. TODO: check wich of them is better.
            {typeof(string),"varchar"}, 
            {typeof(char[]),"varchar"},

            {typeof(char),"char"},

            {typeof(DateTime),"datetime"},
            {typeof(Guid),"uniqueidentifier"}

            // there are more types: timestamps, images ... TODO: check what is the official behaviour
        };

        public override SqlStatement GetLiteralConvert(SqlStatement a, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments().First();

            SqlStatement sqlTypeName;
            if (typeMapping.ContainsKey(type))
                sqlTypeName = typeMapping[type];
            else
                sqlTypeName = "variant";

            return SqlStatement.Format("CONVERT({0},{1})", sqlTypeName, a);
        }

        public override string GetColumn(string table, string column)
        {
            if (column != "*")
                return base.GetColumn(table, column);
            return "*";
        }
    }
}
