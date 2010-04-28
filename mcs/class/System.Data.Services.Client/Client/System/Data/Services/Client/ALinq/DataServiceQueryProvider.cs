//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    #region Namespaces.

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    #endregion Namespaces.

    internal sealed class DataServiceQueryProvider : IQueryProvider
    {
        internal readonly DataServiceContext Context;

        internal DataServiceQueryProvider(DataServiceContext context)
        {
            this.Context = context;
        }

        #region IQueryProvider implementation

        public IQueryable CreateQuery(Expression expression)
        {
            Util.CheckArgumentNull(expression, "expression");
            Type et = TypeSystem.GetElementType(expression.Type);
            Type qt = typeof(DataServiceQuery<>.DataServiceOrderedQuery).MakeGenericType(et);
            object[] args = new object[] { expression, this };

            ConstructorInfo ci = qt.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, 
                null, 
                new Type[] { typeof(Expression), typeof(DataServiceQueryProvider) }, 
                null);

            return (IQueryable)Util.ConstructorInvoke(ci, args);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            Util.CheckArgumentNull(expression, "expression");
            return new DataServiceQuery<TElement>.DataServiceOrderedQuery(expression, this);
        }

        public object Execute(Expression expression)
        {
            Util.CheckArgumentNull(expression, "expression");

            MethodInfo mi = typeof(DataServiceQueryProvider).GetMethod("ReturnSingleton", BindingFlags.NonPublic | BindingFlags.Instance);
            return mi.MakeGenericMethod(expression.Type).Invoke(this, new object[] { expression });
        }

        public TResult Execute<TResult>(Expression expression)
        {
            Util.CheckArgumentNull(expression, "expression");
            return ReturnSingleton<TResult>(expression);
        }

        #endregion

        internal TElement ReturnSingleton<TElement>(Expression expression)
        {
            IQueryable<TElement> query = new DataServiceQuery<TElement>.DataServiceOrderedQuery(expression, this);

            MethodCallExpression mce = expression as MethodCallExpression;
            Debug.Assert(mce != null, "mce != null");

            SequenceMethod sequenceMethod;
            if (ReflectionUtil.TryIdentifySequenceMethod(mce.Method, out sequenceMethod))
            {
                switch (sequenceMethod)
                {
                    case SequenceMethod.Single:
                        return query.AsEnumerable().Single();
                    case SequenceMethod.SingleOrDefault:
                        return query.AsEnumerable().SingleOrDefault();
                    case SequenceMethod.First:
                        return query.AsEnumerable().First();
                    case SequenceMethod.FirstOrDefault:
                        return query.AsEnumerable().FirstOrDefault();
#if !ASTORIA_LIGHT
                    case SequenceMethod.LongCount:
                    case SequenceMethod.Count:
                        return (TElement)Convert.ChangeType(((DataServiceQuery<TElement>)query).GetQuerySetCount(this.Context), typeof(TElement), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
#endif
                    default:
                        throw Error.MethodNotSupported(mce);
                }
            }

            Debug.Assert(false, "Not supported singleton operator not caught by Resource Binder");
            throw Error.MethodNotSupported(mce);
        }

        internal QueryComponents Translate(Expression e)
        {
            Uri uri;
            Version version;
            bool addTrailingParens = false;
            Dictionary<Expression, Expression> normalizerRewrites = null;

            if (!(e is ResourceSetExpression))
            {
                normalizerRewrites = new Dictionary<Expression, Expression>(ReferenceEqualityComparer<Expression>.Instance);
                e = Evaluator.PartialEval(e);
                e = ExpressionNormalizer.Normalize(e, normalizerRewrites);
                e = ResourceBinder.Bind(e);
                addTrailingParens = true;
            }

            UriWriter.Translate(this.Context, addTrailingParens, e, out uri, out version);
            ResourceExpression re = e as ResourceExpression;
            Type lastSegmentType = re.Projection == null ? re.ResourceType : re.Projection.Selector.Parameters[0].Type;
            LambdaExpression selector = re.Projection == null ? null : re.Projection.Selector;
            return new QueryComponents(uri, version, lastSegmentType, selector, normalizerRewrites); 
        }
    }
}
