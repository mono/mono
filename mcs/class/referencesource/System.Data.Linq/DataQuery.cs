using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Data.Linq {
    using System.Data.Linq.Mapping;
    using System.Data.Linq.Provider;

    internal sealed class DataQuery<T> : IOrderedQueryable<T>, IQueryProvider, IEnumerable<T>, IOrderedQueryable, IEnumerable, IListSource {
        DataContext context;
        Expression queryExpression;
        private IBindingList cachedList;

        public DataQuery(DataContext context, Expression expression) {
            this.context = context;
            this.queryExpression = expression;
        }

        Expression IQueryable.Expression {
            get { return this.queryExpression; }
        }

        Type IQueryable.ElementType {
            get { return typeof(T); }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        IQueryable IQueryProvider.CreateQuery(Expression expression) {
            if (expression == null)
                throw Error.ArgumentNull("expression");
            Type eType = System.Data.Linq.SqlClient.TypeSystem.GetElementType(expression.Type);
            Type qType = typeof(IQueryable<>).MakeGenericType(eType);
            if (!qType.IsAssignableFrom(expression.Type))
                throw Error.ExpectedQueryableArgument("expression", qType);
            Type dqType = typeof(DataQuery<>).MakeGenericType(eType);
            return (IQueryable)Activator.CreateInstance(dqType, new object[] { this.context, expression });
        }

        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression) {
            if (expression == null)
                throw Error.ArgumentNull("expression");
            if (!typeof(IQueryable<S>).IsAssignableFrom(expression.Type))
                throw Error.ExpectedQueryableArgument("expression", typeof(IEnumerable<S>));
            return new DataQuery<S>(this.context, expression);
        }

        object IQueryProvider.Execute(Expression expression) {
            return this.context.Provider.Execute(expression).ReturnValue;
        }

        S IQueryProvider.Execute<S>(Expression expression) {
            return (S)this.context.Provider.Execute(expression).ReturnValue;
        }
        IQueryProvider IQueryable.Provider {
            get{
                return (IQueryProvider)this;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)this.context.Provider.Execute(this.queryExpression).ReturnValue).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return ((IEnumerable<T>)this.context.Provider.Execute(this.queryExpression).ReturnValue).GetEnumerator();
        }

        bool IListSource.ContainsListCollection {
            get { return false; }
        }

        IList IListSource.GetList() {
            if (cachedList == null) {
                cachedList = GetNewBindingList();
            }
            return cachedList;
        }

        internal IBindingList GetNewBindingList() {
            return BindingList.Create<T>(this.context, this);
        }

        public override string ToString() {
            return this.context.Provider.GetQueryText(this.queryExpression);
        }
    }
}
