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
using System.Linq.Expressions;
using System.Reflection;
#if MONO_STRICT
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar.Expressions;
#endif
using DbLinq.Util;

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Expressions
#else
namespace DbLinq.Data.Linq.Sugar.Expressions
#endif
{
    /// <summary>
    /// Describes a column, related to a table
    /// </summary>
    [DebuggerDisplay("ColumnExpression {Table.Name} (as {Table.Alias}).{Name}")]
#if !MONO_STRICT
    public
#endif
    class ColumnExpression : MutableExpression
    {
        public const ExpressionType ExpressionType = (ExpressionType)CustomExpressionType.Column;

        public TableExpression Table { get; private set; }
        public string Name { get; private set; }
        public MemberInfo MemberInfo { get; private set; }

        public string Alias { get; set; }

        public int RequestIndex { get; set; }

        public ColumnExpression(TableExpression table, string name, MemberInfo memberInfo)
            : base(ExpressionType, memberInfo.GetMemberType())
        {
            Table = table;
            Name = name;
            MemberInfo = memberInfo;
            RequestIndex = -1; // unused
        }
    }
}