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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;

namespace DbLinq.Data.Linq.Sugar.Implementation
{
    internal class SpecialExpressionTranslator : ISpecialExpressionTranslator
    {
        /// <summary>
        /// Translate a hierarchy's SpecialExpressions to Expressions
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Expression Translate(Expression expression)
        {
            return expression.Recurse(Analyzer);
        }

        protected virtual Expression Analyzer(Expression expression)
        {
            if (expression is SpecialExpression)
                return Translate((SpecialExpression)expression);
            else if (expression is StartIndexOffsetExpression)
                return Translate(((StartIndexOffsetExpression)expression).InnerExpression);
            return expression;
        }

        /// <summary>
        /// Translates a SpecialExpression to standard Expression equivalent
        /// </summary>
        /// <param name="specialExpression"></param>
        /// <returns></returns>
        protected virtual Expression Translate(SpecialExpression specialExpression)
        {
            var operands = specialExpression.Operands.ToList();
            switch (specialExpression.SpecialNodeType)  // SETuse
            {
                case SpecialExpressionType.IsNull:
                    return TranslateIsNull(operands);
                case SpecialExpressionType.IsNotNull:
                    return TranslateIsNotNull(operands);
                case SpecialExpressionType.Concat:
                    return TranslateConcat(operands);
                //case SpecialExpressionType.Count:
                //    break;
                //case SpecialExpressionType.Like:
                //    break;
                //case SpecialExpressionType.Min:
                //    break;
                //case SpecialExpressionType.Max:
                //    break;
                //case SpecialExpressionType.Sum:
                //    break;
                //case SpecialExpressionType.Average:
                //    break;
                case SpecialExpressionType.StringLength:
                    return TranslateStringLength(operands);
                case SpecialExpressionType.ToUpper:
                    return GetStandardCallInvoke("ToUpper", operands);
                case SpecialExpressionType.ToLower:
                    return GetStandardCallInvoke("ToLower", operands);
                //case SpecialExpressionType.In:
                //    break;

                case SpecialExpressionType.StringInsert:
                    return GetStandardCallInvoke("Insert", operands);
                case SpecialExpressionType.Substring:
                case SpecialExpressionType.Trim:
                case SpecialExpressionType.LTrim:
                case SpecialExpressionType.RTrim:
                case SpecialExpressionType.Replace:
                case SpecialExpressionType.Remove:
                case SpecialExpressionType.IndexOf:
                case SpecialExpressionType.Year:
                case SpecialExpressionType.Month:
                case SpecialExpressionType.Day:
                case SpecialExpressionType.Hour:
                case SpecialExpressionType.Minute:
                case SpecialExpressionType.Millisecond:
                case SpecialExpressionType.Date:
                    return GetStandardCallInvoke(specialExpression.SpecialNodeType.ToString(), operands);
                case SpecialExpressionType.Now:
                    return GetDateTimeNowCall(operands);
                case SpecialExpressionType.DateDiffInMilliseconds:
                    return GetCallDateDiffInMilliseconds(operands);
                default:
                    throw Error.BadArgument("S0078: Implement translator for {0}", specialExpression.SpecialNodeType);

            }
        }

        private Expression GetCallDateDiffInMilliseconds(List<Expression> operands)
        {
            return Expression.MakeMemberAccess(Expression.Subtract(operands.First(), operands.ElementAt(1)),
                                                typeof(TimeSpan).GetProperty("TotalMilliseconds"));
        }

        private Expression GetDateTimeNowCall(List<Expression> operands)
        {
            return Expression.Call(typeof(DateTime).GetProperty("Now").GetGetMethod());
        }

        private Expression TranslateStringLength(List<Expression> operands)
        {
            return Expression.MakeMemberAccess(operands[0], typeof(string).GetProperty("Length"));
        }

        protected virtual Expression GetStandardCallInvoke(string methodName, List<Expression> operands)
        {
            var parametersExpressions = operands.Skip(1);
            return Expression.Call(operands[0],
                                   operands[0].Type.GetMethod(methodName, parametersExpressions.Select(op => op.Type).ToArray()),
                                   parametersExpressions);
        }

        //protected virtual Expression TranslateRemove(List<Expression> operands)
        //{
        //    if (operands.Count > 2)
        //    {
        //        return Expression.Call(operands[0], 
        //                            typeof(string).GetMethod("Remove", new[] { typeof(int), typeof(int) }), 
        //                            operands[1], operands[2]);
        //    }
        //    return Expression.Call(operands[0], 
        //                            typeof(string).GetMethod("Remove", new[] { typeof(int) }),
        //                            operands[1]);
        //}

        //protected virtual Expression TranslateStringIndexOf(List<Expression> operands)
        //{
        //    if (operands.Count == 2 && operands[1].Type == typeof(string))
        //    {
        //         return Expression.Call(operands[0], 
        //                            typeof(string).GetMethod("IndexOf", new[] { typeof(string)}), 
        //                            operands[1]);
        //    }
        //    throw new NotSupportedException();
        //}

        //protected virtual Expression TranslateReplace(List<Expression> operands)
        //{
        //    if (operands.ElementAt(1).Type == typeof(string))
        //    {
        //        return Expression.Call(operands[0],
        //                           typeof(string).GetMethod("Replace", new[] { typeof(string), typeof(string) }),
        //                           operands[1], operands[2]);
        //    }
        //    return Expression.Call(operands[0],
        //                        typeof(string).GetMethod("Replace", new[] { typeof(char), typeof(char) }),
        //                        operands[1], operands[2]);
        //}
        //protected virtual Expression TranslateInsertString(List<Expression> operands)
        //{
        //    return Expression.Call(operands.First(), typeof(string).GetMethod("Insert"), operands[1], operands[2]);
        //}

        //protected virtual Expression TranslateTrim(List<Expression> operands)
        //{
        //    return Expression.Call(operands.First(), typeof(string).GetMethod("Trim", new Type[] { }));
        //}
        //protected virtual Expression TranslateSubString(List<Expression> operands)
        //{
        //    if (operands.Count > 2)
        //    {
        //        return Expression.Call(operands[0],
        //                               typeof(string).GetMethod("Substring", new[] { operands[1].Type, operands[2].Type }),
        //                               operands[1], operands[2]);
        //    }

        //    return Expression.Call(operands[0],
        //                           typeof(string).GetMethod("Substring", new[] { operands[1].Type }),
        //                           operands[1]);
        //}

        //protected virtual Expression TranslateToLower(List<Expression> operands)
        //{
        //    return Expression.Call(operands[0], typeof(string).GetMethod("ToLower", new Type[0]));
        //}

        //protected virtual Expression TranslateToUpper(List<Expression> operands)
        //{
        //    return Expression.Call(operands[0], typeof(string).GetMethod("ToUpper", new Type[0]));
        //}

        protected virtual Expression TranslateConcat(List<Expression> operands)
        {
            return Expression.Add(operands[0], operands[1]);
        }

        protected virtual Expression TranslateIsNotNull(List<Expression> operands)
        {
            return Expression.NotEqual(operands[0], Expression.Constant(null));
        }

        protected virtual Expression TranslateIsNull(List<Expression> operands)
        {
            return Expression.Equal(operands[0], Expression.Constant(null));
        }
    }
}