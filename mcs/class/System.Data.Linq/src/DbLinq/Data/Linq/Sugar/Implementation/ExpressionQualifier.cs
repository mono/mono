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
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    internal class ExpressionQualifier : IExpressionQualifier
    {
        /// <summary>
        /// Returns Expression precedence. Higher value means lower precedence.
        /// http://en.csharp-online.net/ECMA-334:_14.2.1_Operator_precedence_and_associativity
        /// We added the Clase precedence, which is the lowest
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ExpressionPrecedence GetPrecedence(Expression expression)
        {
            if (expression is SpecialExpression)
            {
                var specialNodeType = ((SpecialExpression)expression).SpecialNodeType;
                switch (specialNodeType) // SETuse
                {
                case SpecialExpressionType.IsNull:
                case SpecialExpressionType.IsNotNull:
                    return ExpressionPrecedence.Equality;
                case SpecialExpressionType.Concat:
                    return ExpressionPrecedence.Additive;
                case SpecialExpressionType.Like:
                    return ExpressionPrecedence.Equality;
                // the following are methods
                case SpecialExpressionType.Min:
                case SpecialExpressionType.Max:
                case SpecialExpressionType.Sum:
                case SpecialExpressionType.Average:
                case SpecialExpressionType.Count:
                case SpecialExpressionType.StringLength:
                case SpecialExpressionType.ToUpper:
                case SpecialExpressionType.ToLower:
                case SpecialExpressionType.Substring:
                case SpecialExpressionType.Trim:
                case SpecialExpressionType.LTrim:
                case SpecialExpressionType.RTrim:
                case SpecialExpressionType.StringInsert:
                case SpecialExpressionType.Replace:
                case SpecialExpressionType.Remove:
                case SpecialExpressionType.IndexOf:
                case SpecialExpressionType.Year:
                case SpecialExpressionType.Month:
                case SpecialExpressionType.Day:
                case SpecialExpressionType.Hour:
                case SpecialExpressionType.Minute:
                case SpecialExpressionType.Second:
                case SpecialExpressionType.Millisecond:
                case SpecialExpressionType.Now:
                case SpecialExpressionType.DateDiffInMilliseconds:
                case SpecialExpressionType.Abs:
                case SpecialExpressionType.Exp:
                case SpecialExpressionType.Floor:
                case SpecialExpressionType.Ln:
                case SpecialExpressionType.Log:
                case SpecialExpressionType.Pow:
                case SpecialExpressionType.Round:
                case SpecialExpressionType.Sign:
                case SpecialExpressionType.Sqrt:
                    return ExpressionPrecedence.Primary;
                case SpecialExpressionType.In:
                    return ExpressionPrecedence.Equality; // not sure for this one
                default:
                    throw Error.BadArgument("S0050: Unhandled SpecialExpressionType {0}", specialNodeType);
                }
            }
            if (expression is SelectExpression)
                return ExpressionPrecedence.Clause;
            switch (expression.NodeType)
            {
            case ExpressionType.Add:
            case ExpressionType.AddChecked:
                return ExpressionPrecedence.Additive;
            case ExpressionType.And:
            case ExpressionType.AndAlso:
                return ExpressionPrecedence.ConditionalAnd;
            case ExpressionType.ArrayLength:
            case ExpressionType.ArrayIndex:
            case ExpressionType.Call:
                return ExpressionPrecedence.Primary;
            case ExpressionType.Coalesce:
                return ExpressionPrecedence.NullCoalescing;
            case ExpressionType.Conditional:
                return ExpressionPrecedence.Conditional;
            case ExpressionType.Constant:
                return ExpressionPrecedence.Primary;
            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
                return ExpressionPrecedence.Primary;
            case ExpressionType.Divide:
                return ExpressionPrecedence.Multiplicative;
            case ExpressionType.Equal:
                return ExpressionPrecedence.Equality;
            case ExpressionType.ExclusiveOr:
                return ExpressionPrecedence.LogicalXor;
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
                return ExpressionPrecedence.RelationalAndTypeTest;
            case ExpressionType.Invoke:
                return ExpressionPrecedence.Primary;
            case ExpressionType.Lambda:
                return ExpressionPrecedence.Primary;
            case ExpressionType.LeftShift:
                return ExpressionPrecedence.Shift;
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
                return ExpressionPrecedence.RelationalAndTypeTest;
            case ExpressionType.ListInit:
            case ExpressionType.MemberAccess:
            case ExpressionType.MemberInit:
                return ExpressionPrecedence.Primary;
            case ExpressionType.Modulo:
            case ExpressionType.Multiply:
            case ExpressionType.MultiplyChecked:
                return ExpressionPrecedence.Multiplicative;
            case ExpressionType.Negate:
            case ExpressionType.UnaryPlus:
            case ExpressionType.NegateChecked:
                return ExpressionPrecedence.Unary;
            case ExpressionType.New:
            case ExpressionType.NewArrayInit:
            case ExpressionType.NewArrayBounds:
                return ExpressionPrecedence.Primary;
            case ExpressionType.Not:
                return ExpressionPrecedence.Unary;
            case ExpressionType.NotEqual:
                return ExpressionPrecedence.Equality;
            case ExpressionType.Or:
            case ExpressionType.OrElse:
                return ExpressionPrecedence.ConditionalOr;
            case ExpressionType.Parameter:
                return ExpressionPrecedence.Primary;
            case ExpressionType.Power:
                return ExpressionPrecedence.Primary;
            case ExpressionType.Quote:
                return ExpressionPrecedence.Primary;
            case ExpressionType.RightShift:
                return ExpressionPrecedence.Shift;
            case ExpressionType.Subtract:
            case ExpressionType.SubtractChecked:
                return ExpressionPrecedence.Additive;
            case ExpressionType.TypeAs:
            case ExpressionType.TypeIs:
                return ExpressionPrecedence.RelationalAndTypeTest;
            }
            return ExpressionPrecedence.Primary;
        }

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
        public ExpressionTier GetTier(Expression expression)
        {
            if (expression is GroupExpression)
                return ExpressionTier.Clr;
            if (expression is SelectExpression)
                return ExpressionTier.Sql;
            if (expression is ColumnExpression)
                return ExpressionTier.Sql;
            if (expression is TableExpression)
                return ExpressionTier.Sql;
            if (expression is EntitySetExpression)
                return ExpressionTier.Sql;
            if (expression is InputParameterExpression)
                return ExpressionTier.Sql;
            if (expression is SpecialExpression)
            {
                var specialExpressionType = ((SpecialExpression)expression).SpecialNodeType;
                switch (specialExpressionType) // SETuse
                {
                case SpecialExpressionType.IsNull:
                case SpecialExpressionType.IsNotNull:
                case SpecialExpressionType.Concat:
                case SpecialExpressionType.StringLength:
                case SpecialExpressionType.ToUpper:
                case SpecialExpressionType.ToLower:
                case SpecialExpressionType.Substring:
                case SpecialExpressionType.Trim:
                case SpecialExpressionType.LTrim:
                case SpecialExpressionType.RTrim:
                case SpecialExpressionType.StringInsert:
                case SpecialExpressionType.Replace:
                case SpecialExpressionType.Remove:
                case SpecialExpressionType.IndexOf:
                case SpecialExpressionType.Year:
                case SpecialExpressionType.Month:
                case SpecialExpressionType.Day:
                case SpecialExpressionType.Hour:
                case SpecialExpressionType.Minute:
                case SpecialExpressionType.Second:
                case SpecialExpressionType.Millisecond:
                case SpecialExpressionType.Now:
                case SpecialExpressionType.DateDiffInMilliseconds:
                case SpecialExpressionType.Abs:
                case SpecialExpressionType.Exp:
                case SpecialExpressionType.Floor:
                case SpecialExpressionType.Ln:
                case SpecialExpressionType.Log:
                case SpecialExpressionType.Pow:
                case SpecialExpressionType.Round:
                case SpecialExpressionType.Sign:
                case SpecialExpressionType.Sqrt:
                    return ExpressionTier.Any;

                case SpecialExpressionType.Like:
                case SpecialExpressionType.Min:
                case SpecialExpressionType.Max:
                case SpecialExpressionType.Sum:
                case SpecialExpressionType.Average:
                case SpecialExpressionType.Count:
                case SpecialExpressionType.In:
                    return ExpressionTier.Sql; // don't tell anyone, but we can do it on both tiers, anyway this is significantly faster/efficient in SQL anyway
                default:
                    throw Error.BadArgument("S0157: Unhandled node type {0}", specialExpressionType);
                }
            }
            switch (expression.NodeType)
            {
            case ExpressionType.ArrayLength:
            case ExpressionType.ArrayIndex:
            case ExpressionType.Call:
            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
            case ExpressionType.Invoke:
            case ExpressionType.Lambda:
            case ExpressionType.ListInit:
            case ExpressionType.MemberAccess:
            case ExpressionType.MemberInit:
            case ExpressionType.New:
            case ExpressionType.NewArrayInit:
            case ExpressionType.NewArrayBounds:
            case ExpressionType.Parameter:
            case ExpressionType.SubtractChecked:
            case ExpressionType.TypeAs:
            case ExpressionType.TypeIs:
                return ExpressionTier.Clr;
            default:
                return ExpressionTier.Any;
            }
        }
    }
}