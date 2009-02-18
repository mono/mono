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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#if MONO_STRICT
namespace System.Data.Linq.Sql
#else
namespace DbLinq.Data.Linq.Sql
#endif
{
    /// <summary>
    /// An SqlStatement is a literal SQL request, composed of different parts (SqlPart)
    /// each part being either a parameter or a literal string
    /// </summary>
    [DebuggerDisplay("SqlStatement {ToString()}")]
#if MONO_STRICT
    internal
#else
    public
#endif
    class SqlStatement : IEnumerable<SqlPart>
    {
        private readonly List<SqlPart> parts = new List<SqlPart>();

        /// <summary>
        /// Empty SqlStatement, used to build new statements
        /// </summary>
        public static readonly SqlStatement Empty = new SqlStatement();

        /// <summary>
        /// Returns the number of parts present
        /// </summary>
        public int Count { get { return parts.Count; } }

        /// <summary>
        /// Enumerates all parts
        /// </summary>
        /// <returns></returns>
        public IEnumerator<SqlPart> GetEnumerator()
        {
            return parts.GetEnumerator();
        }

        /// <summary>
        /// Enumerates all parts
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns part at given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SqlPart this[int index]
        {
            get { return parts[index]; }
        }

        /// <summary>
        /// Combines all parts, in correct order
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(string.Empty, (from part in parts select part.Sql).ToArray());
        }

        /// <summary>
        /// Joins SqlStatements into a new SqlStatement
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="sqlStatements"></param>
        /// <returns></returns>
        public static SqlStatement Join(SqlStatement sqlStatement, IList<SqlStatement> sqlStatements)
        {
            // optimization: if we have only one statement to join, we return the statement itself
            if (sqlStatements.Count == 1)
                return sqlStatements[0];
            var builder = new SqlStatementBuilder();
            builder.AppendJoin(sqlStatement, sqlStatements);
            return builder.ToSqlStatement();
        }

        /// <summary>
        /// Joins SqlStatements into a new SqlStatement
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="sqlStatements"></param>
        /// <returns></returns>
        public static SqlStatement Join(SqlStatement sqlStatement, params SqlStatement[] sqlStatements)
        {
            return Join(sqlStatement, (IList<SqlStatement>)sqlStatements);
        }

        /// <summary>
        /// Formats an SqlStatement
        /// </summary>
        /// <param name="format"></param>
        /// <param name="sqlStatements"></param>
        /// <returns></returns>
        public static SqlStatement Format(string format, IList<SqlStatement> sqlStatements)
        {
            var builder = new SqlStatementBuilder();
            builder.AppendFormat(format, sqlStatements);
            return builder.ToSqlStatement();
        }

        /// <summary>
        /// Formats the specified text.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="sqlStatements">The SQL statements.</param>
        /// <returns></returns>
        public static SqlStatement Format(string format, params SqlStatement[] sqlStatements)
        {
            return Format(format, (IList<SqlStatement>)sqlStatements);
        }

        /// <summary>
        /// Replaces all text occurrences in the SqlStatement
        /// </summary>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public SqlStatement Replace(string find, string replace, bool ignoreCase)
        {
            var builder = new SqlStatementBuilder(this);
            builder.Replace(find, replace, ignoreCase);
            return builder.ToSqlStatement();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlStatement"/> class.
        /// </summary>
        public SqlStatement()
        {
        }

        /// <summary>
        /// Builds an SqlStatement by concatenating several statements
        /// </summary>
        /// <param name="sqlStatements"></param>
        public SqlStatement(IEnumerable<SqlStatement> sqlStatements)
        {
            foreach (var sqlStatement in sqlStatements)
            {
                parts.AddRange(sqlStatement.parts);
            }
        }

        /// <summary>
        /// Builds SqlStatement
        /// </summary>
        /// <param name="sqlStatements"></param>
        public SqlStatement(params SqlStatement[] sqlStatements)
            : this((IEnumerable<SqlStatement>)sqlStatements)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlStatement"/> class.
        /// </summary>
        /// <param name="sqlParts">The SQL parts.</param>
        public SqlStatement(params SqlPart[] sqlParts)
            : this((IList<SqlPart>)sqlParts)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlStatement"/> class.
        /// </summary>
        /// <param name="sqlParts">The SQL parts.</param>
        public SqlStatement(IEnumerable<SqlPart> sqlParts)
        {
            foreach (var sqlPart in sqlParts)
                SqlStatementBuilder.AddPart(parts, sqlPart);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlStatement"/> class.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        public SqlStatement(string sql)
        {
            parts.Add(new SqlLiteralPart(sql));
        }

        /// <summary>
        /// Converts a string to an SqlStatement
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static implicit operator SqlStatement(string sql)
        {
            return new SqlStatement(sql);
        }
    }
}
