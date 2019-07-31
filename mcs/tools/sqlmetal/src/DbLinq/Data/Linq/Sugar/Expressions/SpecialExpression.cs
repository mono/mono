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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.ObjectModel;

using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;

namespace DbLinq.Data.Linq.Sugar.Expressions
{
    /// <summary>
    /// Holds new expression types (sql related), all well as their operands
    /// </summary>
    [DebuggerDisplay("SpecialExpression {SpecialNodeType}")]
#if !MONO_STRICT
    public
#endif
    class SpecialExpression : OperandsMutableExpression, IExecutableExpression
    {
        public SpecialExpressionType SpecialNodeType { get { return (SpecialExpressionType)NodeType; } }

        protected static Type GetSpecialExpressionTypeType(SpecialExpressionType specialExpressionType, IList<Expression> operands)
        {
            Type defaultType;
            if (operands.Count > 0)
                defaultType = operands[0].Type;
            else
                defaultType = null;
            switch (specialExpressionType) // SETuse
            {
                case SpecialExpressionType.IsNull:
                case SpecialExpressionType.IsNotNull:
                    return typeof(bool);
                case SpecialExpressionType.Concat:
                    return typeof(string);
                case SpecialExpressionType.Count:
                    return typeof(int);
                case SpecialExpressionType.Exists:
                    return typeof(bool);
                case SpecialExpressionType.Like:
                    return typeof(bool);
                case SpecialExpressionType.Min:
                case SpecialExpressionType.Max:
                case SpecialExpressionType.Sum:
                    return defaultType; // for such methods, the type is related to the operands type
                case SpecialExpressionType.Average:
                    return typeof(double);
                case SpecialExpressionType.StringLength:
                    return typeof(int);
                case SpecialExpressionType.ToUpper:
                case SpecialExpressionType.ToLower:
                    return typeof(string);
                case SpecialExpressionType.In:
                    return typeof(bool);
                case SpecialExpressionType.Substring:
                    return defaultType;
                case SpecialExpressionType.Trim:
                case SpecialExpressionType.LTrim:
                case SpecialExpressionType.RTrim:
                    return typeof(string);
                case SpecialExpressionType.StringInsert:
                    return typeof(string);
                case SpecialExpressionType.Replace:
                    return typeof(string);
                case SpecialExpressionType.Remove:
                    return typeof(string);
                case SpecialExpressionType.IndexOf:
                    return typeof(int);
                case SpecialExpressionType.Year:
                case SpecialExpressionType.Month:
                case SpecialExpressionType.Day:
                case SpecialExpressionType.Hour:
                case SpecialExpressionType.Second:
                case SpecialExpressionType.Minute:
                case SpecialExpressionType.Millisecond:
                    return typeof(int);
                case SpecialExpressionType.Now:
                case SpecialExpressionType.Date:
                    return typeof(DateTime);
                case SpecialExpressionType.DateDiffInMilliseconds:
                    return typeof(long);
                case SpecialExpressionType.Abs:
                case SpecialExpressionType.Exp:
                case SpecialExpressionType.Floor:
                case SpecialExpressionType.Ln:
                case SpecialExpressionType.Log:
                case SpecialExpressionType.Pow:
                case SpecialExpressionType.Round:
                case SpecialExpressionType.Sign:
                case SpecialExpressionType.Sqrt:
                    return defaultType;

                default:
                    throw Error.BadArgument("S0058: Unknown SpecialExpressionType value {0}", specialExpressionType);
            }
        }

        public SpecialExpression(SpecialExpressionType expressionType, params Expression[] operands)
            : base((ExpressionType)expressionType, GetSpecialExpressionTypeType(expressionType, operands), operands)
        {
        }

        public SpecialExpression(SpecialExpressionType expressionType, IList<Expression> operands)
            : base((ExpressionType)expressionType, GetSpecialExpressionTypeType(expressionType, operands), operands)
        {
        }

        protected override Expression Mutate2(IList<Expression> newOperands)
        {
            return new SpecialExpression((SpecialExpressionType)NodeType, newOperands);
        }

        public object Execute()
        {
            switch (SpecialNodeType) // SETuse
            {
                case SpecialExpressionType.IsNull:
                    return operands[0].Evaluate() == null;
                case SpecialExpressionType.IsNotNull:
                    return operands[0].Evaluate() != null;
                case SpecialExpressionType.Concat:
                    {
                        var values = new List<string>();
                        foreach (var operand in operands)
                        {
                            var value = operand.Evaluate();
                            if (value != null)
                                values.Add(System.Convert.ToString(value, CultureInfo.InvariantCulture));
                            else
                                values.Add(null);
                        }
                        return string.Concat(values.ToArray());
                    }
                case SpecialExpressionType.Count:
                    {
                        var value = operands[0].Evaluate();
                        // TODO: string is IEnumerable. See what we do here
                        if (value is IEnumerable)
                        {
                            int count = 0;
                            foreach (var dontCare in (IEnumerable)value)
                                count++;
                            return count;
                        }
                        // TODO: by default, shall we answer 1 or throw an exception?
                        return 1;
                    }
                case SpecialExpressionType.Exists:
                    {
                        var value = operands[0].Evaluate();
                        // TODO: string is IEnumerable. See what we do here
                        if (value is IEnumerable)
                        {
                            return true;
                        }
                        // TODO: by default, shall we answer 1 or throw an exception?
                        return false;
                    }
                case SpecialExpressionType.Min:
                    {
                        decimal? min = null;
                        foreach (var operand in operands)
                        {
                            var value = System.Convert.ToDecimal(operand.Evaluate());
                            if (!min.HasValue || value < min.Value)
                                min = value;
                        }
                        return System.Convert.ChangeType(min.Value, operands[0].Type);
                    }
                case SpecialExpressionType.Max:
                    {
                        decimal? max = null;
                        foreach (var operand in operands)
                        {
                            var value = System.Convert.ToDecimal(operand.Evaluate());
                            if (!max.HasValue || value > max.Value)
                                max = value;
                        }
                        return System.Convert.ChangeType(max.Value, operands[0].Type);
                    }
                case SpecialExpressionType.Sum:
                    {
                        decimal sum = operands.Select(op => System.Convert.ToDecimal(op.Evaluate())).Sum();
                        return System.Convert.ChangeType(sum, operands.First().Type);
                    }
                case SpecialExpressionType.Average:
                    {
                        decimal sum = 0;
                        foreach (var operand in operands)
                            sum += System.Convert.ToDecimal(operand.Evaluate());
                        return sum / operands.Count;
                    }
                case SpecialExpressionType.StringLength:
                    return operands[0].Evaluate().ToString().Length;
                case SpecialExpressionType.ToUpper:
                    return operands[0].Evaluate().ToString().ToUpper();
                case SpecialExpressionType.ToLower:
                    return operands[0].Evaluate().ToString().ToLower();
                case SpecialExpressionType.Substring:
                    return EvaluateStandardCallInvoke("SubString", operands);
                case SpecialExpressionType.In:
                    throw new NotImplementedException();
                case SpecialExpressionType.Replace:
                    return EvaluateStandardCallInvoke("Replace", operands);
                case SpecialExpressionType.Remove:
                    return EvaluateStandardCallInvoke("Remove", operands);
                case SpecialExpressionType.IndexOf:
                    return EvaluateStandardCallInvoke("IndexOf", operands);
                case SpecialExpressionType.Year:
                    return ((DateTime)operands[0].Evaluate()).Year;
                case SpecialExpressionType.Month:
                    return ((DateTime)operands[0].Evaluate()).Month;
                case SpecialExpressionType.Day:
                    return ((DateTime)operands[0].Evaluate()).Day;
                case SpecialExpressionType.Hour:
                    return ((DateTime)operands[0].Evaluate()).Hour;
                case SpecialExpressionType.Minute:
                    return ((DateTime)operands[0].Evaluate()).Minute;
                case SpecialExpressionType.Second:
                    return ((DateTime)operands[0].Evaluate()).Second;
                case SpecialExpressionType.Millisecond:
                    return ((DateTime)operands[0].Evaluate()).Millisecond;
                case SpecialExpressionType.Now:
                    return DateTime.Now;
                case SpecialExpressionType.Date:
                    return ((DateTime)operands[0].Evaluate());
                case SpecialExpressionType.DateDiffInMilliseconds:
                    return ((DateTime)operands[0].Evaluate()) - ((DateTime)operands[1].Evaluate());
                case SpecialExpressionType.Abs:
                case SpecialExpressionType.Exp:
                case SpecialExpressionType.Floor:
                case SpecialExpressionType.Ln:
                case SpecialExpressionType.Log:
                case SpecialExpressionType.Pow:
                case SpecialExpressionType.Round:
                case SpecialExpressionType.Sign:
                case SpecialExpressionType.Sqrt:
                    return EvaluateMathCallInvoke(SpecialNodeType, operands);
                default:
                    throw Error.BadArgument("S0116: Unknown SpecialExpressionType ({0})", SpecialNodeType);
            }
        }

        private object EvaluateMathCallInvoke(SpecialExpressionType SpecialNodeType, ReadOnlyCollection<Expression> operands)
        {
            return typeof(Math).GetMethod(SpecialNodeType.ToString(), operands.Skip(1).Select(op => op.Type).ToArray())
                    .Invoke(null, operands.Skip(1).Select(op => op.Evaluate()).ToArray());
        }
        protected object EvaluateStatardMemberAccess(string propertyName, ReadOnlyCollection<Expression> operands)
        {
            return operands[0].Type.GetProperty(propertyName).GetValue(operands.First().Evaluate(), null);
        }
        protected object EvaluateStandardCallInvoke(string methodName, ReadOnlyCollection<Expression> operands)
        {
            return operands[0].Type.GetMethod(methodName,
                                       operands.Skip(1).Select(op => op.Type).ToArray())
                                       .Invoke(operands[0].Evaluate(),
                                               operands.Skip(1).Select(op => op.Evaluate()).ToArray());
        }
    }
}