﻿#region MIT license
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif
using DbLinq.Data.Linq.Implementation;
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;
using DbLinq.Data.Linq.Sugar.Implementation;
using DbLinq.Factory;
using DbLinq.Util;

namespace DbLinq.Data.Linq.Sugar.Implementation
{
    partial class ExpressionDispatcher
    {
        public Expression Analyze(ExpressionChain expressions, Expression parameter, BuilderContext builderContext)
        {
            Expression tableExpression = parameter;

            Expression last = expressions.Last();
            IExpressionLanguageParser languageParser = ObjectFactory.Get<IExpressionLanguageParser>();
            foreach (Expression e in expressions)
            {
                if (e == last)
                    builderContext.IsExternalInExpressionChain = true;

                // write full debug
#if DEBUG && !MONO_STRICT
                var log = builderContext.QueryContext.DataContext.Log;
                if (log != null)
                    log.WriteExpression(e);
#endif

                // Convert linq Expressions to QueryOperationExpressions and QueryConstantExpressions 
                // Query expressions language identification
                var currentExpression = languageParser.Parse(e, builderContext);
                // Query expressions query identification 
                currentExpression = this.Analyze(currentExpression, tableExpression, builderContext);

                if (!builderContext.IsExternalInExpressionChain)
                {
                    EntitySetExpression setExpression = currentExpression as EntitySetExpression;
                    if (setExpression != null)
                        currentExpression = setExpression.TableExpression;
                }
                tableExpression = currentExpression;
            }

            return tableExpression;
        }

        /// <summary>
        /// Entry point for Analyzis
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="parameter"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression Analyze(Expression expression, Expression parameter, BuilderContext builderContext)
        {
            return Analyze(expression, new[] { parameter }, builderContext);
        }

        protected virtual Expression Analyze(Expression expression, BuilderContext builderContext)
        {
            return Analyze(expression, new Expression[0], builderContext);
        }

        protected virtual Expression Analyze(Expression expression, IList<Expression> parameters, BuilderContext builderContext)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    return AnalyzeCall((MethodCallExpression)expression, parameters, builderContext);
                case ExpressionType.Lambda:
                    return AnalyzeLambda(expression, parameters, builderContext);
                case ExpressionType.Parameter:
                    return AnalyzeParameter(expression, builderContext);
                case ExpressionType.Quote:
                    return AnalyzeQuote(expression, parameters, builderContext);
                case ExpressionType.MemberAccess:
                    return AnalyzeMember(expression, builderContext);
                #region case ExpressionType.<Common operators>:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Power:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.And:
                case ExpressionType.Or:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Coalesce:
                //case ExpressionType.ArrayIndex
                //case ExpressionType.ArrayLength
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                //case ExpressionType.TypeAs
                case ExpressionType.UnaryPlus:
                case ExpressionType.MemberInit:
                #endregion
                    return AnalyzeOperator(expression, builderContext);
                case ExpressionType.New:
                    return AnalyzeNewOperator(expression, builderContext);
                case ExpressionType.Constant:
                    return AnalyzeConstant(expression, builderContext);
                case ExpressionType.Invoke:
                    return AnalyzeInvoke(expression, parameters, builderContext);
            }
            return expression;
        }

        /// <summary>
        /// Analyzes method call, uses specified parameters
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeCall(MethodCallExpression expression, IList<Expression> parameters, BuilderContext builderContext)
        {
            var operands = expression.GetOperands().ToList();
            var operarandsToSkip = expression.Method.IsStatic ? 1 : 0;
            var originalParameters = operands.Skip(parameters.Count + operarandsToSkip);
            var newParameters = parameters.Union(originalParameters).ToList();

            return AnalyzeQueryableCall(expression.Method, newParameters, builderContext) ??
                AnalyzeStringCall(expression.Method, newParameters, builderContext) ??
                AnalyzeMathCall(expression.Method, newParameters, builderContext) ??
                AnalyzeUnknownCall(expression, newParameters, builderContext);
        }

        private Expression AnalyzeQueryableCall(MethodInfo method, IList<Expression> parameters, BuilderContext builderContext)
        {
            if (!(method.DeclaringType == typeof(Queryable) || method.DeclaringType == typeof(Enumerable)))
                return null;
            var popCallStack = PushCallStack(method, builderContext);
            // all methods to handle are listed here:
            // ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/fxref_system.core/html/2a54ce9d-76f2-81e2-95bb-59740c85386b.htm
            string methodName = method.Name;
            switch (methodName)
            {
                case "All":
                    return popCallStack(AnalyzeAll(parameters, builderContext));
                case "Any":
                    return popCallStack(AnalyzeAny(parameters, builderContext));
                case "Average":
                    return popCallStack(AnalyzeProjectionQuery(SpecialExpressionType.Average, parameters, builderContext));
                case "Concat":
                    return popCallStack(AnalyzeSelectOperation(SelectOperatorType.UnionAll, parameters, builderContext));
                case "Contains":
                    return popCallStack(AnalyzeContains(parameters, builderContext));
                case "Count":
                    return popCallStack(AnalyzeProjectionQuery(SpecialExpressionType.Count, parameters, builderContext));
                case "DefaultIfEmpty":
                    return popCallStack(AnalyzeOuterJoin(parameters, builderContext));
                case "Distinct":
                    return popCallStack(AnalyzeDistinct(parameters, builderContext));
                case "Except":
                    return popCallStack(AnalyzeSelectOperation(SelectOperatorType.Exception, parameters, builderContext));
                case "First":
                case "FirstOrDefault":
                    return popCallStack(AnalyzeScalar(methodName, 1, parameters, builderContext));
                case "GroupBy":
                    return popCallStack(AnalyzeGroupBy(parameters, builderContext));
                case "GroupJoin":
                    return popCallStack(AnalyzeGroupJoin(parameters, builderContext));
                case "Intersect":
                    return popCallStack(AnalyzeSelectOperation(SelectOperatorType.Intersection, parameters, builderContext));
                case "Join":
                    return popCallStack(AnalyzeJoin(parameters, builderContext));
                case "Last":
                    return popCallStack(AnalyzeScalar(methodName, null, parameters, builderContext));
                case "Max":
                    return popCallStack(AnalyzeProjectionQuery(SpecialExpressionType.Max, parameters, builderContext));
                case "Min":
                    return popCallStack(AnalyzeProjectionQuery(SpecialExpressionType.Min, parameters, builderContext));
                case "OrderBy":
                case "ThenBy":
                    return popCallStack(AnalyzeOrderBy(parameters, false, builderContext));
                case "OrderByDescending":
                case "ThenByDescending":
                    return popCallStack(AnalyzeOrderBy(parameters, true, builderContext));
                case "Select":
                    return popCallStack(AnalyzeSelect(parameters, builderContext));
                case "SelectMany":
                    return popCallStack(AnalyzeSelectMany(parameters, builderContext));
                case "Single":
                case "SingleOrDefault":
                    return popCallStack(AnalyzeScalar(methodName, 2, parameters, builderContext));
                case "Skip":
                    return popCallStack(AnalyzeSkip(parameters, builderContext));
                case "Sum":
                    return popCallStack(AnalyzeProjectionQuery(SpecialExpressionType.Sum, parameters, builderContext));
                case "Take":
                    return popCallStack(AnalyzeTake(parameters, builderContext));
                case "Union":
                    return popCallStack(AnalyzeSelectOperation(SelectOperatorType.Union, parameters, builderContext));
                case "Where":
                    return popCallStack(AnalyzeWhere(parameters, builderContext));
                default:
                    if (method.DeclaringType == typeof(Queryable))
                        throw Error.BadArgument("S0133: Implement QueryMethod Queryable.{0}.", methodName);
                    return popCallStack(null);
            }
        }

        Func<Expression, Expression> PushCallStack(MethodInfo method, BuilderContext builderContext)
        {

            builderContext.CallStack.Push(method);
            Func<Expression, Expression> popCallStack = r =>
            {
                builderContext.CallStack.Pop();
                return r;
            };
            return popCallStack;
        }

        private Expression AnalyzeStringCall(MethodInfo method, IList<Expression> parameters, BuilderContext builderContext)
        {
            if (method.DeclaringType != typeof(string))
                return null;
            var popCallStack = PushCallStack(method, builderContext);
            switch (method.Name)
            {
                case "Contains":
                    return popCallStack(AnalyzeLike(parameters, builderContext));
                case "EndsWith":
                    return popCallStack(AnalyzeLikeEnd(parameters, builderContext));
                case "IndexOf":
                    return popCallStack(AnalyzeGenericSpecialExpressionType(SpecialExpressionType.IndexOf, parameters, builderContext));
                case "Insert":
                    return popCallStack(AnalyzeStringInsert(parameters, builderContext));
                case "Remove":
                    return popCallStack(AnalyzeGenericSpecialExpressionType(SpecialExpressionType.Remove, parameters, builderContext));
                case "Replace":
                    return popCallStack(AnalyzeGenericSpecialExpressionType(SpecialExpressionType.Replace, parameters, builderContext));
                case "StartsWith":
                    return popCallStack(AnalyzeLikeStart(parameters, builderContext));
                case "Substring":
                    return popCallStack(AnalyzeSubString(parameters, builderContext));
                case "ToLower":
                    return popCallStack(AnalyzeToLower(parameters, builderContext));
                case "ToString":
                    return popCallStack(AnalyzeToString(method, parameters, builderContext));
                case "ToUpper":
                    return popCallStack(AnalyzeToUpper(parameters, builderContext));
                case "Trim":
                    return popCallStack(AnalyzeGenericSpecialExpressionType(SpecialExpressionType.Trim, parameters, builderContext));
                case "TrimEnd":
                    return popCallStack(AnalyzeGenericSpecialExpressionType(SpecialExpressionType.RTrim, parameters, builderContext));
                case "TrimStart":
                    return popCallStack(AnalyzeGenericSpecialExpressionType(SpecialExpressionType.LTrim, parameters, builderContext));
                default:
                    throw Error.BadArgument("S0133: Implement QueryMethod String.{0}.", method.Name);
            }
        }

        private Expression AnalyzeMathCall(MethodInfo method, IList<Expression> parameters, BuilderContext builderContext)
        {
            if (method.DeclaringType != typeof(System.Math))
                return null;
            var popCallStack = PushCallStack(method, builderContext);
            switch (method.Name)
            {
                case "Abs":
                case "Exp":
                case "Floor":
                case "Pow":
                case "Round":
                case "Sign":
                case "Sqrt":
                    return popCallStack(AnalyzeGenericSpecialExpressionType((SpecialExpressionType)Enum.Parse(typeof(SpecialExpressionType), method.Name), parameters, builderContext));
                case "Log":
                    return popCallStack(AnalyzeLog(parameters, builderContext));
                case "Log10":
                    return popCallStack(AnalyzeGenericSpecialExpressionType(SpecialExpressionType.Log, parameters, builderContext));
                default:
                    throw Error.BadArgument("S0133: Implement QueryMethod Math.{0}.", method.Name);
            }
        }

        private Expression AnalyzeUnknownCall(MethodCallExpression expression, IList<Expression> parameters, BuilderContext builderContext)
        {
            var method = expression.Method;
            switch (method.Name)
            {
                case "Parse":
                    if (method.IsStatic && parameters.Count == 1)
                        return AnalyzeParse(method, parameters, builderContext);
                    break;
                case "ToString": // Can we sanity check this type?
                    return AnalyzeToString(method, parameters, builderContext);
            }

            var args = new List<Expression>();
            foreach (var arg in expression.Arguments)
            {
                Expression newArg = arg;
                var pe = arg as ParameterExpression;
                if (pe != null)
                {
                    if (!builderContext.Parameters.TryGetValue(pe.Name, out newArg))
                        throw new NotSupportedException("Do not currently support expression: " + expression);
                }
                else
                    newArg = Analyze(arg, builderContext);
                args.Add(newArg);
            }
            return Expression.Call(expression.Object, expression.Method, args);
        }

        private Expression AnalyzeStringInsert(IList<Expression> parameters, BuilderContext builderContext)
        {
            var startIndexExpression = new StartIndexOffsetExpression(builderContext.QueryContext.DataContext.Vendor.SqlProvider.StringIndexStartsAtOne, parameters.ElementAt(1));
            var stringToInsertExpression = parameters.ElementAt(2);
            return AnalyzeGenericSpecialExpressionType(SpecialExpressionType.StringInsert, new Expression[] { parameters.First(), startIndexExpression, stringToInsertExpression }, builderContext);
        }

        protected virtual Expression AnalyzeLog(IList<Expression> parameters, BuilderContext builderContext)
        {
            if (parameters.Count == 1)
                return new SpecialExpression(SpecialExpressionType.Ln, parameters.Select(p => Analyze(p, builderContext)).ToList());
            else if (parameters.Count == 2)
                return new SpecialExpression(SpecialExpressionType.Log, parameters.Select(p => Analyze(p, builderContext)).ToList());
            else
                throw new NotSupportedException();
        }

        protected virtual Expression AnalyzeGenericSpecialExpressionType(SpecialExpressionType specialType, IList<Expression> parameters, BuilderContext builderContext)
        {
            return new SpecialExpression(specialType, parameters.Select(p => Analyze(p, builderContext)).ToList());
        }

        protected virtual Expression AnalyzeParse(MethodInfo method, IList<Expression> parameters, BuilderContext builderContext)
        {
            if (method.IsStatic && parameters.Count == 1)
            {
                Expression parsed = null;
                Expression toParse = Analyze(parameters.First(), builderContext);
                InputParameterExpression inputParameterToParse = toParse as InputParameterExpression;
                if (inputParameterToParse != null)
                {
                    ExpressionTier tier = ExpressionQualifier.GetTier(parameters[0]);
                    if (tier == ExpressionTier.Clr)
                    {
                        parsed = RegisterParameter(System.Linq.Expressions.Expression.Call(method, inputParameterToParse.Expression), inputParameterToParse.Alias, builderContext);
                        UnregisterParameter(inputParameterToParse, builderContext);
                    }
                }
                if(parsed == null)
                {
                    parsed = Expression.Convert(toParse, method.ReturnType, method);
                    ExpressionTier tier = ExpressionQualifier.GetTier(toParse);
                    //pibgeus: I would like to call to the expression optimizer since the exception must be thrown if the expression cannot be executed
                    //in Clr tier, if it can be executed in Clr tier it should continue
                    // ie: from e in db.Employees where DateTime.Parse("1/1/1999").Year==1999 select e  <--- this should work
                    // ie: from e in db.Employees where DateTime.Parse(e.BirthDate).Year==1999 select e  <--- a NotSupportedException must be throwed (this is the behaviour of linq2sql)

                    //if (method.ReturnType == typeof(DateTime))
                    //{
                    //        expression = ExpressionOptimizer.Analyze(expression);
                    //        //same behaviour that Linq2Sql
                    //        throw new NotSupportedException("Method 'System.DateTime Parse(System.String)' has no supported translation to SQL");
                    //}
                }
                return parsed;
            }
            else
                throw new ArgumentException();

        }

        protected virtual Expression AnalyzeToString(MethodInfo method, IList<Expression> parameters, BuilderContext builderContext)
        {
            if (parameters.Count != 1)
                throw new ArgumentException();

            Expression parameter = parameters.First();
            Expression parameterToHandle;

            if(parameter.Type.IsNullable())
                parameter = Analyze(Expression.Convert(parameter, parameter.Type.GetNullableType()), builderContext);

            parameterToHandle = Analyze(parameter, builderContext);

            InputParameterExpression inputParameter = parameterToHandle as InputParameterExpression;
            if (inputParameter != null)
            {
                parameterToHandle = RegisterParameter(System.Linq.Expressions.Expression.Call(inputParameter.Expression, method), inputParameter.Alias, builderContext);
                UnregisterParameter(inputParameter, builderContext);

                return parameterToHandle;
            }
            
            if (!parameter.Type.IsPrimitive && parameterToHandle.Type != typeof(string))
            {
                //TODO: ExpressionDispacher.Analyze.AnalyzeToString is not complete
                //This is the standar behaviour in linq2sql, nonetheless the behaviour isn't complete since when the expression
                //can be executed in the clr, ie: (where new StrangeObject().ToString()) should work. The problem is that
                //we don't have a reference to the optimizer here.
                //Working samples in: /Tests/Test_Nunit/ReadTests_Conversions.cs
                string message = "Method ToString can only be translated to SQL for primitive types.";
                int? select = FirstIndexOf(builderContext.CallStack, "Select");
                int? where  = FirstIndexOf(builderContext.CallStack, "Where");
                if ((where ?? int.MaxValue) < (select ?? int.MaxValue))
                    // Assume we're generating the .Where() clause, not .Select()
                    throw new NotSupportedException(message);
                // for .Select()
                throw new InvalidOperationException(message);
            }

            return Expression.Convert(parameterToHandle, typeof(string), typeof(Convert).GetMethod("ToString", new[] { parameterToHandle.Type }));
        }

        static int? FirstIndexOf(Stack<MethodInfo> callStack, string methodName)
        {
            int? index = null;
            callStack.Where((m, i) =>
            {
                if (m.Name == methodName)
                {
                    index = i;
                    return true;
                }
                return false;
            }).FirstOrDefault();
            return index;
        }

        /// <summary>
        /// Limits selection count
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeTake(IList<Expression> parameters, BuilderContext builderContext)
        {
            AddLimit(Analyze(parameters[1], builderContext), builderContext);
            return Analyze(parameters[0], builderContext);
        }

        protected virtual void AddLimit(Expression limit, BuilderContext builderContext)
        {
            var previousLimit = builderContext.CurrentSelect.Limit;
            if (previousLimit != null)
                builderContext.CurrentSelect.Limit = Expression.Condition(Expression.LessThan(previousLimit, limit),
                                                                          previousLimit, limit);
            else
                builderContext.CurrentSelect.Limit = limit;
        }

        /// <summary>
        /// Skip selection items
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeSkip(IList<Expression> parameters, BuilderContext builderContext)
        {
            AddOffset(Analyze(parameters[1], builderContext), builderContext);
            return Analyze(parameters[0], builderContext);
        }

        protected virtual void AddOffset(Expression offset, BuilderContext builderContext)
        {
            var previousOffset = builderContext.CurrentSelect.Offset;
            if (previousOffset != null)
                builderContext.CurrentSelect.Offset = Expression.Add(offset, previousOffset);
            else
                builderContext.CurrentSelect.Offset = offset;
        }

        /// <summary>
        /// Registers a scalar method call for result
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="limit"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeScalar(string methodName, int? limit, IList<Expression> parameters, BuilderContext builderContext)
        {
            builderContext.CurrentSelect.ExecuteMethodName = methodName;
            if (limit.HasValue)
                AddLimit(Expression.Constant(limit.Value), builderContext);
            var table = Analyze(parameters[0], builderContext);
            var set = table as EntitySetExpression;
            if (set != null)
                table = set.TableExpression;
            CheckWhere(table, parameters, 1, builderContext);
            return table;
        }

        /// <summary>
        /// Some methods, like Single(), Count(), etc. can get an extra parameter, specifying a restriction.
        /// This method checks if the parameter is specified, and adds it to the WHERE clauses
        /// </summary>
        /// <param name="table"></param>
        /// <param name="parameters"></param>
        /// <param name="extraParameterIndex"></param>
        /// <param name="builderContext"></param>
        private void CheckWhere(Expression table, IList<Expression> parameters, int extraParameterIndex, BuilderContext builderContext)
        {
            if (parameters.Count > extraParameterIndex) // a lambda can be specified here, this is a restriction
                RegisterWhere(Analyze(parameters[extraParameterIndex], table, builderContext), builderContext);
        }

        /// <summary>
        /// Returns a projection method call
        /// </summary>
        /// <param name="specialExpressionType"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeProjectionQuery(SpecialExpressionType specialExpressionType, IList<Expression> parameters,
                                                            BuilderContext builderContext)
        {

            if (builderContext.IsExternalInExpressionChain)
            {
                var operand0 = Analyze(parameters[0], builderContext);
                Expression projectionOperand;

                if (    builderContext.CurrentSelect.NextSelectExpression != null 
                    ||  builderContext.CurrentSelect.Operands.Count() > 0
                    ||  builderContext.CurrentSelect.Group.Count > 0
                   )
                {
                    //BuildSelect(builderContext.CurrentSelect, builderContext);
                    operand0 = new SubSelectExpression(builderContext.CurrentSelect, operand0.Type, "source");
                    builderContext.NewParentSelect();

                    // In the new scope we should not have MaximumDatabaseLoad
                    builderContext.QueryContext.MaximumDatabaseLoad = false;

                    builderContext.CurrentSelect.Tables.Add(operand0 as TableExpression);
                }

                // basically, we have three options for projection methods:
                // - projection on grouped table (1 operand, a GroupExpression)
                // - projection on grouped column (2 operands, GroupExpression and ColumnExpression)
                // - projection on table/column, with optional restriction
                var groupOperand0 = operand0 as GroupExpression;
                if (groupOperand0 != null)
                {
                    if (parameters.Count > 1)
                    {
                        projectionOperand = Analyze(parameters[1], groupOperand0.GroupedExpression,
                                                    builderContext);
                    }
                    else
                        projectionOperand = Analyze(groupOperand0.GroupedExpression, builderContext);
                }
                else
                {
                    projectionOperand = operand0;
                    CheckWhere(projectionOperand, parameters, 1, builderContext);
                }

                if (projectionOperand is TableExpression)
                    projectionOperand = RegisterTable((TableExpression)projectionOperand, builderContext);

                if (groupOperand0 != null)
                    projectionOperand = new GroupExpression(projectionOperand, groupOperand0.KeyExpression);

                return new SpecialExpression(specialExpressionType, projectionOperand);
            }
            else
            {
                var projectionQueryBuilderContext = builderContext.NewSelect();

                var tableExpression = Analyze(parameters[0], projectionQueryBuilderContext);

                if (!(tableExpression is TableExpression) && !(tableExpression is EntitySetExpression))
                    tableExpression = Analyze(tableExpression, projectionQueryBuilderContext);
                EntitySetExpression setExpression = tableExpression as EntitySetExpression;
                if (setExpression != null)
                    tableExpression = setExpression.TableExpression;

                // from here we build a custom clause:
                // <anyClause> ==> "(select count(*) from <table> where <anyClause>)>0"
                // TODO (later...): see if some vendors support native Any operator and avoid this substitution
                if (parameters.Count > 1)
                {
                    setExpression = tableExpression as EntitySetExpression;
                    if (setExpression != null)
                        tableExpression = setExpression.TableExpression;
                    var anyClause = Analyze(parameters[1], tableExpression, projectionQueryBuilderContext);
                    RegisterWhere(anyClause, projectionQueryBuilderContext);
                }

                projectionQueryBuilderContext.CurrentSelect = projectionQueryBuilderContext.CurrentSelect.ChangeOperands(new SpecialExpression(specialExpressionType, tableExpression));

                // we now switch back to current context, and compare the result with 0
                return projectionQueryBuilderContext.CurrentSelect;
            }
        }

        /// <summary>
        /// Entry point for a Select()
        /// static Select(this Expression table, λ(table))
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeSelect(IList<Expression> parameters, BuilderContext builderContext)
        {
            // just call back the underlying lambda (or quote, whatever)
            Expression ex = Analyze(parameters[1], parameters[0], builderContext);

            // http://social.msdn.microsoft.com/Forums/en-US/linqprojectgeneral/thread/1ce25da3-44c6-407d-8395-4c146930004b
            if (ex.NodeType == ExpressionType.MemberInit &&
                    builderContext.QueryContext.DataContext.Mapping.GetMetaType(ex.Type) != null)
                throw new NotSupportedException(
                    string.Format("Explicit construction of entity type '{0}' in query is not allowed.",
                        ex.Type.FullName));
            TableExpression tableExpression = parameters[0] as TableExpression;
            if (tableExpression != null && builderContext.CurrentSelect.Tables.Count == 0)
                RegisterTable(tableExpression, builderContext);
            return ex;
        }

        /// <summary>
        /// Entry point for a Where()
        /// static Where(this Expression table, λ(table))
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeWhere(IList<Expression> parameters, BuilderContext builderContext)
        {
			var tablePiece = parameters[0];
            RegisterWhere(Analyze(parameters[1], tablePiece, builderContext), builderContext);
            return tablePiece;
        }

        /// <summary>
        /// Handling a lambda consists in:
        /// - filling its input parameters with what's on the stack
        /// - using the body (parameters are registered in the context)
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeLambda(Expression expression, IList<Expression> parameters, BuilderContext builderContext)
        {
            var lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression == null)
                throw Error.BadArgument("S0227: Unknown type for AnalyzeLambda() ({0})", expression.GetType());
            // for a lambda, first parameter is body, others are input parameters
            // so we create a parameters stack
            for (int parameterIndex = 0; parameterIndex < lambdaExpression.Parameters.Count; parameterIndex++)
            {
                var parameterExpression = lambdaExpression.Parameters[parameterIndex];
                builderContext.Parameters[parameterExpression.Name] = Analyze(parameters[parameterIndex], builderContext);
            }
            // we keep only the body, the header is now useless
            // and once the parameters have been substituted, we don't pass one anymore
            return Analyze(lambdaExpression.Body, builderContext);
        }

        /// <summary>
        /// When a parameter is used, we replace it with its original value
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeParameter(Expression expression, BuilderContext builderContext)
        {
            Expression unaliasedExpression;
            var parameterName = GetParameterName(expression);
            builderContext.Parameters.TryGetValue(parameterName, out unaliasedExpression);
            if (unaliasedExpression == null)
                throw Error.BadArgument("S0257: can not find parameter '{0}'", parameterName);

            #region set alias helper

            // for table...
            var unaliasedTableExpression = unaliasedExpression as TableExpression;
            if (unaliasedTableExpression != null && unaliasedTableExpression.Alias == null)
                unaliasedTableExpression.Alias = parameterName;
            // .. or column
            var unaliasedColumnExpression = unaliasedExpression as ColumnExpression;
            if (unaliasedColumnExpression != null && unaliasedColumnExpression.Alias == null)
                unaliasedColumnExpression.Alias = parameterName;

            #endregion

            //var groupByExpression = unaliasedExpression as GroupByExpression;
            //if (groupByExpression != null)
            //    unaliasedExpression = groupByExpression.ColumnExpression.Table;

            return unaliasedExpression;
        }

        /// <summary>
        /// Returns if the given member can be considered as an EntitySet<>
        /// </summary>
        /// <param name="memberType"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        protected virtual bool IsEntitySet(Type memberType, out Type entityType)
        {
            entityType = memberType;
            // one check, a generic EntityRef<> or inherited
            if (memberType.IsGenericType && typeof(EntitySet<>).IsAssignableFrom(memberType.GetGenericTypeDefinition()))
            {
                entityType = memberType.GetGenericArguments()[0];
                return true;
            }
#if !MONO_STRICT
            // this is for compatibility with previously generated .cs files
            // TODO: remove in 2009
            if (memberType.IsGenericType && typeof(System.Data.Linq.EntitySet<>).IsAssignableFrom(memberType.GetGenericTypeDefinition()))
            {
                entityType = memberType.GetGenericArguments()[0];
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Analyzes a member access.
        /// This analyzis is down to top: the highest identifier is at bottom
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeMember(Expression expression, BuilderContext builderContext)
        {
            var memberExpression = (MemberExpression)expression;

            Expression objectExpression = null;
            //maybe is a static member access like DateTime.Now
            bool isStaticMemberAccess = memberExpression.Member.GetIsStaticMember();

            var memberInfo = memberExpression.Member;
            if (!isStaticMemberAccess && memberInfo.Name == "Count")
                return AnalyzeProjectionQuery(SpecialExpressionType.Count, new[] { memberExpression.Expression }, builderContext);

            if (!isStaticMemberAccess)
                // first parameter is object, second is member
                objectExpression = Analyze(memberExpression.Expression, builderContext);

            // then see what we can do, depending on object type
            // - MetaTable --> then the result is a table
            // - Table --> the result may be a column or a join
            // - Object --> external parameter or table (can this happen here? probably not... to be checked)

            EntitySetExpression setExpression = objectExpression as EntitySetExpression;
            if (setExpression != null)
            {
                objectExpression = setExpression.TableExpression;
            }

            if (objectExpression is MetaTableExpression)
            {
                var metaTableExpression = (MetaTableExpression)objectExpression;
                var tableExpression = metaTableExpression.GetTableExpression(memberInfo);
                if (tableExpression == null)
                    throw Error.BadArgument("S0270: MemberInfo '{0}' not found in MetaTable", memberInfo.Name);
                return tableExpression;
            }

            if (objectExpression is GroupExpression)
            {
                if (memberInfo.Name == "Key")
                    return ((GroupExpression)objectExpression).KeyExpression;
            }

            // if object is a table, then we need a column, or an association
            if (objectExpression is TableExpression)
            {
                var tableExpression = (TableExpression)objectExpression;


                // before finding an association, we check for an EntitySet<>
                // this will be used in RegisterAssociation
                Type entityType;
                if (IsEntitySet(memberInfo.GetMemberType(), out entityType))
                    return new EntitySetExpression(tableExpression, memberInfo, memberInfo.GetMemberType(), builderContext, this);

                // first of all, then, try to find the association
                var queryAssociationExpression = RegisterAssociation(tableExpression, memberInfo, entityType,
                                                                     builderContext);
                if (queryAssociationExpression != null)
                {
                    return queryAssociationExpression;
                }
                // then, try the column
                ColumnExpression queryColumnExpression = RegisterColumn(tableExpression, memberInfo, builderContext);
                if (queryColumnExpression != null)
                {
                    Type storageType = queryColumnExpression.StorageInfo != null ? queryColumnExpression.StorageInfo.GetMemberType() : null;
                    if (storageType != null && queryColumnExpression.Type != storageType)
                    {
                        return Expression.Convert(queryColumnExpression, queryColumnExpression.Type, typeof(Convert).GetMethod("To" + queryColumnExpression.Type.Name, new Type[] { queryColumnExpression.Type }));
                    }
                    else
                    {
                        return queryColumnExpression;
                    }
                }
                // then, cry
                throw Error.BadArgument("S0293: Column must be mapped. Non-mapped columns are not handled by now.");
            }

            // if object is still an object (== a constant), then we have an external parameter
            if (objectExpression is ConstantExpression)
            {
                // the memberInfo.Name is provided here only to ease the SQL reading
                var parameterExpression = RegisterParameter(expression, memberInfo.Name, builderContext);
                if (parameterExpression != null)
                    return parameterExpression;
                throw Error.BadArgument("S0302: Can not created parameter from expression '{0}'", expression);
            }

            // we have here a special cases for nullables
            if (!isStaticMemberAccess && objectExpression.Type != null && objectExpression.Type.IsNullable())
            {
                // Value means we convert the nullable to a value --> use Convert instead (works both on CLR and SQL, too)
                if (memberInfo.Name == "Value")
                    return Expression.Convert(objectExpression, memberInfo.GetMemberType());
                // HasValue means not null (works both on CLR and SQL, too)
                if (memberInfo.Name == "HasValue")
                    return new SpecialExpression(SpecialExpressionType.IsNotNull, objectExpression);
            }


            if (memberInfo.DeclaringType == typeof(DateTime))
                return AnalyzeDateTimeMemberAccess(objectExpression, memberInfo, isStaticMemberAccess);

            // TODO: make this expresion safe (objectExpression can be null here)
            if (objectExpression.Type == typeof(TimeSpan))
                return AnalyzeTimeSpanMemberAccess(objectExpression, memberInfo);


            if (objectExpression is InputParameterExpression)
            {
                return AnalyzeExternalParameterMember((InputParameterExpression)objectExpression, memberInfo, builderContext);
            }

            if (objectExpression is MemberInitExpression)
            {
                var foundExpression = AnalyzeMemberInit((MemberInitExpression)objectExpression, memberInfo, builderContext);
                if (foundExpression != null)
                    return foundExpression;
            }

            return AnalyzeCommonMember(objectExpression, memberInfo, builderContext);
        }

        protected Expression AnalyzeTimeSpanMemberAccess(Expression objectExpression, MemberInfo memberInfo)
        {
            //A timespan expression can be only generated in a c# query as a DateTime difference, as a function call return or as a paramter
            //this case is for the DateTime difference operation

            if (!(objectExpression is BinaryExpression))
                throw new NotSupportedException();

            var operands = objectExpression.GetOperands();

            bool absoluteSpam = memberInfo.Name.StartsWith("Total");
            string operationKey = absoluteSpam ? memberInfo.Name.Substring(5) : memberInfo.Name;

            Expression currentExpression;
            switch (operationKey)
            {
                case "Milliseconds":
                    currentExpression = Expression.Convert(new SpecialExpression(SpecialExpressionType.DateDiffInMilliseconds, operands.First(), operands.ElementAt(1)), typeof(double));
                    break;
                case "Seconds":
                    currentExpression = Expression.Divide(
                        Expression.Convert(new SpecialExpression(SpecialExpressionType.DateDiffInMilliseconds, operands.First(), operands.ElementAt(1)), typeof(double)),
                        Expression.Constant(1000.0));
                    break;
                case "Minutes":
                    currentExpression = Expression.Divide(
                            Expression.Convert(new SpecialExpression(SpecialExpressionType.DateDiffInMilliseconds, operands.First(), operands.ElementAt(1)), typeof(double)),
                            Expression.Constant(60000.0));
                    break;
                case "Hours":
                    currentExpression = Expression.Divide(
                            Expression.Convert(new SpecialExpression(SpecialExpressionType.DateDiffInMilliseconds, operands.First(), operands.ElementAt(1)), typeof(double)),
                            Expression.Constant(3600000.0));
                    break;
                case "Days":
                    currentExpression = Expression.Divide(
                            Expression.Convert(new SpecialExpression(SpecialExpressionType.DateDiffInMilliseconds, operands.First(), operands.ElementAt(1)), typeof(double)),
                            Expression.Constant(86400000.0));
                    break;
                default:
                    throw new NotSupportedException(string.Format("The operation {0} over the TimeSpan isn't currently supported", memberInfo.Name));
            }

            if (!absoluteSpam)
            {
                switch (memberInfo.Name)
                {
                    case "Milliseconds":
                        currentExpression = Expression.Convert(Expression.Modulo(Expression.Convert(currentExpression, typeof(long)), Expression.Constant(1000L)), typeof(int));
                        break;
                    case "Seconds":
                        currentExpression = Expression.Convert(Expression.Modulo(Expression.Convert(currentExpression, typeof(long)),
                                                              Expression.Constant(60L)), typeof(int));
                        break;
                    case "Minutes":
                        currentExpression = Expression.Convert(Expression.Modulo(Expression.Convert(currentExpression, typeof(long)),
                                                                Expression.Constant(60L)), typeof(int));
                        break;
                    case "Hours":
                        currentExpression = Expression.Convert(Expression.Modulo(Expression.Convert(
                                                                                        currentExpression, typeof(long)),
                                                                Expression.Constant(24L)), typeof(int));
                        break;
                    case "Days":
                        currentExpression = Expression.Convert(currentExpression, typeof(int));
                        break;
                }

            }
            return currentExpression;
        }

        protected Expression AnalyzeDateTimeMemberAccess(Expression objectExpression, MemberInfo memberInfo, bool isStaticMemberAccess)
        {
            if (isStaticMemberAccess)
            {
                if (memberInfo.Name == "Now")
                    return new SpecialExpression(SpecialExpressionType.Now);
                else
                    throw new NotSupportedException(string.Format("DateTime Member access {0} not supported", memberInfo.Name));
            }
            else
            {
                switch (memberInfo.Name)
                {
                    case "Year":
                        return new SpecialExpression(SpecialExpressionType.Year, objectExpression);
                    case "Month":
                        return new SpecialExpression(SpecialExpressionType.Month, objectExpression);
                    case "Day":
                        return new SpecialExpression(SpecialExpressionType.Day, objectExpression);
                    case "Hour":
                        return new SpecialExpression(SpecialExpressionType.Hour, objectExpression);
                    case "Minute":
                        return new SpecialExpression(SpecialExpressionType.Minute, objectExpression);
                    case "Second":
                        return new SpecialExpression(SpecialExpressionType.Second, objectExpression);
                    case "Millisecond":
                        return new SpecialExpression(SpecialExpressionType.Millisecond, objectExpression);
                    case "Date":
                        return new SpecialExpression(SpecialExpressionType.Date, objectExpression);
                    default:
                        throw new NotSupportedException(string.Format("DateTime Member access {0} not supported", memberInfo.Name));
                }
            }
        }

        /// <summary>
        /// This method analyzes the case of a new followed by a member access
        /// for example new "A(M = value).M", where the Expression can be reduced to "value"
        /// Caution: it may return null if no result is found
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="memberInfo"></param>
        /// <param name="builderContext"></param>
        /// <returns>A member initializer or null</returns>
        protected virtual Expression AnalyzeMemberInit(MemberInitExpression expression, MemberInfo memberInfo,
                                                       BuilderContext builderContext)
        {
            // TODO: a method for NewExpression that we will use directly from AnalyzeMember and indirectly from here
            foreach (var binding in expression.Bindings)
            {
                var memberAssignment = binding as MemberAssignment;
                if (memberAssignment != null)
                {
                    if (memberAssignment.Member == memberInfo)
                        return memberAssignment.Expression;
                }
            }
            return null;
        }

        protected virtual Expression AnalyzeExternalParameterMember(InputParameterExpression expression, MemberInfo memberInfo, BuilderContext builderContext)
        {
            UnregisterParameter(expression, builderContext);
            return RegisterParameter(Expression.MakeMemberAccess(expression.Expression, memberInfo), memberInfo.Name, builderContext);
        }

        protected virtual Expression AnalyzeCommonMember(Expression objectExpression, MemberInfo memberInfo, BuilderContext builderContext)
        {
            if (typeof(string).IsAssignableFrom(objectExpression.Type))
            {
                switch (memberInfo.Name)
                {
                    case "Length":
                        return new SpecialExpression(SpecialExpressionType.StringLength, objectExpression);
                }
            }
            //throw Error.BadArgument("S0324: Don't know how to handle Piece");
            return Expression.MakeMemberAccess(objectExpression, memberInfo);
        }

        /// <summary>
        /// A Quote creates a new local context, outside which created parameters disappear
        /// This is why we clone the BuilderContext
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeQuote(Expression piece, IList<Expression> parameters, BuilderContext builderContext)
        {
            var builderContextClone = builderContext.NewQuote();
            var firstExpression = piece.GetOperands().First();
            return Analyze(firstExpression, parameters, builderContextClone);
        }

        /// <summary>
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeOperatorSubstract(Expression expression, BuilderContext builderContext)
        {
            return AnalyzeOperator(expression, builderContext);
        }

        /// <summary>
        /// Operator analysis consists in anlyzing all operands
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeOperator(Expression expression, BuilderContext builderContext)
        {
            var u = expression as UnaryExpression;
            string parameterName;
            if (expression.NodeType == ExpressionType.Convert && 
                    u.Method == null &&
                    (parameterName = GetParameterName(u.Operand)) != null)
            {
                Expression unaliasedExpression;
                builderContext.Parameters.TryGetValue(parameterName, out unaliasedExpression);
                var unaliasedTableExpression = unaliasedExpression as TableExpression;
                if (unaliasedExpression != null && unaliasedTableExpression != null)
                    return unaliasedTableExpression;
            }
            var operands = expression.GetOperands().ToList();
            for (int operandIndex = 0; operandIndex < operands.Count; operandIndex++)
            {
                var operand = operands[operandIndex];
                operands[operandIndex] = Analyze(operand, builderContext);
            }

            return expression.ChangeOperands(operands);
        }

        protected virtual Expression AnalyzeNewOperator(Expression expression, BuilderContext builderContext)
        {
            if (builderContext.ExpectMetaTableDefinition)
            {
                // first, check if we have a MetaTable definition
                Type metaType;
                var typeInitializers = GetTypeInitializers<Expression>((NewExpression)expression, true, out metaType);
                var aliases = new Dictionary<MemberInfo, MutableExpression>();
                foreach (var memberInfo in typeInitializers.Keys)
                {
                    var e = Analyze(typeInitializers[memberInfo], builderContext);
                    var tableExpression = e as TableExpression;
                    var ese = e as EntitySetExpression;
                    if (ese != null)
                        tableExpression = ese.TableExpression;
                    if (tableExpression != null)
                    {
                        aliases[memberInfo] = tableExpression;
                    }
                    else
                    {
                        aliases[memberInfo] = Analyze(typeInitializers[memberInfo], builderContext) as MetaTableExpression;
                    }
                }
                if (IsMetaTableDefinition(aliases))
                    return RegisterMetaTable(metaType, aliases, builderContext);
            }
            return AnalyzeOperator(expression, builderContext);
        }

        protected virtual bool IsMetaTableDefinition(IDictionary<MemberInfo, MutableExpression> aliases)
        {
            if (aliases.Count != 2)
                return false;
            foreach (var tableExpression in aliases.Values)
            {
                if (tableExpression == null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// SelectMany() joins tables
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeSelectMany(IList<Expression> parameters, BuilderContext builderContext)
        {
            if (parameters.Count == 3)
            {
                // ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/fxref_system.core/html/3371348f-7811-b0bc-8c0a-2a595e08e086.htm
                var tableExpression = parameters[0];
                var projectionExpression = Analyze(parameters[1], new[] { tableExpression }, builderContext);
                //var manyPiece = Analyze(parameters[2], new[] { tableExpression, projectionExpression }, builderContext);
                // from here, our manyPiece is a MetaTable definition
                //var newExpression = manyPiece as NewExpression;
                //if (newExpression == null)
                //    throw Error.BadArgument("S0377: Expected a NewExpression as SelectMany() return value");
                //Type metaTableType;
                //var associations = GetTypeInitializers<TableExpression>(newExpression, true, out metaTableType);
                //return RegisterMetaTable(metaTableType, associations, builderContext);
                var metaTableDefinitionBuilderContext = builderContext.Clone();
                metaTableDefinitionBuilderContext.ExpectMetaTableDefinition = true;
                var expression = Analyze(parameters[2], new[] { tableExpression, projectionExpression },
                                         metaTableDefinitionBuilderContext);
                return expression;
            }
            throw Error.BadArgument("S0358: Don't know how to handle this SelectMany() overload ({0} parameters)", parameters.Count);
        }

        protected virtual IDictionary<MemberInfo, E> GetTypeInitializers<E>(NewExpression newExpression,
                                                                            bool checkCast, out Type metaType)
            where E : Expression
        {
            var associations = new Dictionary<MemberInfo, E>();
            metaType = null;
            for (int ctorParameterIndex = 0; ctorParameterIndex < newExpression.Arguments.Count; ctorParameterIndex++)
            {
                var aliasedExpression = newExpression.Arguments[ctorParameterIndex] as E;
                if (aliasedExpression == null && checkCast)
                    throw Error.BadArgument("S0541: Expected an specific Expression type for GetTypeInitializers()");
                var memberInfo = newExpression.Members[ctorParameterIndex];
                metaType = memberInfo.ReflectedType;
                // the property info is the reflecting property for the memberInfo, if memberInfo is a get_*
                // otherwise we keep the memberInfo as is, since it is a field
                var propertyInfo = memberInfo.GetExposingProperty() ?? memberInfo;
                associations[propertyInfo] = aliasedExpression;
            }
            if (metaType == null && checkCast)
                throw Error.BadArgument("S0550: Empty NewExpression found"); // this should never happen, otherwise we may simply ignore it or take the type from elsewhere
            return associations;
        }

        //protected virtual IDictionary<MemberInfo, E> GetTypeInitializers<E>(NewExpression newExpression)
        //    where E : Expression
        //{
        //    Type metaType;
        //    return GetTypeInitializers<E>(newExpression, out metaType);
        //}

        /// <summary>
        /// Analyzes a Join statement (explicit join)
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeJoin(IList<Expression> parameters, BuilderContext builderContext)
        {
            return AnalyzeJoin(parameters, TableJoinType.Inner, builderContext);
        }

        /// <summary>
        /// Analyzes a Join statement (explicit join)
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeGroupJoin(IList<Expression> parameters, BuilderContext builderContext)
        {
            return AnalyzeJoin(parameters, TableJoinType.Inner, builderContext);
        }

        protected virtual Expression AnalyzeOuterJoin(IList<Expression> parameters, BuilderContext builderContext)
        {
            var expression = Analyze(parameters[0], builderContext);
            var tableExpression = expression as TableExpression;
            if (tableExpression != null)
            {
                tableExpression.SetOuterJoin();
            }
            return expression;
        }

        private Expression AnalyzeJoin(IList<Expression> parameters, TableJoinType joinType, BuilderContext builderContext)
        {
            if (parameters.Count == 5)
            {
                var leftExpression = Analyze(parameters[0], builderContext);
                var rightTable = Analyze(parameters[1], builderContext) as TableExpression;
                if (rightTable == null)
                    throw Error.BadArgument("S0536: Expected a TableExpression for Join");
                var leftJoin = Analyze(parameters[2], leftExpression, builderContext);
                var rightJoin = Analyze(parameters[3], rightTable, builderContext);
                // from here, we have two options to join:
                // 1. left and right are tables, we can use generic expressions (most common)
                // 2. left is something else (a meta table)
                var leftTable = leftExpression as TableExpression;
                if (leftTable == null)
                {
                    var leftColumn = leftJoin as ColumnExpression;
                    if (leftColumn == null)
                        throw Error.BadArgument("S0701: No way to find left table for Join");
                    leftTable = leftColumn.Table;
                }
                rightTable.Join(joinType, leftTable, Expression.Equal(leftJoin, rightJoin),
                                string.Format("join{0}", builderContext.EnumerateAllTables().Count()));
                // last part is lambda, with two tables as parameters
                var metaTableDefinitionBuilderContext = builderContext.Clone();
                metaTableDefinitionBuilderContext.ExpectMetaTableDefinition = true;
                var expression = Analyze(parameters[4], new[] { leftExpression, rightTable }, metaTableDefinitionBuilderContext);
                return expression;
            }
            throw Error.BadArgument("S0530: Don't know how to handle GroupJoin() with {0} parameters", parameters.Count);
        }

        /// <summary>
        /// "Distinct" means select X group by X
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeDistinct(IList<Expression> parameters, BuilderContext builderContext)
        {
            var expression = Analyze(parameters[0], builderContext);
            // we select and group by the same criterion
            var group = new GroupExpression(expression, expression);
            if (builderContext.CurrentSelect.NextSelectExpression != null)
            {
                expression = new SubSelectExpression(builderContext.CurrentSelect, expression.Type, "source");
                builderContext.NewParentSelect();

                // In the new scope we should not have MaximumDatabaseLoad
                builderContext.QueryContext.MaximumDatabaseLoad = false;

                builderContext.CurrentSelect.Tables.Add(expression as TableExpression);
            }
            builderContext.CurrentSelect.Group.Add(group);
            // "Distinct" method is equivalent to a GroupBy
            // but for some obscure reasons, Linq expects a IQueryable instead of an IGrouping
            // so we return the column, not the group
            return expression;
        }

        /// <summary>
        /// Creates a group by clause
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeGroupBy(IList<Expression> parameters, BuilderContext builderContext)
        {
            var table = Analyze(parameters[0], builderContext);
            var keyExpression = Analyze(parameters[1], table, builderContext);

            Expression result;
            if (parameters.Count == 2)
                result = table; // we return the whole table
            else if (parameters.Count == 3)
                result = Analyze(parameters[2], table, builderContext); // 3 parameters for a projection expression
            else
                throw Error.BadArgument("S0629: Don't know how to handle Expression to group by with {0} parameters", parameters.Count);

            var group = new GroupExpression(result, keyExpression);
            builderContext.CurrentSelect.Group.Add(group);
            return group;
        }

        /// <summary>
        /// All() returns true if the given condition satisfies all provided elements
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeAll(IList<Expression> parameters, BuilderContext builderContext)
        {
            var allBuilderContext = builderContext.NewSelect();
            var tableExpression = Analyze(parameters[0], allBuilderContext);
            var allClause = Analyze(parameters[1], tableExpression, allBuilderContext);
            // from here we build a custom clause:
            // <allClause> ==> "(select count(*) from <table> where not <allClause>)==0"
            // TODO (later...): see if some vendors support native All operator and avoid this substitution
            var whereExpression = Expression.Not(allClause);
            RegisterWhere(whereExpression, allBuilderContext);
            allBuilderContext.CurrentSelect = allBuilderContext.CurrentSelect.ChangeOperands(new SpecialExpression(SpecialExpressionType.Count, tableExpression));
            // TODO: see if we need to register the tablePiece here (we probably don't)

            // we now switch back to current context, and compare the result with 0
            var allExpression = Expression.Equal(allBuilderContext.CurrentSelect, Expression.Constant(0));
            return allExpression;
        }

        /// <summary>
        /// Any() returns true if the given condition satisfies at least one of provided elements
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeAny(IList<Expression> parameters, BuilderContext builderContext)
        {
            if (builderContext.IsExternalInExpressionChain)
            {
                var tableExpression = Analyze(parameters[0], builderContext);
                Expression projectionOperand;

                if (builderContext.CurrentSelect.NextSelectExpression != null)
                {
                    TableExpression currentTableExpression = tableExpression as TableExpression;
                    tableExpression = new SubSelectExpression(builderContext.CurrentSelect, currentTableExpression.Type, "source");
                    builderContext.NewParentSelect();

                    // In the new scope we should not have MaximumDatabaseLoad
                    builderContext.QueryContext.MaximumDatabaseLoad = false;

                    builderContext.CurrentSelect.Tables.Add(tableExpression as TableExpression);
                }

                // basically, we have three options for projection methods:
                // - projection on grouped table (1 operand, a GroupExpression)
                // - projection on grouped column (2 operands, GroupExpression and ColumnExpression)
                // - projection on table/column, with optional restriction
                var groupOperand0 = tableExpression as GroupExpression;
                if (groupOperand0 != null)
                {
                    if (parameters.Count > 1)
                    {
                        projectionOperand = Analyze(parameters[1], groupOperand0.GroupedExpression,
                                                    builderContext);
                    }
                    else
                        projectionOperand = Analyze(groupOperand0.GroupedExpression, builderContext);
                }
                else
                {
                    projectionOperand = tableExpression;
                    CheckWhere(projectionOperand, parameters, 1, builderContext);
                }

                if (projectionOperand is TableExpression)
                    projectionOperand = RegisterTable((TableExpression)projectionOperand, builderContext);

                if (groupOperand0 != null)
                    projectionOperand = new GroupExpression(projectionOperand, groupOperand0.KeyExpression);

                return Expression.GreaterThan(new SpecialExpression(SpecialExpressionType.Count, projectionOperand), Expression.Constant(0));
            }
            else
            {
                var anyBuilderContext = builderContext.NewSelect();
                var tableExpression = Analyze(parameters[0], anyBuilderContext);

                if (!(tableExpression is TableExpression) && !(tableExpression is EntitySetExpression))
                    tableExpression = Analyze(tableExpression, anyBuilderContext);
                EntitySetExpression setExpression = tableExpression as EntitySetExpression;
                if (setExpression != null)
                    tableExpression = setExpression.TableExpression;

                // from here we build a custom clause:
                // <anyClause> ==> "(select count(*) from <table> where <anyClause>)>0"
                // TODO (later...): see if some vendors support native Any operator and avoid this substitution
                if (parameters.Count > 1)
                {
                    setExpression = tableExpression as EntitySetExpression;
                    if (setExpression != null)
                        tableExpression = setExpression.TableExpression;
                    var anyClause = Analyze(parameters[1], tableExpression, anyBuilderContext);
                    RegisterWhere(anyClause, anyBuilderContext);
                }
                anyBuilderContext.CurrentSelect = anyBuilderContext.CurrentSelect.ChangeOperands(new SpecialExpression(SpecialExpressionType.Count, tableExpression));
                // TODO: see if we need to register the tablePiece here (we probably don't)

                // we now switch back to current context, and compare the result with 0
                var anyExpression = Expression.GreaterThan(anyBuilderContext.CurrentSelect, Expression.Constant(0));
                return anyExpression;
            }
        }

        protected virtual Expression AnalyzeLikeStart(IList<Expression> parameters, BuilderContext builderContext)
        {
            return AnalyzeLike(parameters[0], null, parameters[1], "%", builderContext);
        }

        protected virtual Expression AnalyzeLikeEnd(IList<Expression> parameters, BuilderContext builderContext)
        {
            return AnalyzeLike(parameters[0], "%", parameters[1], null, builderContext);
        }

        protected virtual Expression AnalyzeLike(IList<Expression> parameters, BuilderContext builderContext)
        {
            return AnalyzeLike(parameters[0], "%", parameters[1], "%", builderContext);
        }

        protected virtual Expression AnalyzeLike(Expression value, string before, Expression operand, string after, BuilderContext builderContext)
        {
            operand = Analyze(operand, builderContext);
            if (before != null)
                operand = new SpecialExpression(SpecialExpressionType.Concat, Expression.Constant(before), operand);
            if (after != null)
                operand = new SpecialExpression(SpecialExpressionType.Concat, operand, Expression.Constant(after));
            return new SpecialExpression(SpecialExpressionType.Like, Analyze(value, builderContext), operand);
        }

        protected virtual Expression AnalyzeSubString(IList<Expression> parameters, BuilderContext builderContext)
        {
            var stringExpression = Analyze(parameters[0], builderContext);
            var startExpression = new StartIndexOffsetExpression(builderContext.QueryContext.DataContext.Vendor.SqlProvider.StringIndexStartsAtOne,
                                                        Analyze(parameters[1], builderContext));
            if (parameters.Count > 2)
            {
                var lengthExpression = parameters[2];
                return new SpecialExpression(SpecialExpressionType.Substring, stringExpression, startExpression, lengthExpression);
            }
            return new SpecialExpression(SpecialExpressionType.Substring, stringExpression, startExpression);
        }

        protected virtual Expression AnalyzeContains(IList<Expression> parameters, BuilderContext builderContext)
        {
            if (parameters[0].Type.IsArray)
            {
                Expression array = Analyze(parameters[0], builderContext);
                var expression = Analyze(parameters[1], builderContext);
                return new SpecialExpression(SpecialExpressionType.In, expression, array);
            }
            else
            {
                if (typeof(IQueryable).IsAssignableFrom(parameters[0].Type))
                {
                    Expression p0 = Analyze(parameters[1], builderContext);
                    BuilderContext newContext = builderContext.NewSelect();
                    InputParameterExpression ip1 = new InputParameterExpression(parameters[0], "dummy");

                    Expression p1 = AnalyzeQueryProvider(ip1.GetValue() as QueryProvider, newContext);
                    ColumnExpression c = p1 as ColumnExpression;
                    if (!newContext.CurrentSelect.Tables.Contains(c.Table))
                    {
                        newContext.CurrentSelect.Tables.Add(c.Table);
                    }
                    // TODO: verify if this is the right place to work
                    return new SpecialExpression(SpecialExpressionType.In, p0, newContext.CurrentSelect.Mutate(new Expression[] { p1 }));
                }
            }
            throw Error.BadArgument("S0548: Can't analyze Contains() method");
        }

        protected virtual Expression AnalyzeToUpper(IList<Expression> parameters, BuilderContext builderContext)
        {
            return new SpecialExpression(SpecialExpressionType.ToUpper, Analyze(parameters[0], builderContext));
        }

        protected virtual Expression AnalyzeToLower(IList<Expression> parameters, BuilderContext builderContext)
        {
            return new SpecialExpression(SpecialExpressionType.ToLower, Analyze(parameters[0], builderContext));
        }

        /// <summary>
        /// Registers ordering request
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="descending"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeOrderBy(IList<Expression> parameters, bool descending, BuilderContext builderContext)
        {
            var table = Analyze(parameters[0], builderContext);
            // the column is related to table
            var column = Analyze(parameters[1], table, builderContext);
            builderContext.CurrentSelect.OrderBy.Add(new OrderByExpression(descending, column));
            return table;
        }

        /// <summary>
        /// Analyzes constant expression value, and eventually extracts a table
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeConstant(Expression expression, BuilderContext builderContext)
        {
            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
            {
                var queriedType = GetQueriedType(expression);
                if (queriedType != null)
                {
                    //return new TableExpression(queriedType, DataMapper.GetTableName(queriedType, builderContext.QueryContext.DataContext));
                }
                if (constantExpression.Value is ITable)
                {
                    var tableType = constantExpression.Type.GetGenericArguments()[0];
                    return CreateTable(tableType, builderContext);
                }
                else
                {
                    QueryProvider queryProvider = constantExpression.Value as QueryProvider;
                    if (queryProvider != null)
                    {
                        Expression tableExpression = AnalyzeQueryProvider(queryProvider, builderContext.NewQuote());
                        return tableExpression;
                    }
                }
            }
            return expression;
        }

        protected virtual Expression AnalyzeQueryProvider(QueryProvider queryProvider, BuilderContext builderContext)
        {
            // TODO: check if the QueryProvider queryProvider belong to DataContext present in builderContext.QueryContext.DataContext
            // otherwise strange things could happen in the future (I suppose)

            // Build a new Context for the query
            ExpressionChain expressions = queryProvider.ExpressionChain;
            Expression tableExpression = CreateTableExpression(queryProvider.ExpressionChain.Expressions[0], builderContext);

            return this.Analyze(expressions, tableExpression, builderContext);
        }

        protected virtual Expression AnalyzeSelectOperation(SelectOperatorType operatorType, IList<Expression> parameters, BuilderContext builderContext)
        {
            // a special case: if we have several SELECT expressions linked together,
            // we maximize the load to the database, since the result must use the same parameters
            // types and count.
            builderContext.QueryContext.MaximumDatabaseLoad = true; // all select expression goes to SQL tier

            var constantExpression = parameters[1] as ConstantExpression;
            QueryProvider queryProvider = constantExpression.Value as QueryProvider;
            if (queryProvider != null)
            {
                // Handle second select first
                BuilderContext newContext = builderContext.NewSisterSelect();
                Expression tableExpression = AnalyzeQueryProvider(queryProvider, newContext);
                BuildSelect(tableExpression, newContext);

                // add the second select select to the chain
                if (newContext.CurrentSelect.NextSelectExpression != null)
                {
                    var operand0 = new SubSelectExpression(newContext.CurrentSelect, tableExpression.Type, "source");
                    newContext.NewParentSelect();
                    newContext.CurrentSelect.Tables.Add(operand0);
                }
                SelectExpression selectToModify = builderContext.CurrentSelect;
                while (selectToModify.NextSelectExpression != null)
                    selectToModify = selectToModify.NextSelectExpression;

                selectToModify.NextSelectExpression = newContext.CurrentSelect;
                selectToModify.NextSelectExpressionOperator = operatorType;

                Expression firstSelection = Analyze(parameters[0], builderContext);
                BuildSelect(firstSelection, builderContext);

                return firstSelection;
            }

            return Analyze(parameters[0], builderContext);
        }

        /// <summary>
        /// Analyses InvokeExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeInvoke(Expression expression, IList<Expression> parameters,
                                                   BuilderContext builderContext)
        {
            var invocationExpression = (InvocationExpression)expression;
            var lambda = invocationExpression.Expression as LambdaExpression;
            if (lambda != null)
            {
                var localBuilderContext = builderContext.NewQuote();
                //for (int parameterIndex = 0; parameterIndex < lambda.Parameters.Count; parameterIndex++)
                //{
                //    var parameter = lambda.Parameters[parameterIndex];
                //    localBuilderContext.Parameters[parameter.Name] = Analyze(invocationExpression.Arguments[parameterIndex], builderContext);
                //}
                //return Analyze(lambda, localBuilderContext);
                return Analyze(lambda, invocationExpression.Arguments, localBuilderContext);
            }
            // TODO: see what we must do here
            return expression;
        }
    }
}
