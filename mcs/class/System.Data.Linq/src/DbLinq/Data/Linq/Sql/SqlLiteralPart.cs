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

using System.Diagnostics;

#if MONO_STRICT
namespace System.Data.Linq.Sql
#else
namespace DbLinq.Data.Linq.Sql
#endif
{
    /// <summary>
    /// Represents a literal SQL part
    /// </summary>
    [DebuggerDisplay("SqlLiteralPart {Literal}")]
#if MONO_STRICT
    internal
#else
    public
#endif
    class SqlLiteralPart : SqlPart
    {
        /// <summary>
        /// The resulting SQL string
        /// </summary>
        /// <value></value>
        public override string Sql { get { return Literal; } }

        /// <summary>
        /// Literal SQL used as is
        /// </summary>
        public string Literal { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlLiteralPart"/> class.
        /// </summary>
        /// <param name="literal">The literal.</param>
        public SqlLiteralPart(string literal)
        {
            Literal = literal;
        }

        /// <summary>
        /// Creates a SqlLiteralPart from a given string (implicit)
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        public static implicit operator SqlLiteralPart(string literal)
        {
            return new SqlLiteralPart(literal);
        }
    }
}
