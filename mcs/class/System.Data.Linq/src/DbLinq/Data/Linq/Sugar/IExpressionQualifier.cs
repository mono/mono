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

using System.Linq.Expressions;

#if MONO_STRICT
namespace System.Data.Linq.Sugar
#else
namespace DbLinq.Data.Linq.Sugar
#endif
{
    internal interface IExpressionQualifier
    {
        /// <summary>
        /// Returns Expression precedence. Higher value means lower precedence.
        /// http://en.csharp-online.net/ECMA-334:_14.2.1_Operator_precedence_and_associativity
        /// We added the Clase precedence, which is the lowest
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        ExpressionPrecedence GetPrecedence(Expression expression);

        /// <summary>
        /// Determines wether an expression can run in Clr or Sql
        /// A request is valid is it starts with Clr only, followed by Any and ends (at bottom) with Sql.
        /// With this, we can:
        /// - Find the first point cut from Clr to Any
        /// - Find the second point cut from Any to Sql
        /// Select a strategy to load more or less the Clr or Sql engine
        /// This is used only for SELECT clause
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        ExpressionTier GetTier(Expression expression);
    }
}