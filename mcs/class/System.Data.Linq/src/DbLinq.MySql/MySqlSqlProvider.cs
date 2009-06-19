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

namespace DbLinq.MySql
{
#if !MONO_STRICT
    public
#endif
    class MySqlSqlProvider : SqlProvider
    {
        public override string GetParameterName(string nameBase)
        {
            return string.Format("?{0}", nameBase);
        }

        protected override SqlStatement GetLiteralCount(SqlStatement a)
        {
            return "COUNT(*)";
        }

        protected override SqlStatement GetLiteralStringConcat(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("CONCAT({0}, {1})", a, b);
        }

        public virtual string GetBulkInsert(string table, IList<string> columns, IList<IList<string>> valuesLists)
        {
            if (columns.Count == 0)
                return string.Empty;

            var insertBuilder = new StringBuilder("INSERT INTO ");
            insertBuilder.Append(table);
            insertBuilder.AppendFormat(" ({0})", string.Join(", ", columns.ToArray()));
            insertBuilder.Append(" VALUES ");
            var literalValuesLists = new List<string>();
            foreach (var values in valuesLists)
                literalValuesLists.Add(string.Format("({0})", string.Join(", ", values.ToArray())));
            insertBuilder.Append(string.Join(", ", literalValuesLists.ToArray()));
            return insertBuilder.ToString();
        }

        protected override char SafeNameStartQuote { get { return '`'; } }
        protected override char SafeNameEndQuote { get { return '`'; } }

        /// <summary>
        /// MySQL is case insensitive, and names always specify a case (there is no default casing)
        /// However, tables appear to be full lowercase
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        protected override bool IsNameCaseSafe(string dbName)
        {
            return true;
        }
    }
}
