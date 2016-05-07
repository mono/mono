using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

// Include Silverlight's managed resources
#if SILVERLIGHT
using System.Core;
#endif //SILVERLIGHT

namespace System.Linq {

    public interface IQueryable : IEnumerable {
        Expression Expression { get; }
        Type ElementType { get; }

        // the provider that created this query
        IQueryProvider Provider { get; }
    }

    public interface IQueryable<out T> : IEnumerable<T>, IQueryable {
    }

    public interface IQueryProvider{
        IQueryable CreateQuery(Expression expression);
        IQueryable<TElement> CreateQuery<TElement>(Expression expression);

        object Execute(Expression expression);

        TResult Execute<TResult>(Expression expression);
    }

    public interface IOrderedQueryable : IQueryable {
    }

    public interface IOrderedQueryable<out T> : IQueryable<T>, IOrderedQueryable {
    }

    public static class Queryable {

#region Helper methods to obtain MethodInfo in a safe way

        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1) {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2) {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> f, T1 unused1, T2 unused2, T3 unused3) {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4) {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5) {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5, T6 unused6) {
            return f.Method;
        }

#endregion

        public static IQueryable<TElement> AsQueryable<TElement>(this IEnumerable<TElement> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (source is IQueryable<TElement>)
                return (IQueryable<TElement>)source;
            return new EnumerableQuery<TElement>(source);
        }

        public static IQueryable AsQueryable(this IEnumerable source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (source is IQueryable)
                return (IQueryable)source;
            Type enumType = TypeHelper.FindGenericType(typeof(IEnumerable<>), source.GetType());
            if (enumType == null)
                throw Error.ArgumentNotIEnumerableGeneric("source");
            return EnumerableQuery.Create(enumType.GetGenericArguments()[0], source);
        }

        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Where, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Where, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static IQueryable<TResult> OfType<TResult>(this IQueryable source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.CreateQuery<TResult>( 
                Expression.Call(
                    null, 
                    GetMethodInfo(Queryable.OfType<TResult>, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static IQueryable<TResult> Cast<TResult>(this IQueryable source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.CreateQuery<TResult>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Cast<TResult>, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static IQueryable<TResult> Select<TSource,TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.CreateQuery<TResult>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Select, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static IQueryable<TResult> Select<TSource,TResult>(this IQueryable<TSource> source, Expression<Func<TSource, int, TResult>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.CreateQuery<TResult>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Select, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static IQueryable<TResult> SelectMany<TSource,TResult>(this IQueryable<TSource> source, Expression<Func<TSource, IEnumerable<TResult>>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.CreateQuery<TResult>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.SelectMany, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static IQueryable<TResult> SelectMany<TSource,TResult>(this IQueryable<TSource> source, Expression<Func<TSource, int, IEnumerable<TResult>>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.CreateQuery<TResult>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.SelectMany, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, int, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector){
            if (source == null)
                throw Error.ArgumentNull("source");
            if (collectionSelector == null)
                throw Error.ArgumentNull("collectionSelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            return source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.SelectMany, source, collectionSelector, resultSelector),
                    new Expression[] { source.Expression, Expression.Quote(collectionSelector), Expression.Quote(resultSelector) }
                    ));
        }

        public static IQueryable<TResult> SelectMany<TSource,TCollection,TResult>(this IQueryable<TSource> source, Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (collectionSelector == null)
                throw Error.ArgumentNull("collectionSelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            return source.Provider.CreateQuery<TResult>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.SelectMany, source, collectionSelector, resultSelector),
                    new Expression[] { source.Expression, Expression.Quote(collectionSelector), Expression.Quote(resultSelector) }
                    ));
        }

        private static Expression GetSourceExpression<TSource>(IEnumerable<TSource> source) {
            IQueryable<TSource> q = source as IQueryable<TSource>;
            if (q != null) return q.Expression;
            return Expression.Constant(source, typeof(IEnumerable<TSource>));
        }

        public static IQueryable<TResult> Join<TOuter,TInner,TKey,TResult>(this IQueryable<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter,TKey>> outerKeySelector, Expression<Func<TInner,TKey>> innerKeySelector, Expression<Func<TOuter,TInner,TResult>> resultSelector) {
            if (outer == null)
                throw Error.ArgumentNull("outer");
            if (inner == null)
                throw Error.ArgumentNull("inner");
            if (outerKeySelector == null)
                throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null)
                throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            return outer.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Join, outer, inner, outerKeySelector, innerKeySelector, resultSelector),
                    new Expression[] { 
                        outer.Expression, 
                        GetSourceExpression(inner),
                        Expression.Quote(outerKeySelector), 
                        Expression.Quote(innerKeySelector),
                        Expression.Quote(resultSelector) 
                        }
                    ));
        }

        public static IQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector, IEqualityComparer<TKey> comparer) {
            if (outer == null)
                throw Error.ArgumentNull("outer");
            if (inner == null)
                throw Error.ArgumentNull("inner");
            if (outerKeySelector == null)
                throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null)
                throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            return outer.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Join, outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer),
                    new Expression[] { 
                        outer.Expression, 
                        GetSourceExpression(inner),
                        Expression.Quote(outerKeySelector), 
                        Expression.Quote(innerKeySelector),
                        Expression.Quote(resultSelector),
                        Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))
                        }
                    ));
        }

        public static IQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector) {
            if (outer == null)
                throw Error.ArgumentNull("outer");
            if (inner == null)
                throw Error.ArgumentNull("inner");
            if (outerKeySelector == null)
                throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null)
                throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            return outer.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.GroupJoin, outer, inner, outerKeySelector, innerKeySelector, resultSelector),
                    new Expression[] { 
                        outer.Expression, 
                        GetSourceExpression(inner),
                        Expression.Quote(outerKeySelector), 
                        Expression.Quote(innerKeySelector), 
                        Expression.Quote(resultSelector) }
                    ));
        }

        public static IQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector, IEqualityComparer<TKey> comparer) {
            if (outer == null)
                throw Error.ArgumentNull("outer");
            if (inner == null)
                throw Error.ArgumentNull("inner");
            if (outerKeySelector == null)
                throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null)
                throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            return outer.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.GroupJoin, outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer),
                    new Expression[] { 
                        outer.Expression, 
                        GetSourceExpression(inner),
                        Expression.Quote(outerKeySelector), 
                        Expression.Quote(innerKeySelector), 
                        Expression.Quote(resultSelector),
                        Expression.Constant(comparer, typeof(IEqualityComparer<TKey>)) }
                    ));
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            return (IOrderedQueryable<TSource>) source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.OrderBy, source, keySelector),
                    new Expression[] { source.Expression, Expression.Quote(keySelector) }
                    ));
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey> comparer) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            return (IOrderedQueryable<TSource>) source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.OrderBy, source, keySelector, comparer),
                    new Expression[] { source.Expression, Expression.Quote(keySelector), Expression.Constant(comparer, typeof(IComparer<TKey>)) }
                    ));
        }

        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.OrderByDescending, source, keySelector),
                    new Expression[] { source.Expression, Expression.Quote(keySelector) }
                    ));
        }

        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey> comparer) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            return (IOrderedQueryable<TSource>) source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.OrderByDescending, source, keySelector, comparer),
                    new Expression[] { source.Expression, Expression.Quote(keySelector), Expression.Constant(comparer, typeof(IComparer<TKey>)) }
                    ));
        }

        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            return (IOrderedQueryable<TSource>) source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.ThenBy, source, keySelector),
                    new Expression[] { source.Expression, Expression.Quote(keySelector) }
                    ));
        }

        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey> comparer) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            return (IOrderedQueryable<TSource>) source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.ThenBy, source, keySelector, comparer),
                    new Expression[] { source.Expression, Expression.Quote(keySelector), Expression.Constant(comparer, typeof(IComparer<TKey>)) }
                    ));
        }

        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            return (IOrderedQueryable<TSource>) source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.ThenByDescending, source, keySelector),
                    new Expression[] { source.Expression, Expression.Quote(keySelector) }
                    ));
        }

        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey> comparer) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            return (IOrderedQueryable<TSource>) source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.ThenByDescending, source, keySelector, comparer),
                    new Expression[] { source.Expression, Expression.Quote(keySelector), Expression.Constant(comparer, typeof(IComparer<TKey>)) }
                    ));
        }

        public static IQueryable<TSource> Take<TSource>(this IQueryable<TSource> source, int count) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Take, source, count),
                    new Expression[] { source.Expression, Expression.Constant(count) }
                    ));
        }

        public static IQueryable<TSource> TakeWhile<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.TakeWhile, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static IQueryable<TSource> TakeWhile<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.TakeWhile, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static IQueryable<TSource> Skip<TSource>(this IQueryable<TSource> source, int count) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Skip, source, count),
                    new Expression[] { source.Expression, Expression.Constant(count) }
                    ));
        }

        public static IQueryable<TSource> SkipWhile<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.SkipWhile, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static IQueryable<TSource> SkipWhile<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.SkipWhile, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static IQueryable<IGrouping<TKey,TSource>> GroupBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            return source.Provider.CreateQuery<IGrouping<TKey,TSource>>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.GroupBy, source, keySelector),
                    new Expression[] { source.Expression, Expression.Quote(keySelector) }
                    ));
        }

        public static IQueryable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            if (elementSelector == null)
                throw Error.ArgumentNull("elementSelector");
            return source.Provider.CreateQuery<IGrouping<TKey,TElement>>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.GroupBy, source, keySelector, elementSelector),
                    new Expression[] { source.Expression, Expression.Quote(keySelector), Expression.Quote(elementSelector) }
                    ));
        }

        public static IQueryable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IEqualityComparer<TKey> comparer) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            return source.Provider.CreateQuery<IGrouping<TKey,TSource>>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.GroupBy, source, keySelector, comparer),
                    new Expression[] { source.Expression, Expression.Quote(keySelector), Expression.Constant(comparer, typeof(IEqualityComparer<TKey>)) }
                    ));
        }

        public static IQueryable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource,TElement>> elementSelector, IEqualityComparer<TKey> comparer) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            if (elementSelector == null)
                throw Error.ArgumentNull("elementSelector");
            return source.Provider.CreateQuery<IGrouping<TKey,TElement>>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.GroupBy, source, keySelector, elementSelector, comparer),
                    new Expression[] { source.Expression, Expression.Quote(keySelector), Expression.Quote(elementSelector), Expression.Constant(comparer, typeof(IEqualityComparer<TKey>)) }
                    ));
        }

        public static IQueryable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, Expression<Func<TKey, IEnumerable<TElement>, TResult>> resultSelector)
        {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            if (elementSelector == null)
                throw Error.ArgumentNull("elementSelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            return source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.GroupBy, source, keySelector, elementSelector, resultSelector),
                    new Expression[] { source.Expression, Expression.Quote(keySelector), Expression.Quote(elementSelector), Expression.Quote(resultSelector) }
                    ));
        }

        public static IQueryable<TResult> GroupBy<TSource, TKey, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector,Expression<Func<TKey, IEnumerable<TSource>, TResult>> resultSelector)
        {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            return source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.GroupBy, source, keySelector, resultSelector),
                    new Expression[] { source.Expression, Expression.Quote(keySelector), Expression.Quote(resultSelector) }
                    ));
        }

        public static IQueryable<TResult> GroupBy<TSource, TKey, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TKey, IEnumerable<TSource>, TResult>> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            return source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.GroupBy, source, keySelector, resultSelector, comparer),
                    new Expression[] { source.Expression, Expression.Quote(keySelector), Expression.Quote(resultSelector), Expression.Constant(comparer, typeof(IEqualityComparer<TKey>)) }
                    ));
        }

        public static IQueryable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, Expression<Func<TKey, IEnumerable<TElement>, TResult>> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (keySelector == null)
                throw Error.ArgumentNull("keySelector");
            if (elementSelector == null)
                throw Error.ArgumentNull("elementSelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            return source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.GroupBy, source, keySelector, elementSelector, resultSelector, comparer),
                    new Expression[] { source.Expression, Expression.Quote(keySelector), Expression.Quote(elementSelector), Expression.Quote(resultSelector), Expression.Constant(comparer, typeof(IEqualityComparer<TKey>)) }
                    ));
        }

        public static IQueryable<TSource> Distinct<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Distinct, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static IQueryable<TSource> Distinct<TSource>(this IQueryable<TSource> source, IEqualityComparer<TSource> comparer) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Distinct, source, comparer),
                    new Expression[] { source.Expression, Expression.Constant(comparer, typeof(IEqualityComparer<TSource>)) }
                    ));
        }

        public static IQueryable<TSource> Concat<TSource>(this IQueryable<TSource> source1, IEnumerable<TSource> source2) {
            if (source1 == null)
                throw Error.ArgumentNull("source1");
            if (source2 == null)
                throw Error.ArgumentNull("source2");
            return source1.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Concat, source1, source2),
                    new Expression[] { source1.Expression, GetSourceExpression(source2) }
                    ));
        }

        public static IQueryable<TResult> Zip<TFirst, TSecond, TResult>(this IQueryable<TFirst> source1, IEnumerable<TSecond> source2, Expression<Func<TFirst, TSecond, TResult>> resultSelector) {
            if (source1 == null)
                throw Error.ArgumentNull("source1");
            if (source2 == null)
                throw Error.ArgumentNull("source2");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            return source1.Provider.CreateQuery<TResult>( 
                Expression.Call(
                    null, 
                    GetMethodInfo(Queryable.Zip, source1, source2, resultSelector),
                    new Expression[] { source1.Expression, GetSourceExpression(source2), Expression.Quote(resultSelector) }
                    ));
        }

        public static IQueryable<TSource> Union<TSource>(this IQueryable<TSource> source1, IEnumerable<TSource> source2) {
            if (source1 == null)
                throw Error.ArgumentNull("source1");
            if (source2 == null)
                throw Error.ArgumentNull("source2");
            return source1.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Union, source1, source2),
                    new Expression[] { source1.Expression, GetSourceExpression(source2) }
                    ));
        }

        public static IQueryable<TSource> Union<TSource>(this IQueryable<TSource> source1, IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer) {
            if (source1 == null)
                throw Error.ArgumentNull("source1");
            if (source2 == null)
                throw Error.ArgumentNull("source2");
            return source1.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Union, source1, source2, comparer),
                    new Expression[] { 
                        source1.Expression, 
                        GetSourceExpression(source2),
                        Expression.Constant(comparer, typeof(IEqualityComparer<TSource>)) 
                        }
                    ));
        }

        public static IQueryable<TSource> Intersect<TSource>(this IQueryable<TSource> source1, IEnumerable<TSource> source2) {
            if (source1 == null)
                throw Error.ArgumentNull("source1");
            if (source2 == null)
                throw Error.ArgumentNull("source2");
            return source1.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null, 
                    GetMethodInfo(Queryable.Intersect, source1, source2),
                    new Expression[] { source1.Expression, GetSourceExpression(source2) }
                    ));
        }

        public static IQueryable<TSource> Intersect<TSource>(this IQueryable<TSource> source1, IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer) {
            if (source1 == null)
                throw Error.ArgumentNull("source1");
            if (source2 == null)
                throw Error.ArgumentNull("source2");
            return source1.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Intersect, source1, source2, comparer),
                    new Expression[] { 
                        source1.Expression, 
                        GetSourceExpression(source2),
                        Expression.Constant(comparer, typeof(IEqualityComparer<TSource>)) 
                        }
                    ));
        }

        public static IQueryable<TSource> Except<TSource>(this IQueryable<TSource> source1, IEnumerable<TSource> source2) {
            if (source1 == null)
                throw Error.ArgumentNull("source1");
            if (source2 == null)
                throw Error.ArgumentNull("source2");
            return source1.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null, 
                    GetMethodInfo(Queryable.Except, source1, source2),
                    new Expression[] { source1.Expression, GetSourceExpression(source2) }
                    ));
        }

        public static IQueryable<TSource> Except<TSource>(this IQueryable<TSource> source1, IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer) {
            if (source1 == null)
                throw Error.ArgumentNull("source1");
            if (source2 == null)
                throw Error.ArgumentNull("source2");
            return source1.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Except, source1, source2, comparer),
                    new Expression[] { 
                        source1.Expression, 
                        GetSourceExpression(source2),
                        Expression.Constant(comparer, typeof(IEqualityComparer<TSource>)) 
                        }
                    ));
        }

        public static TSource First<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.First, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TSource First<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.First, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource FirstOrDefault<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.FirstOrDefault, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TSource FirstOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.FirstOrDefault, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource Last<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Last, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TSource Last<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Last, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource LastOrDefault<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.LastOrDefault, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TSource LastOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.LastOrDefault, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource Single<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Single, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TSource Single<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Single, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource SingleOrDefault<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.SingleOrDefault, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TSource SingleOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.SingleOrDefault, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource ElementAt<TSource>(this IQueryable<TSource> source, int index) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (index < 0)
                throw Error.ArgumentOutOfRange("index");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.ElementAt, source, index),
                    new Expression[] { source.Expression, Expression.Constant(index) }
                    ));
        }

        public static TSource ElementAtOrDefault<TSource>(this IQueryable<TSource> source, int index) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.ElementAtOrDefault, source, index),
                    new Expression[] { source.Expression, Expression.Constant(index) }
                    ));
        }

        public static IQueryable<TSource> DefaultIfEmpty<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.DefaultIfEmpty, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static IQueryable<TSource> DefaultIfEmpty<TSource>(this IQueryable<TSource> source, TSource defaultValue) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.DefaultIfEmpty, source, defaultValue),
                    new Expression[] { source.Expression, Expression.Constant(defaultValue, typeof(TSource)) }
                    ));
        }

        public static bool Contains<TSource>(this IQueryable<TSource> source, TSource item) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<bool>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Contains, source, item),
                    new Expression[] { source.Expression, Expression.Constant(item, typeof(TSource)) }
                    ));
        }

        public static bool Contains<TSource>(this IQueryable<TSource> source, TSource item, IEqualityComparer<TSource> comparer) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<bool>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Contains, source, item, comparer),
                    new Expression[] { source.Expression, Expression.Constant(item, typeof(TSource)), Expression.Constant(comparer, typeof(IEqualityComparer<TSource>)) }
                    ));
        }

        public static IQueryable<TSource> Reverse<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.CreateQuery<TSource>( 
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Reverse, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static bool SequenceEqual<TSource>(this IQueryable<TSource> source1, IEnumerable<TSource> source2) {
            if (source1 == null)
                throw Error.ArgumentNull("source1");
            if (source2 == null)
                throw Error.ArgumentNull("source2");
            return source1.Provider.Execute<bool>(
                Expression.Call(
                    null, 
                    GetMethodInfo(Queryable.SequenceEqual, source1, source2),
                    new Expression[] { source1.Expression, GetSourceExpression(source2) }
                    ));
        }

        public static bool SequenceEqual<TSource>(this IQueryable<TSource> source1, IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer) {
            if (source1 == null)
                throw Error.ArgumentNull("source1");
            if (source2 == null)
                throw Error.ArgumentNull("source2");
            return source1.Provider.Execute<bool>(
                Expression.Call(
                    null, 
                    GetMethodInfo(Queryable.SequenceEqual, source1, source2, comparer),
                    new Expression[] { 
                        source1.Expression, 
                        GetSourceExpression(source2),
                        Expression.Constant(comparer, typeof(IEqualityComparer<TSource>)) 
                        }
                    ));
        }

        public static bool Any<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<bool>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Any, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static bool Any<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.Execute<bool>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Any, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static bool All<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.Execute<bool>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.All, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static int Count<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Count, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static int Count<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Count, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static long LongCount<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<long>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.LongCount, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static long LongCount<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            return source.Provider.Execute<long>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.LongCount, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource Min<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Min, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TResult Min<TSource,TResult>(this IQueryable<TSource> source, Expression<Func<TSource,TResult>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Min, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static TSource Max<TSource>(this IQueryable<TSource> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Max, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TResult Max<TSource,TResult>(this IQueryable<TSource> source, Expression<Func<TSource,TResult>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Max, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static int Sum(this IQueryable<int> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static int? Sum(this IQueryable<int?> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<int?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static long Sum(this IQueryable<long> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<long>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static long? Sum(this IQueryable<long?> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<long?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static float Sum(this IQueryable<float> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<float>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static float? Sum(this IQueryable<float?> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<float?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static double Sum(this IQueryable<double> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<double>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static double? Sum(this IQueryable<double?> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<double?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static decimal Sum(this IQueryable<decimal> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<decimal>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static decimal? Sum(this IQueryable<decimal?> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<decimal?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static int Sum<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,int>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static int? Sum<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,int?>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<int?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static long Sum<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,long>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<long>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static long? Sum<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,long?>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<long?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static float Sum<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,float>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<float>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static float? Sum<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,float?>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<float?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static double Sum<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,double>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<double>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static double? Sum<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,double?>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<double?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static decimal Sum<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,decimal>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<decimal>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static decimal? Sum<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,decimal?>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<decimal?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Sum, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static double Average(this IQueryable<int> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<double>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static double? Average(this IQueryable<int?> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<double?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static double Average(this IQueryable<long> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<double>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static double? Average(this IQueryable<long?> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<double?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static float Average(this IQueryable<float> source)
        {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<float>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static float? Average(this IQueryable<float?> source)
        {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<float?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static double Average(this IQueryable<double> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<double>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static double? Average(this IQueryable<double?> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<double?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static decimal Average(this IQueryable<decimal> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<decimal>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static decimal? Average(this IQueryable<decimal?> source) {
            if (source == null)
                throw Error.ArgumentNull("source");
            return source.Provider.Execute<decimal?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static double Average<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,int>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<double>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static double? Average<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,int?>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<double?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static float Average<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<float>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static float? Average<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<float?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static double Average<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,long>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<double>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static double? Average<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,long?>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<double?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static double Average<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,double>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<double>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static double? Average<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,double?>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<double?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static decimal Average<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,decimal>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<decimal>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static decimal? Average<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,decimal?>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<decimal?>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Average, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static TSource Aggregate<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,TSource,TSource>> func) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (func == null)
                throw Error.ArgumentNull("func");
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null, 
                    GetMethodInfo(Queryable.Aggregate, source, func),
                    new Expression[] { source.Expression, Expression.Quote(func) }
                    ));
        }

        public static TAccumulate Aggregate<TSource,TAccumulate>(this IQueryable<TSource> source, TAccumulate seed, Expression<Func<TAccumulate,TSource,TAccumulate>> func) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (func == null)
                throw Error.ArgumentNull("func");
            return source.Provider.Execute<TAccumulate>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Aggregate, source, seed, func),
                    new Expression[] { source.Expression, Expression.Constant(seed), Expression.Quote(func) }
                    ));
        }

        public static TResult Aggregate<TSource,TAccumulate,TResult>(this IQueryable<TSource> source, TAccumulate seed, Expression<Func<TAccumulate,TSource,TAccumulate>> func, Expression<Func<TAccumulate,TResult>> selector) {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (func == null)
                throw Error.ArgumentNull("func");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            return source.Provider.Execute<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Aggregate, source, seed, func, selector),
                    new Expression[] { source.Expression, Expression.Constant(seed), Expression.Quote(func), Expression.Quote(selector) }
                    ));
        }
    }
}
