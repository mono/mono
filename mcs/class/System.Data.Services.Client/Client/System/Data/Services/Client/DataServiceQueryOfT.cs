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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
#if !ASTORIA_LIGHT    
    using System.Net;
#else
    using System.Data.Services.Http;
#endif
    using System.Reflection;
    using System.Collections;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "required for this feature")]
    public class DataServiceQuery<TElement> : DataServiceQuery, IQueryable<TElement>
    {
        #region Private fields.

        private readonly Expression queryExpression;

        private readonly DataServiceQueryProvider queryProvider;

        private QueryComponents queryComponents;

        #endregion Private fields.

        private DataServiceQuery(Expression expression, DataServiceQueryProvider provider)
        {
            Debug.Assert(null != provider.Context, "null context");
            Debug.Assert(expression != null, "null expression");
            Debug.Assert(provider is DataServiceQueryProvider, "Currently only support Web Query Provider");

            this.queryExpression = expression;
            this.queryProvider = provider;
        }

        #region IQueryable implementation
        public override Type ElementType
        {
            get { return typeof(TElement); }
        }

        public override Expression Expression
        {
            get { return this.queryExpression; }
        }

        public override IQueryProvider Provider
        {
            get { return this.queryProvider; }
        }

        #endregion

        public override Uri RequestUri
        {
            get
            {
                return this.Translate().Uri;
            }
        }

        internal override ProjectionPlan Plan
        {
            get { return null; }
        }

        internal override QueryComponents QueryComponents
        {
            get
            {
                return this.Translate();
            }
        }

        public new IAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            return base.BeginExecute(this, this.queryProvider.Context, callback, state);
        }

        public new IEnumerable<TElement> EndExecute(IAsyncResult asyncResult)
        {
            return DataServiceRequest.EndExecute<TElement>(this, this.queryProvider.Context, asyncResult);
        }

#if !ASTORIA_LIGHT        
        public new IEnumerable<TElement> Execute()
        {
            return this.Execute<TElement>(this.queryProvider.Context, this.Translate());
        }
#endif

        public DataServiceQuery<TElement> Expand(string path)
        {
            Util.CheckArgumentNull(path, "path");
            Util.CheckArgumentNotEmpty(path, "path");

            MethodInfo mi = typeof(DataServiceQuery<TElement>).GetMethod("Expand");
            return (DataServiceQuery<TElement>)this.Provider.CreateQuery<TElement>(
                Expression.Call(
                    Expression.Convert(this.Expression, typeof(DataServiceQuery<TElement>.DataServiceOrderedQuery)),
                    mi,
                    new Expression[] { Expression.Constant(path) }));
        }

        public DataServiceQuery<TElement> IncludeTotalCount()
        {
            MethodInfo mi = typeof(DataServiceQuery<TElement>).GetMethod("IncludeTotalCount");
            
            return (DataServiceQuery<TElement>)this.Provider.CreateQuery<TElement>(
                Expression.Call(
                    Expression.Convert(this.Expression, typeof(DataServiceQuery<TElement>.DataServiceOrderedQuery)),
                    mi));
        }

        public DataServiceQuery<TElement> AddQueryOption(string name, object value)
        {
            Util.CheckArgumentNull(name, "name");
            Util.CheckArgumentNull(value, "value");
            MethodInfo mi = typeof(DataServiceQuery<TElement>).GetMethod("AddQueryOption");
            return (DataServiceQuery<TElement>)this.Provider.CreateQuery<TElement>(
                Expression.Call(
                    Expression.Convert(this.Expression, typeof(DataServiceQuery<TElement>.DataServiceOrderedQuery)),
                    mi,
                    new Expression[] { Expression.Constant(name), Expression.Constant(value, typeof(object)) }));
        }

#if !ASTORIA_LIGHT        
        public IEnumerator<TElement> GetEnumerator()
        {
            return this.Execute().GetEnumerator();            
        }
#else
        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            throw Error.NotSupported(Strings.DataServiceQuery_EnumerationNotSupportedInSL);
        }
#endif

        public override string ToString()
        {
            try
            {
                return base.ToString();
            }
            catch (NotSupportedException e)
            {
                return Strings.ALinq_TranslationError(e.Message);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
#if !ASTORIA_LIGHT            
    return this.GetEnumerator();
#else
            throw Error.NotSupported();
#endif
        }

#if !ASTORIA_LIGHT
        internal override IEnumerable ExecuteInternal()
        {
            return this.Execute();
        }
#endif

        internal override IAsyncResult BeginExecuteInternal(AsyncCallback callback, object state)
        {
            return this.BeginExecute(callback, state);
        }

        internal override IEnumerable EndExecuteInternal(IAsyncResult asyncResult)
        {
            return this.EndExecute(asyncResult);
        }

        private QueryComponents Translate()
        {
            if (this.queryComponents == null)
            {
                this.queryComponents = this.queryProvider.Translate(this.queryExpression);
            }

            return this.queryComponents;
        }

        internal class DataServiceOrderedQuery : DataServiceQuery<TElement>, IOrderedQueryable<TElement>, IOrderedQueryable
        {
            internal DataServiceOrderedQuery(Expression expression, DataServiceQueryProvider provider)
                : base(expression, provider)
            {
            }
        }
    }
}
