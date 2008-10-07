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
using System.Data.Linq.Sugar;
using System.Data.Linq.Sugar.ExpressionMutator;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    /// <summary>
    /// Analyzes Expressions before translation to SQL, to help the translator
    /// "Prequel" stands, of course, for "PreSQL"
    /// </summary>
    internal class PrequelAnalyzer : IPrequelAnalyzer
    {
        /// <summary>
        /// Translates some generic CLR patterns to specific preSQL patterns
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual Expression Analyze(Expression expression, BuilderContext builderContext)
        {
            return expression.Recurse(e => AnalyzeExpression(e, builderContext));
        }

        protected virtual Expression AnalyzeExpression(Expression expression, BuilderContext builderContext)
        {
            // string Add --> Concat
            var binaryExpression = expression as BinaryExpression;
            if (expression.NodeType == ExpressionType.Add
                && binaryExpression != null && typeof(string).IsAssignableFrom(binaryExpression.Left.Type))
            {
                return new SpecialExpression(SpecialExpressionType.Concat, binaryExpression.Left, binaryExpression.Right);
            }
            return expression;
        }
    }
}