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
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if MONO_STRICT
using System.Data.Linq.Sugar;
using System.Data.Linq.Sugar.ExpressionMutator;
using System.Data.Linq.Sugar.Expressions;
using System.Data.Linq.Sugar.Implementation;
using MappingContext = System.Data.Linq.Mapping.MappingContext;
#else
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;
using DbLinq.Data.Linq.Sugar.Implementation;
using MappingContext = DbLinq.Data.Linq.Mapping.MappingContext;
#endif

using DbLinq.Factory;


#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    internal partial class ExpressionDispatcher : IExpressionDispatcher
    {
        public IExpressionQualifier ExpressionQualifier { get; set; }
        public IDataRecordReader DataRecordReader { get; set; }
        public IDataMapper DataMapper { get; set; }

        public ExpressionDispatcher()
        {
            ExpressionQualifier = ObjectFactory.Get<IExpressionQualifier>();
            DataRecordReader = ObjectFactory.Get<IDataRecordReader>();
            DataMapper = ObjectFactory.Get<IDataMapper>();
        }

        /// <summary>
        /// Registers the first table. Extracts the table type and registeres the piece
        /// </summary>
        /// <param name="requestingExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual Expression CreateTableExpression(Expression requestingExpression, BuilderContext builderContext)
        {
            var callExpression = (MethodCallExpression)requestingExpression;
            var requestingType = callExpression.Arguments[0].Type;
            return CreateTable(GetQueriedType(requestingType), builderContext);
        }

        /// <summary>
        /// Registers the first table. Extracts the table type and registeres the piece
        /// </summary>
        /// <param name="requestingExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        public virtual Expression GetTable(Expression requestingExpression, BuilderContext builderContext)
        {
            var callExpression = (MethodCallExpression)requestingExpression;
            var requestingType = callExpression.Arguments[0].Type;
            return CreateTable(GetQueriedType(requestingType), builderContext);
        }

        /// <summary>
        /// Builds the upper select clause
        /// </summary>
        /// <param name="selectExpression"></param>
        /// <param name="builderContext"></param>
        public virtual void BuildSelect(Expression selectExpression, BuilderContext builderContext)
        {
            // collect columns, split Expression in
            // - things we will do in CLR
            // - things we will do in SQL
            LambdaExpression lambdaSelectExpression;
            // if we have a GroupByExpression, the result type is not the same:
            // - we need to read what is going to be the Key expression
            // - the final row generator builds a IGrouping<K,T> instead of T
            var selectGroupExpression = selectExpression as GroupExpression;
            if (selectGroupExpression != null)
            {
                lambdaSelectExpression = CutOutOperands(selectGroupExpression.GroupedExpression, builderContext);
                var lambdaSelectKeyExpression = CutOutOperands(selectGroupExpression.KeyExpression, builderContext);
                lambdaSelectExpression = BuildSelectGroup(lambdaSelectExpression, lambdaSelectKeyExpression,
                                                          builderContext);
            }
            else
                lambdaSelectExpression = CutOutOperands(selectExpression, builderContext);
            // look for tables and use columns instead
            // (this is done after cut, because the part that went to SQL must not be converted)
            //selectExpression = selectExpression.Recurse(e => CheckTableExpression(e, builderContext));
            // the last return value becomes the select, with CurrentScope
            builderContext.CurrentSelect.Reader = lambdaSelectExpression;
        }

        /// <summary>
        /// Builds the lambda as:
        /// (dr, mc) => new LineGrouping<K,T>(selectKey(dr,mc),select(dr,mc))
        /// </summary>
        /// <param name="select"></param>
        /// <param name="selectKey"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual LambdaExpression BuildSelectGroup(LambdaExpression select, LambdaExpression selectKey,
                                                            BuilderContext builderContext)
        {
            var dataRecordParameter = Expression.Parameter(typeof(IDataRecord), "dataRecord");
            var mappingContextParameter = Expression.Parameter(typeof(MappingContext), "mappingContext");
            var kType = selectKey.Body.Type;
            var lType = select.Body.Type;
            var groupingType = typeof(LineGrouping<,>).MakeGenericType(kType, lType);
            var groupingCtor = groupingType.GetConstructor(new[] { kType, lType });
            var invokeSelectKey = Expression.Invoke(selectKey, dataRecordParameter, mappingContextParameter);
            var invokeSelect = Expression.Invoke(select, dataRecordParameter, mappingContextParameter);
            var newLineGrouping = Expression.New(groupingCtor, invokeSelectKey, invokeSelect);
            var iGroupingType = typeof(IGrouping<,>).MakeGenericType(kType, lType);
            var newIGrouping = Expression.Convert(newLineGrouping, iGroupingType);
            var lambda = Expression.Lambda(newIGrouping, dataRecordParameter, mappingContextParameter);
            return lambda;
        }

        /// <summary>
        /// Cuts Expressions between CLR and SQL:
        /// - Replaces Expressions moved to SQL by calls to DataRecord values reader
        /// - SQL expressions are placed into Operands
        /// - Return value creator is the returned Expression
        /// </summary>
        /// <param name="selectExpression"></param>
        /// <param name="builderContext"></param>
        protected virtual LambdaExpression CutOutOperands(Expression selectExpression, BuilderContext builderContext)
        {
            var dataRecordParameter = Expression.Parameter(typeof(IDataRecord), "dataRecord");
            var mappingContextParameter = Expression.Parameter(typeof(MappingContext), "mappingContext");
            var expression = CutOutOperands(selectExpression, dataRecordParameter, mappingContextParameter, builderContext);
            return Expression.Lambda(expression, dataRecordParameter, mappingContextParameter);
        }

        /// <summary>
        /// Cuts tiers in CLR / SQL.
        /// The search for cut is top-down
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="dataRecordParameter"></param>
        /// <param name="mappingContextParameter"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression CutOutOperands(Expression expression,
                                                    ParameterExpression dataRecordParameter, ParameterExpression mappingContextParameter,
                                                    BuilderContext builderContext)
        {
            // two options: we cut and return
            if (GetCutOutOperand(expression, builderContext))
            {
                // "cutting out" means we replace the current expression by a SQL result reader
                // before cutting out, we check that we're not cutting a table
                // in this case, we convert it into its declared columns
                if (expression is TableExpression)
                {
                    return GetOutputTableReader((TableExpression)expression, dataRecordParameter,
                                                mappingContextParameter, builderContext);
                }
                // then, the result is registered
                return GetOutputValueReader(expression, dataRecordParameter, mappingContextParameter, builderContext);
            }
            // or we dig down
            var operands = new List<Expression>();
            foreach (var operand in expression.GetOperands())
            {
                operands.Add(CutOutOperands(operand, dataRecordParameter, mappingContextParameter, builderContext));
            }
            return expression.ChangeOperands(operands);
        }

        /// <summary>
        /// Returns true if we must cut out the given Expression
        /// </summary>
        /// <param name="operand"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        private bool GetCutOutOperand(Expression operand, BuilderContext builderContext)
        {
            bool cutOut = false;
            var tier = ExpressionQualifier.GetTier(operand);
            if ((tier & ExpressionTier.Sql) != 0) // we can cut out only if the following expressiong can go to SQL
            {
                // then we have two possible strategies, load the DB at max, then it's always true from here
                if (builderContext.QueryContext.MaximumDatabaseLoad)
                    cutOut = true;
                else // if no max database load then it's min: we switch to SQL only when CLR doesn't support the Expression
                    cutOut = (tier & ExpressionTier.Clr) == 0;
            }
            return cutOut;
        }

        /// <summary>
        /// Checks any expression for a TableExpression, and eventually replaces it with the convenient columns selection
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression CheckTableExpression(Expression expression, BuilderContext builderContext)
        {
            if (expression is TableExpression)
                return GetSelectTableExpression((TableExpression)expression, builderContext);
            return expression;
        }

        /// <summary>
        /// Replaces a table selection by a selection of all mapped columns (ColumnExpressions).
        /// ColumnExpressions will be replaced at a later time by the tier splitter
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        protected virtual Expression GetSelectTableExpression(TableExpression tableExpression, BuilderContext builderContext)
        {
            var bindings = new List<MemberBinding>();
            foreach (var columnExpression in RegisterAllColumns(tableExpression, builderContext))
            {
                var binding = Expression.Bind((MethodInfo) columnExpression.MemberInfo, columnExpression);
                bindings.Add(binding);
            }
            var newExpression = Expression.New(tableExpression.Type);
            return Expression.MemberInit(newExpression, bindings);
        }

        /// <summary>
        /// Returns a queried type from a given expression, or null if no type can be found
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Type GetQueriedType(Expression expression)
        {
            return GetQueriedType(expression.Type);
        }

        /// <summary>
        /// Extracts the type from the potentially generic type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual Type GetQueriedType(Type type)
        {
            if (typeof(IQueryable).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                    return type.GetGenericArguments()[0];
            }
            return null;
        }

        /// <summary>
        /// Returns the parameter name, if the Expression is a ParameterExpression, null otherwise
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual string GetParameterName(Expression expression)
        {
            if (expression is ParameterExpression)
                return ((ParameterExpression)expression).Name;
            return null;
        }

        /// <summary>
        /// Merges a parameter and a parameter list
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public virtual IList<Expression> MergeParameters(Expression p1, IEnumerable<Expression> p2)
        {
            var p = new List<Expression>();
            p.Add(p1);
            p.AddRange(p2);
            return p;
        }
    }
}
