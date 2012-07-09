// Type: System.Linq.Enumerable
// Assembly: System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Core.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using System.Threading;

namespace System.Linq
{
  [__DynamicallyInvokable]
  public static class Enumerable
  {
    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      if (source is Enumerable.Iterator<TSource>)
        return ((Enumerable.Iterator<TSource>) source).Where(predicate);
      if (source is TSource[])
        return (IEnumerable<TSource>) new Enumerable.WhereArrayIterator<TSource>((TSource[]) source, predicate);
      if (source is List<TSource>)
        return (IEnumerable<TSource>) new Enumerable.WhereListIterator<TSource>((List<TSource>) source, predicate);
      else
        return (IEnumerable<TSource>) new Enumerable.WhereEnumerableIterator<TSource>(source, predicate);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      else
        return Enumerable.WhereIterator<TSource>(source, predicate);
    }

    private static IEnumerable<TSource> WhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
    {
      int index = -1;
      foreach (TSource source1 in source)
      {
        checked { ++index; }
        if (predicate(source1, index))
          yield return source1;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (selector == null)
        throw Error.ArgumentNull("selector");
      if (source is Enumerable.Iterator<TSource>)
        return ((Enumerable.Iterator<TSource>) source).Select<TResult>(selector);
      if (source is TSource[])
        return (IEnumerable<TResult>) new Enumerable.WhereSelectArrayIterator<TSource, TResult>((TSource[]) source, (Func<TSource, bool>) null, selector);
      if (source is List<TSource>)
        return (IEnumerable<TResult>) new Enumerable.WhereSelectListIterator<TSource, TResult>((List<TSource>) source, (Func<TSource, bool>) null, selector);
      else
        return (IEnumerable<TResult>) new Enumerable.WhereSelectEnumerableIterator<TSource, TResult>(source, (Func<TSource, bool>) null, selector);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (selector == null)
        throw Error.ArgumentNull("selector");
      else
        return Enumerable.SelectIterator<TSource, TResult>(source, selector);
    }

    private static IEnumerable<TResult> SelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
    {
      int index = -1;
      foreach (TSource source1 in source)
      {
        checked { ++index; }
        yield return selector(source1, index);
      }
    }

    private static Func<TSource, bool> CombinePredicates<TSource>(Func<TSource, bool> predicate1, Func<TSource, bool> predicate2)
    {
      return (Func<TSource, bool>) (x =>
      {
        if (predicate1(x))
          return predicate2(x);
        else
          return false;
      });
    }

    private static Func<TSource, TResult> CombineSelectors<TSource, TMiddle, TResult>(Func<TSource, TMiddle> selector1, Func<TMiddle, TResult> selector2)
    {
      return (Func<TSource, TResult>) (x => selector2(selector1(x)));
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (selector == null)
        throw Error.ArgumentNull("selector");
      else
        return Enumerable.SelectManyIterator<TSource, TResult>(source, selector);
    }

    private static IEnumerable<TResult> SelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
    {
      foreach (TSource source1 in source)
      {
        foreach (TResult result in selector(source1))
          yield return result;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (selector == null)
        throw Error.ArgumentNull("selector");
      else
        return Enumerable.SelectManyIterator<TSource, TResult>(source, selector);
    }

    private static IEnumerable<TResult> SelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
    {
      int index = -1;
      foreach (TSource source1 in source)
      {
        checked { ++index; }
        foreach (TResult result in selector(source1, index))
          yield return result;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (collectionSelector == null)
        throw Error.ArgumentNull("collectionSelector");
      if (resultSelector == null)
        throw Error.ArgumentNull("resultSelector");
      else
        return Enumerable.SelectManyIterator<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
    }

    private static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
      int index = -1;
      foreach (TSource source1 in source)
      {
        checked { ++index; }
        foreach (TCollection collection in collectionSelector(source1, index))
          yield return resultSelector(source1, collection);
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (collectionSelector == null)
        throw Error.ArgumentNull("collectionSelector");
      if (resultSelector == null)
        throw Error.ArgumentNull("resultSelector");
      else
        return Enumerable.SelectManyIterator<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
    }

    private static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
      foreach (TSource source1 in source)
      {
        foreach (TCollection collection in collectionSelector(source1))
          yield return resultSelector(source1, collection);
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return Enumerable.TakeIterator<TSource>(source, count);
    }

    private static IEnumerable<TSource> TakeIterator<TSource>(IEnumerable<TSource> source, int count)
    {
      if (count > 0)
      {
        foreach (TSource source1 in source)
        {
          yield return source1;
          if (--count == 0)
            break;
        }
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      else
        return Enumerable.TakeWhileIterator<TSource>(source, predicate);
    }

    private static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      foreach (TSource source1 in source)
      {
        if (predicate(source1))
          yield return source1;
        else
          break;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      else
        return Enumerable.TakeWhileIterator<TSource>(source, predicate);
    }

    private static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
    {
      int index = -1;
      foreach (TSource source1 in source)
      {
        checked { ++index; }
        if (predicate(source1, index))
          yield return source1;
        else
          break;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return Enumerable.SkipIterator<TSource>(source, count);
    }

    private static IEnumerable<TSource> SkipIterator<TSource>(IEnumerable<TSource> source, int count)
    {
      using (IEnumerator<TSource> enumerator = source.GetEnumerator())
      {
        while (count > 0 && enumerator.MoveNext())
          --count;
        if (count <= 0)
        {
          while (enumerator.MoveNext())
            yield return enumerator.Current;
        }
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      else
        return Enumerable.SkipWhileIterator<TSource>(source, predicate);
    }

    private static IEnumerable<TSource> SkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      bool yielding = false;
      foreach (TSource source1 in source)
      {
        if (!yielding && !predicate(source1))
          yielding = true;
        if (yielding)
          yield return source1;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      else
        return Enumerable.SkipWhileIterator<TSource>(source, predicate);
    }

    private static IEnumerable<TSource> SkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
    {
      int index = -1;
      bool yielding = false;
      foreach (TSource source1 in source)
      {
        checked { ++index; }
        if (!yielding && !predicate(source1, index))
          yielding = true;
        if (yielding)
          yield return source1;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
    {
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
      else
        return Enumerable.JoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, (IEqualityComparer<TKey>) null);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
    {
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
      else
        return Enumerable.JoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
    }

    private static IEnumerable<TResult> JoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
    {
      Lookup<TKey, TInner> lookup = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
      foreach (TOuter outer1 in outer)
      {
        Lookup<TKey, TInner>.Grouping g = lookup.GetGrouping(outerKeySelector(outer1), false);
        if (g != null)
        {
          for (int i = 0; i < g.count; ++i)
            yield return resultSelector(outer1, g.elements[i]);
        }
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
    {
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
      else
        return Enumerable.GroupJoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, (IEqualityComparer<TKey>) null);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
    {
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
      else
        return Enumerable.GroupJoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
    }

    private static IEnumerable<TResult> GroupJoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
    {
      Lookup<TKey, TInner> lookup = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
      foreach (TOuter outer1 in outer)
        yield return resultSelector(outer1, lookup[outerKeySelector(outer1)]);
    }

    [__DynamicallyInvokable]
    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
      return (IOrderedEnumerable<TSource>) new OrderedEnumerable<TSource, TKey>(source, keySelector, (IComparer<TKey>) null, false);
    }

    [__DynamicallyInvokable]
    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
    {
      return (IOrderedEnumerable<TSource>) new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, false);
    }

    [__DynamicallyInvokable]
    public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
      return (IOrderedEnumerable<TSource>) new OrderedEnumerable<TSource, TKey>(source, keySelector, (IComparer<TKey>) null, true);
    }

    [__DynamicallyInvokable]
    public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
    {
      return (IOrderedEnumerable<TSource>) new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, true);
    }

    [__DynamicallyInvokable]
    public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return source.CreateOrderedEnumerable<TKey>(keySelector, (IComparer<TKey>) null, false);
    }

    [__DynamicallyInvokable]
    public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return source.CreateOrderedEnumerable<TKey>(keySelector, comparer, false);
    }

    [__DynamicallyInvokable]
    public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return source.CreateOrderedEnumerable<TKey>(keySelector, (IComparer<TKey>) null, true);
    }

    [__DynamicallyInvokable]
    public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return source.CreateOrderedEnumerable<TKey>(keySelector, comparer, true);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
      return (IEnumerable<IGrouping<TKey, TSource>>) new GroupedEnumerable<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, (IEqualityComparer<TKey>) null);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
      return (IEnumerable<IGrouping<TKey, TSource>>) new GroupedEnumerable<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
    {
      return (IEnumerable<IGrouping<TKey, TElement>>) new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, (IEqualityComparer<TKey>) null);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
    {
      return (IEnumerable<IGrouping<TKey, TElement>>) new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
    {
      return (IEnumerable<TResult>) new GroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, (IEqualityComparer<TKey>) null);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
    {
      return (IEnumerable<TResult>) new GroupedEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, (IEqualityComparer<TKey>) null);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
    {
      return (IEnumerable<TResult>) new GroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, comparer);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
    {
      return (IEnumerable<TResult>) new GroupedEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, comparer);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
      if (first == null)
        throw Error.ArgumentNull("first");
      if (second == null)
        throw Error.ArgumentNull("second");
      else
        return Enumerable.ConcatIterator<TSource>(first, second);
    }

    private static IEnumerable<TSource> ConcatIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
      foreach (TSource source in first)
        yield return source;
      foreach (TSource source in second)
        yield return source;
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
    {
      if (first == null)
        throw Error.ArgumentNull("first");
      if (second == null)
        throw Error.ArgumentNull("second");
      if (resultSelector == null)
        throw Error.ArgumentNull("resultSelector");
      else
        return Enumerable.ZipIterator<TFirst, TSecond, TResult>(first, second, resultSelector);
    }

    private static IEnumerable<TResult> ZipIterator<TFirst, TSecond, TResult>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
    {
      using (IEnumerator<TFirst> enumerator1 = first.GetEnumerator())
      {
        using (IEnumerator<TSecond> enumerator2 = second.GetEnumerator())
        {
          while (enumerator1.MoveNext() && enumerator2.MoveNext())
            yield return resultSelector(enumerator1.Current, enumerator2.Current);
        }
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return Enumerable.DistinctIterator<TSource>(source, (IEqualityComparer<TSource>) null);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return Enumerable.DistinctIterator<TSource>(source, comparer);
    }

    private static IEnumerable<TSource> DistinctIterator<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
    {
      Set<TSource> set = new Set<TSource>(comparer);
      foreach (TSource source1 in source)
      {
        if (set.Add(source1))
          yield return source1;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
      if (first == null)
        throw Error.ArgumentNull("first");
      if (second == null)
        throw Error.ArgumentNull("second");
      else
        return Enumerable.UnionIterator<TSource>(first, second, (IEqualityComparer<TSource>) null);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
    {
      if (first == null)
        throw Error.ArgumentNull("first");
      if (second == null)
        throw Error.ArgumentNull("second");
      else
        return Enumerable.UnionIterator<TSource>(first, second, comparer);
    }

    private static IEnumerable<TSource> UnionIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
    {
      Set<TSource> set = new Set<TSource>(comparer);
      foreach (TSource source in first)
      {
        if (set.Add(source))
          yield return source;
      }
      foreach (TSource source in second)
      {
        if (set.Add(source))
          yield return source;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
      if (first == null)
        throw Error.ArgumentNull("first");
      if (second == null)
        throw Error.ArgumentNull("second");
      else
        return Enumerable.IntersectIterator<TSource>(first, second, (IEqualityComparer<TSource>) null);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
    {
      if (first == null)
        throw Error.ArgumentNull("first");
      if (second == null)
        throw Error.ArgumentNull("second");
      else
        return Enumerable.IntersectIterator<TSource>(first, second, comparer);
    }

    private static IEnumerable<TSource> IntersectIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
    {
      Set<TSource> set = new Set<TSource>(comparer);
      foreach (TSource source in second)
        set.Add(source);
      foreach (TSource source in first)
      {
        if (set.Remove(source))
          yield return source;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
      if (first == null)
        throw Error.ArgumentNull("first");
      if (second == null)
        throw Error.ArgumentNull("second");
      else
        return Enumerable.ExceptIterator<TSource>(first, second, (IEqualityComparer<TSource>) null);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
    {
      if (first == null)
        throw Error.ArgumentNull("first");
      if (second == null)
        throw Error.ArgumentNull("second");
      else
        return Enumerable.ExceptIterator<TSource>(first, second, comparer);
    }

    private static IEnumerable<TSource> ExceptIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
    {
      Set<TSource> set = new Set<TSource>(comparer);
      foreach (TSource source in second)
        set.Add(source);
      foreach (TSource source in first)
      {
        if (set.Add(source))
          yield return source;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return Enumerable.ReverseIterator<TSource>(source);
    }

    private static IEnumerable<TSource> ReverseIterator<TSource>(IEnumerable<TSource> source)
    {
      Buffer<TSource> buffer = new Buffer<TSource>(source);
      for (int i = buffer.count - 1; i >= 0; --i)
        yield return buffer.items[i];
    }

    [__DynamicallyInvokable]
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
      return Enumerable.SequenceEqual<TSource>(first, second, (IEqualityComparer<TSource>) null);
    }

    [__DynamicallyInvokable]
    public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
    {
      if (comparer == null)
        comparer = (IEqualityComparer<TSource>) EqualityComparer<TSource>.Default;
      if (first == null)
        throw Error.ArgumentNull("first");
      if (second == null)
        throw Error.ArgumentNull("second");
      using (IEnumerator<TSource> enumerator1 = first.GetEnumerator())
      {
        using (IEnumerator<TSource> enumerator2 = second.GetEnumerator())
        {
          while (enumerator1.MoveNext())
          {
            if (!enumerator2.MoveNext() || !comparer.Equals(enumerator1.Current, enumerator2.Current))
              return false;
          }
          if (enumerator2.MoveNext())
            return false;
        }
      }
      return true;
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
    {
      return source;
    }

    [__DynamicallyInvokable]
    public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return new Buffer<TSource>(source).ToArray();
    }

    [__DynamicallyInvokable]
    public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return new List<TSource>(source);
    }

    [__DynamicallyInvokable]
    public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
      return Enumerable.ToDictionary<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, (IEqualityComparer<TKey>) null);
    }

    [__DynamicallyInvokable]
    public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
      return Enumerable.ToDictionary<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
    }

    [__DynamicallyInvokable]
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
    {
      return Enumerable.ToDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, (IEqualityComparer<TKey>) null);
    }

    [__DynamicallyInvokable]
    public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (keySelector == null)
        throw Error.ArgumentNull("keySelector");
      if (elementSelector == null)
        throw Error.ArgumentNull("elementSelector");
      Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(comparer);
      foreach (TSource source1 in source)
        dictionary.Add(keySelector(source1), elementSelector(source1));
      return dictionary;
    }

    [__DynamicallyInvokable]
    public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
      return (ILookup<TKey, TSource>) Lookup<TKey, TSource>.Create<TSource>(source, keySelector, IdentityFunction<TSource>.Instance, (IEqualityComparer<TKey>) null);
    }

    [__DynamicallyInvokable]
    public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
      return (ILookup<TKey, TSource>) Lookup<TKey, TSource>.Create<TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
    }

    [__DynamicallyInvokable]
    public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
    {
      return (ILookup<TKey, TElement>) Lookup<TKey, TElement>.Create<TSource>(source, keySelector, elementSelector, (IEqualityComparer<TKey>) null);
    }

    [__DynamicallyInvokable]
    public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
    {
      return (ILookup<TKey, TElement>) Lookup<TKey, TElement>.Create<TSource>(source, keySelector, elementSelector, comparer);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source)
    {
      return Enumerable.DefaultIfEmpty<TSource>(source, default (TSource));
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return Enumerable.DefaultIfEmptyIterator<TSource>(source, defaultValue);
    }

    private static IEnumerable<TSource> DefaultIfEmptyIterator<TSource>(IEnumerable<TSource> source, TSource defaultValue)
    {
      using (IEnumerator<TSource> enumerator = source.GetEnumerator())
      {
        if (enumerator.MoveNext())
        {
          do
          {
            yield return enumerator.Current;
          }
          while (enumerator.MoveNext());
        }
        else
          yield return defaultValue;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return Enumerable.OfTypeIterator<TResult>(source);
    }

    private static IEnumerable<TResult> OfTypeIterator<TResult>(IEnumerable source)
    {
      foreach (object obj in source)
      {
        if (obj is TResult)
          yield return (TResult) obj;
      }
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
    {
      IEnumerable<TResult> enumerable = source as IEnumerable<TResult>;
      if (enumerable != null)
        return enumerable;
      if (source == null)
        throw Error.ArgumentNull("source");
      else
        return Enumerable.CastIterator<TResult>(source);
    }

    private static IEnumerable<TResult> CastIterator<TResult>(IEnumerable source)
    {
      foreach (object obj in source)
        yield return (TResult) obj;
    }

    [__DynamicallyInvokable]
    public static TSource First<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      IList<TSource> list = source as IList<TSource>;
      if (list != null)
      {
        if (list.Count > 0)
          return list[0];
      }
      else
      {
        using (IEnumerator<TSource> enumerator = source.GetEnumerator())
        {
          if (enumerator.MoveNext())
            return enumerator.Current;
        }
      }
      throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      foreach (TSource source1 in source)
      {
        if (predicate(source1))
          return source1;
      }
      throw Error.NoMatch();
    }

    [__DynamicallyInvokable]
    public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      IList<TSource> list = source as IList<TSource>;
      if (list != null)
      {
        if (list.Count > 0)
          return list[0];
      }
      else
      {
        using (IEnumerator<TSource> enumerator = source.GetEnumerator())
        {
          if (enumerator.MoveNext())
            return enumerator.Current;
        }
      }
      return default (TSource);
    }

    [__DynamicallyInvokable]
    public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      foreach (TSource source1 in source)
      {
        if (predicate(source1))
          return source1;
      }
      return default (TSource);
    }

    [__DynamicallyInvokable]
    public static TSource Last<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      IList<TSource> list = source as IList<TSource>;
      if (list != null)
      {
        int count = list.Count;
        if (count > 0)
          return list[count - 1];
      }
      else
      {
        using (IEnumerator<TSource> enumerator = source.GetEnumerator())
        {
          if (enumerator.MoveNext())
          {
            TSource current;
            do
            {
              current = enumerator.Current;
            }
            while (enumerator.MoveNext());
            return current;
          }
        }
      }
      throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      TSource source1 = default (TSource);
      bool flag = false;
      foreach (TSource source2 in source)
      {
        if (predicate(source2))
        {
          source1 = source2;
          flag = true;
        }
      }
      if (flag)
        return source1;
      else
        throw Error.NoMatch();
    }

    [__DynamicallyInvokable]
    public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      IList<TSource> list = source as IList<TSource>;
      if (list != null)
      {
        int count = list.Count;
        if (count > 0)
          return list[count - 1];
      }
      else
      {
        using (IEnumerator<TSource> enumerator = source.GetEnumerator())
        {
          if (enumerator.MoveNext())
          {
            TSource current;
            do
            {
              current = enumerator.Current;
            }
            while (enumerator.MoveNext());
            return current;
          }
        }
      }
      return default (TSource);
    }

    [__DynamicallyInvokable]
    public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      TSource source1 = default (TSource);
      foreach (TSource source2 in source)
      {
        if (predicate(source2))
          source1 = source2;
      }
      return source1;
    }

    [__DynamicallyInvokable]
    public static TSource Single<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      IList<TSource> list = source as IList<TSource>;
      if (list != null)
      {
        switch (list.Count)
        {
          case 0:
            throw Error.NoElements();
          case 1:
            return list[0];
        }
      }
      else
      {
        using (IEnumerator<TSource> enumerator = source.GetEnumerator())
        {
          if (!enumerator.MoveNext())
            throw Error.NoElements();
          TSource current = enumerator.Current;
          if (!enumerator.MoveNext())
            return current;
        }
      }
      throw Error.MoreThanOneElement();
    }

    [__DynamicallyInvokable]
    public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      TSource source1 = default (TSource);
      long num = 0L;
      foreach (TSource source2 in source)
      {
        if (predicate(source2))
        {
          source1 = source2;
          checked { ++num; }
        }
      }
      switch (num)
      {
        case 0L:
          throw Error.NoMatch();
        case 1L:
          return source1;
        default:
          throw Error.MoreThanOneMatch();
      }
    }

    [__DynamicallyInvokable]
    public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      IList<TSource> list = source as IList<TSource>;
      if (list != null)
      {
        switch (list.Count)
        {
          case 0:
            return default (TSource);
          case 1:
            return list[0];
        }
      }
      else
      {
        using (IEnumerator<TSource> enumerator = source.GetEnumerator())
        {
          if (!enumerator.MoveNext())
            return default (TSource);
          TSource current = enumerator.Current;
          if (!enumerator.MoveNext())
            return current;
        }
      }
      throw Error.MoreThanOneElement();
    }

    [__DynamicallyInvokable]
    public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      TSource source1 = default (TSource);
      long num = 0L;
      foreach (TSource source2 in source)
      {
        if (predicate(source2))
        {
          source1 = source2;
          checked { ++num; }
        }
      }
      switch (num)
      {
        case 0L:
          return default (TSource);
        case 1L:
          return source1;
        default:
          throw Error.MoreThanOneMatch();
      }
    }

    [__DynamicallyInvokable]
    public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      IList<TSource> list = source as IList<TSource>;
      if (list != null)
        return list[index];
      if (index < 0)
        throw Error.ArgumentOutOfRange("index");
      using (IEnumerator<TSource> enumerator = source.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          if (index == 0)
            return enumerator.Current;
          --index;
        }
        throw Error.ArgumentOutOfRange("index");
      }
    }

    [__DynamicallyInvokable]
    public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (index >= 0)
      {
        IList<TSource> list = source as IList<TSource>;
        if (list != null)
        {
          if (index < list.Count)
            return list[index];
        }
        else
        {
          foreach (TSource source1 in source)
          {
            if (index == 0)
              return source1;
            --index;
          }
        }
      }
      return default (TSource);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<int> Range(int start, int count)
    {
      long num = (long) start + (long) count - 1L;
      if (count < 0 || num > (long) int.MaxValue)
        throw Error.ArgumentOutOfRange("count");
      else
        return Enumerable.RangeIterator(start, count);
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
    {
      if (count < 0)
        throw Error.ArgumentOutOfRange("count");
      else
        return Enumerable.RepeatIterator<TResult>(element, count);
    }

    private static IEnumerable<TResult> RepeatIterator<TResult>(TResult element, int count)
    {
      for (int i = 0; i < count; ++i)
        yield return element;
    }

    [__DynamicallyInvokable]
    public static IEnumerable<TResult> Empty<TResult>()
    {
      return EmptyEnumerable<TResult>.Instance;
    }

    [__DynamicallyInvokable]
    public static bool Any<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      using (IEnumerator<TSource> enumerator = source.GetEnumerator())
      {
        if (enumerator.MoveNext())
          return true;
      }
      return false;
    }

    [__DynamicallyInvokable]
    public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      foreach (TSource source1 in source)
      {
        if (predicate(source1))
          return true;
      }
      return false;
    }

    [__DynamicallyInvokable]
    public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      foreach (TSource source1 in source)
      {
        if (!predicate(source1))
          return false;
      }
      return true;
    }

    [__DynamicallyInvokable]
    public static int Count<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      ICollection<TSource> collection1 = source as ICollection<TSource>;
      if (collection1 != null)
        return collection1.Count;
      ICollection collection2 = source as ICollection;
      if (collection2 != null)
        return collection2.Count;
      int num = 0;
      using (IEnumerator<TSource> enumerator = source.GetEnumerator())
      {
        while (enumerator.MoveNext())
          checked { ++num; }
      }
      return num;
    }

    [__DynamicallyInvokable]
    public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      int num = 0;
      foreach (TSource source1 in source)
      {
        if (predicate(source1))
          checked { ++num; }
      }
      return num;
    }

    [__DynamicallyInvokable]
    public static long LongCount<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      long num = 0L;
      using (IEnumerator<TSource> enumerator = source.GetEnumerator())
      {
        while (enumerator.MoveNext())
          checked { ++num; }
      }
      return num;
    }

    [__DynamicallyInvokable]
    public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (predicate == null)
        throw Error.ArgumentNull("predicate");
      long num = 0L;
      foreach (TSource source1 in source)
      {
        if (predicate(source1))
          checked { ++num; }
      }
      return num;
    }

    [__DynamicallyInvokable]
    public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
    {
      ICollection<TSource> collection = source as ICollection<TSource>;
      if (collection != null)
        return collection.Contains(value);
      else
        return Enumerable.Contains<TSource>(source, value, (IEqualityComparer<TSource>) null);
    }

    [__DynamicallyInvokable]
    public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
    {
      if (comparer == null)
        comparer = (IEqualityComparer<TSource>) EqualityComparer<TSource>.Default;
      if (source == null)
        throw Error.ArgumentNull("source");
      foreach (TSource x in source)
      {
        if (comparer.Equals(x, value))
          return true;
      }
      return false;
    }

    [__DynamicallyInvokable]
    public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (func == null)
        throw Error.ArgumentNull("func");
      using (IEnumerator<TSource> enumerator = source.GetEnumerator())
      {
        if (!enumerator.MoveNext())
          throw Error.NoElements();
        TSource source1 = enumerator.Current;
        while (enumerator.MoveNext())
          source1 = func(source1, enumerator.Current);
        return source1;
      }
    }

    [__DynamicallyInvokable]
    public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (func == null)
        throw Error.ArgumentNull("func");
      TAccumulate accumulate = seed;
      foreach (TSource source1 in source)
        accumulate = func(accumulate, source1);
      return accumulate;
    }

    [__DynamicallyInvokable]
    public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      if (func == null)
        throw Error.ArgumentNull("func");
      if (resultSelector == null)
        throw Error.ArgumentNull("resultSelector");
      TAccumulate accumulate = seed;
      foreach (TSource source1 in source)
        accumulate = func(accumulate, source1);
      return resultSelector(accumulate);
    }

    [__DynamicallyInvokable]
    public static int Sum(this IEnumerable<int> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      int num1 = 0;
      foreach (int num2 in source)
        checked { num1 += num2; }
      return num1;
    }

    [__DynamicallyInvokable]
    public static int? Sum(this IEnumerable<int?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      int num = 0;
      foreach (int? nullable in source)
      {
        if (nullable.HasValue)
          checked { num += nullable.GetValueOrDefault(); }
      }
      return new int?(num);
    }

    [__DynamicallyInvokable]
    public static long Sum(this IEnumerable<long> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      long num1 = 0L;
      foreach (long num2 in source)
        checked { num1 += num2; }
      return num1;
    }

    [__DynamicallyInvokable]
    public static long? Sum(this IEnumerable<long?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      long num = 0L;
      foreach (long? nullable in source)
      {
        if (nullable.HasValue)
          checked { num += nullable.GetValueOrDefault(); }
      }
      return new long?(num);
    }

    [__DynamicallyInvokable]
    public static float Sum(this IEnumerable<float> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double num1 = 0.0;
      foreach (float num2 in source)
        num1 += (double) num2;
      return (float) num1;
    }

    [__DynamicallyInvokable]
    public static float? Sum(this IEnumerable<float?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double num = 0.0;
      foreach (float? nullable in source)
      {
        if (nullable.HasValue)
          num += (double) nullable.GetValueOrDefault();
      }
      return new float?((float) num);
    }

    [__DynamicallyInvokable]
    public static double Sum(this IEnumerable<double> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double num1 = 0.0;
      foreach (double num2 in source)
        num1 += num2;
      return num1;
    }

    [__DynamicallyInvokable]
    public static double? Sum(this IEnumerable<double?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double num = 0.0;
      foreach (double? nullable in source)
      {
        if (nullable.HasValue)
          num += nullable.GetValueOrDefault();
      }
      return new double?(num);
    }

    [__DynamicallyInvokable]
    public static Decimal Sum(this IEnumerable<Decimal> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      Decimal num1 = new Decimal(0);
      foreach (Decimal num2 in source)
        num1 += num2;
      return num1;
    }

    [__DynamicallyInvokable]
    public static Decimal? Sum(this IEnumerable<Decimal?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      Decimal num = new Decimal(0);
      foreach (Decimal? nullable in source)
      {
        if (nullable.HasValue)
          num += nullable.GetValueOrDefault();
      }
      return new Decimal?(num);
    }

    [__DynamicallyInvokable]
    public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
    {
      return Enumerable.Sum(Enumerable.Select<TSource, int>(source, selector));
    }

    [__DynamicallyInvokable]
    public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
    {
      return Enumerable.Sum(Enumerable.Select<TSource, int?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
    {
      return Enumerable.Sum(Enumerable.Select<TSource, long>(source, selector));
    }

    [__DynamicallyInvokable]
    public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
    {
      return Enumerable.Sum(Enumerable.Select<TSource, long?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
    {
      return Enumerable.Sum(Enumerable.Select<TSource, float>(source, selector));
    }

    [__DynamicallyInvokable]
    public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
    {
      return Enumerable.Sum(Enumerable.Select<TSource, float?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
    {
      return Enumerable.Sum(Enumerable.Select<TSource, double>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
    {
      return Enumerable.Sum(Enumerable.Select<TSource, double?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static Decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Decimal> selector)
    {
      return Enumerable.Sum(Enumerable.Select<TSource, Decimal>(source, selector));
    }

    [__DynamicallyInvokable]
    public static Decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Decimal?> selector)
    {
      return Enumerable.Sum(Enumerable.Select<TSource, Decimal?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static int Min(this IEnumerable<int> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      int num1 = 0;
      bool flag = false;
      foreach (int num2 in source)
      {
        if (flag)
        {
          if (num2 < num1)
            num1 = num2;
        }
        else
        {
          num1 = num2;
          flag = true;
        }
      }
      if (flag)
        return num1;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static int? Min(this IEnumerable<int?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      int? nullable1 = new int?();
      foreach (int? nullable2 in source)
      {
        if (nullable1.HasValue)
        {
          int? nullable3 = nullable2;
          int? nullable4 = nullable1;
          if ((nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault() ? 0 : (nullable3.HasValue & nullable4.HasValue ? 1 : 0)) == 0)
            continue;
        }
        nullable1 = nullable2;
      }
      return nullable1;
    }

    [__DynamicallyInvokable]
    public static long Min(this IEnumerable<long> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      long num1 = 0L;
      bool flag = false;
      foreach (long num2 in source)
      {
        if (flag)
        {
          if (num2 < num1)
            num1 = num2;
        }
        else
        {
          num1 = num2;
          flag = true;
        }
      }
      if (flag)
        return num1;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static long? Min(this IEnumerable<long?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      long? nullable1 = new long?();
      foreach (long? nullable2 in source)
      {
        if (nullable1.HasValue)
        {
          long? nullable3 = nullable2;
          long? nullable4 = nullable1;
          if ((nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault() ? 0 : (nullable3.HasValue & nullable4.HasValue ? 1 : 0)) == 0)
            continue;
        }
        nullable1 = nullable2;
      }
      return nullable1;
    }

    [__DynamicallyInvokable]
    public static float Min(this IEnumerable<float> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      float num = 0.0f;
      bool flag = false;
      foreach (float f in source)
      {
        if (flag)
        {
          if ((double) f < (double) num || float.IsNaN(f))
            num = f;
        }
        else
        {
          num = f;
          flag = true;
        }
      }
      if (flag)
        return num;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static float? Min(this IEnumerable<float?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      float? nullable1 = new float?();
      foreach (float? nullable2 in source)
      {
        if (nullable2.HasValue)
        {
          if (nullable1.HasValue)
          {
            float? nullable3 = nullable2;
            float? nullable4 = nullable1;
            if (((double) nullable3.GetValueOrDefault() >= (double) nullable4.GetValueOrDefault() ? 0 : (nullable3.HasValue & nullable4.HasValue ? 1 : 0)) == 0 && !float.IsNaN(nullable2.Value))
              continue;
          }
          nullable1 = nullable2;
        }
      }
      return nullable1;
    }

    [__DynamicallyInvokable]
    public static double Min(this IEnumerable<double> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double num = 0.0;
      bool flag = false;
      foreach (double d in source)
      {
        if (flag)
        {
          if (d < num || double.IsNaN(d))
            num = d;
        }
        else
        {
          num = d;
          flag = true;
        }
      }
      if (flag)
        return num;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static double? Min(this IEnumerable<double?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double? nullable1 = new double?();
      foreach (double? nullable2 in source)
      {
        if (nullable2.HasValue)
        {
          if (nullable1.HasValue)
          {
            double? nullable3 = nullable2;
            double? nullable4 = nullable1;
            if ((nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault() ? 0 : (nullable3.HasValue & nullable4.HasValue ? 1 : 0)) == 0 && !double.IsNaN(nullable2.Value))
              continue;
          }
          nullable1 = nullable2;
        }
      }
      return nullable1;
    }

    [__DynamicallyInvokable]
    public static Decimal Min(this IEnumerable<Decimal> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      Decimal num1 = new Decimal(0);
      bool flag = false;
      foreach (Decimal num2 in source)
      {
        if (flag)
        {
          if (num2 < num1)
            num1 = num2;
        }
        else
        {
          num1 = num2;
          flag = true;
        }
      }
      if (flag)
        return num1;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static Decimal? Min(this IEnumerable<Decimal?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      Decimal? nullable1 = new Decimal?();
      foreach (Decimal? nullable2 in source)
      {
        if (nullable1.HasValue)
        {
          Decimal? nullable3 = nullable2;
          Decimal? nullable4 = nullable1;
          if ((!(nullable3.GetValueOrDefault() < nullable4.GetValueOrDefault()) ? 0 : (nullable3.HasValue & nullable4.HasValue ? 1 : 0)) == 0)
            continue;
        }
        nullable1 = nullable2;
      }
      return nullable1;
    }

    [__DynamicallyInvokable]
    public static TSource Min<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      Comparer<TSource> @default = Comparer<TSource>.Default;
      TSource y = default (TSource);
      if ((object) y == null)
      {
        foreach (TSource x in source)
        {
          if ((object) x != null && ((object) y == null || @default.Compare(x, y) < 0))
            y = x;
        }
        return y;
      }
      else
      {
        bool flag = false;
        foreach (TSource x in source)
        {
          if (flag)
          {
            if (@default.Compare(x, y) < 0)
              y = x;
          }
          else
          {
            y = x;
            flag = true;
          }
        }
        if (flag)
          return y;
        else
          throw Error.NoElements();
      }
    }

    [__DynamicallyInvokable]
    public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
    {
      return Enumerable.Min(Enumerable.Select<TSource, int>(source, selector));
    }

    [__DynamicallyInvokable]
    public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
    {
      return Enumerable.Min(Enumerable.Select<TSource, int?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
    {
      return Enumerable.Min(Enumerable.Select<TSource, long>(source, selector));
    }

    [__DynamicallyInvokable]
    public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
    {
      return Enumerable.Min(Enumerable.Select<TSource, long?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
    {
      return Enumerable.Min(Enumerable.Select<TSource, float>(source, selector));
    }

    [__DynamicallyInvokable]
    public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
    {
      return Enumerable.Min(Enumerable.Select<TSource, float?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
    {
      return Enumerable.Min(Enumerable.Select<TSource, double>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
    {
      return Enumerable.Min(Enumerable.Select<TSource, double?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static Decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, Decimal> selector)
    {
      return Enumerable.Min(Enumerable.Select<TSource, Decimal>(source, selector));
    }

    [__DynamicallyInvokable]
    public static Decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, Decimal?> selector)
    {
      return Enumerable.Min(Enumerable.Select<TSource, Decimal?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
      return Enumerable.Min<TResult>(Enumerable.Select<TSource, TResult>(source, selector));
    }

    [__DynamicallyInvokable]
    public static int Max(this IEnumerable<int> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      int num1 = 0;
      bool flag = false;
      foreach (int num2 in source)
      {
        if (flag)
        {
          if (num2 > num1)
            num1 = num2;
        }
        else
        {
          num1 = num2;
          flag = true;
        }
      }
      if (flag)
        return num1;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static int? Max(this IEnumerable<int?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      int? nullable1 = new int?();
      foreach (int? nullable2 in source)
      {
        if (nullable1.HasValue)
        {
          int? nullable3 = nullable2;
          int? nullable4 = nullable1;
          if ((nullable3.GetValueOrDefault() <= nullable4.GetValueOrDefault() ? 0 : (nullable3.HasValue & nullable4.HasValue ? 1 : 0)) == 0)
            continue;
        }
        nullable1 = nullable2;
      }
      return nullable1;
    }

    [__DynamicallyInvokable]
    public static long Max(this IEnumerable<long> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      long num1 = 0L;
      bool flag = false;
      foreach (long num2 in source)
      {
        if (flag)
        {
          if (num2 > num1)
            num1 = num2;
        }
        else
        {
          num1 = num2;
          flag = true;
        }
      }
      if (flag)
        return num1;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static long? Max(this IEnumerable<long?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      long? nullable1 = new long?();
      foreach (long? nullable2 in source)
      {
        if (nullable1.HasValue)
        {
          long? nullable3 = nullable2;
          long? nullable4 = nullable1;
          if ((nullable3.GetValueOrDefault() <= nullable4.GetValueOrDefault() ? 0 : (nullable3.HasValue & nullable4.HasValue ? 1 : 0)) == 0)
            continue;
        }
        nullable1 = nullable2;
      }
      return nullable1;
    }

    [__DynamicallyInvokable]
    public static double Max(this IEnumerable<double> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double d = 0.0;
      bool flag = false;
      foreach (double num in source)
      {
        if (flag)
        {
          if (num > d || double.IsNaN(d))
            d = num;
        }
        else
        {
          d = num;
          flag = true;
        }
      }
      if (flag)
        return d;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static double? Max(this IEnumerable<double?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double? nullable1 = new double?();
      foreach (double? nullable2 in source)
      {
        if (nullable2.HasValue)
        {
          if (nullable1.HasValue)
          {
            double? nullable3 = nullable2;
            double? nullable4 = nullable1;
            if ((nullable3.GetValueOrDefault() <= nullable4.GetValueOrDefault() ? 0 : (nullable3.HasValue & nullable4.HasValue ? 1 : 0)) == 0 && !double.IsNaN(nullable1.Value))
              continue;
          }
          nullable1 = nullable2;
        }
      }
      return nullable1;
    }

    [__DynamicallyInvokable]
    public static float Max(this IEnumerable<float> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      float num1 = 0.0f;
      bool flag = false;
      foreach (float num2 in source)
      {
        if (flag)
        {
          if ((double) num2 > (double) num1 || double.IsNaN((double) num1))
            num1 = num2;
        }
        else
        {
          num1 = num2;
          flag = true;
        }
      }
      if (flag)
        return num1;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static float? Max(this IEnumerable<float?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      float? nullable1 = new float?();
      foreach (float? nullable2 in source)
      {
        if (nullable2.HasValue)
        {
          if (nullable1.HasValue)
          {
            float? nullable3 = nullable2;
            float? nullable4 = nullable1;
            if (((double) nullable3.GetValueOrDefault() <= (double) nullable4.GetValueOrDefault() ? 0 : (nullable3.HasValue & nullable4.HasValue ? 1 : 0)) == 0 && !float.IsNaN(nullable1.Value))
              continue;
          }
          nullable1 = nullable2;
        }
      }
      return nullable1;
    }

    [__DynamicallyInvokable]
    public static Decimal Max(this IEnumerable<Decimal> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      Decimal num1 = new Decimal(0);
      bool flag = false;
      foreach (Decimal num2 in source)
      {
        if (flag)
        {
          if (num2 > num1)
            num1 = num2;
        }
        else
        {
          num1 = num2;
          flag = true;
        }
      }
      if (flag)
        return num1;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static Decimal? Max(this IEnumerable<Decimal?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      Decimal? nullable1 = new Decimal?();
      foreach (Decimal? nullable2 in source)
      {
        if (nullable1.HasValue)
        {
          Decimal? nullable3 = nullable2;
          Decimal? nullable4 = nullable1;
          if ((!(nullable3.GetValueOrDefault() > nullable4.GetValueOrDefault()) ? 0 : (nullable3.HasValue & nullable4.HasValue ? 1 : 0)) == 0)
            continue;
        }
        nullable1 = nullable2;
      }
      return nullable1;
    }

    [__DynamicallyInvokable]
    public static TSource Max<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      Comparer<TSource> @default = Comparer<TSource>.Default;
      TSource y = default (TSource);
      if ((object) y == null)
      {
        foreach (TSource x in source)
        {
          if ((object) x != null && ((object) y == null || @default.Compare(x, y) > 0))
            y = x;
        }
        return y;
      }
      else
      {
        bool flag = false;
        foreach (TSource x in source)
        {
          if (flag)
          {
            if (@default.Compare(x, y) > 0)
              y = x;
          }
          else
          {
            y = x;
            flag = true;
          }
        }
        if (flag)
          return y;
        else
          throw Error.NoElements();
      }
    }

    [__DynamicallyInvokable]
    public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
    {
      return Enumerable.Max(Enumerable.Select<TSource, int>(source, selector));
    }

    [__DynamicallyInvokable]
    public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
    {
      return Enumerable.Max(Enumerable.Select<TSource, int?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
    {
      return Enumerable.Max(Enumerable.Select<TSource, long>(source, selector));
    }

    [__DynamicallyInvokable]
    public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
    {
      return Enumerable.Max(Enumerable.Select<TSource, long?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
    {
      return Enumerable.Max(Enumerable.Select<TSource, float>(source, selector));
    }

    [__DynamicallyInvokable]
    public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
    {
      return Enumerable.Max(Enumerable.Select<TSource, float?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
    {
      return Enumerable.Max(Enumerable.Select<TSource, double>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
    {
      return Enumerable.Max(Enumerable.Select<TSource, double?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static Decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, Decimal> selector)
    {
      return Enumerable.Max(Enumerable.Select<TSource, Decimal>(source, selector));
    }

    [__DynamicallyInvokable]
    public static Decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, Decimal?> selector)
    {
      return Enumerable.Max(Enumerable.Select<TSource, Decimal?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
      return Enumerable.Max<TResult>(Enumerable.Select<TSource, TResult>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double Average(this IEnumerable<int> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      long num1 = 0L;
      long num2 = 0L;
      foreach (int num3 in source)
      {
        checked { num1 += (long) num3; }
        checked { ++num2; }
      }
      if (num2 > 0L)
        return (double) num1 / (double) num2;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static double? Average(this IEnumerable<int?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      long num1 = 0L;
      long num2 = 0L;
      foreach (int? nullable in source)
      {
        if (nullable.HasValue)
        {
          checked { num1 += (long) nullable.GetValueOrDefault(); }
          checked { ++num2; }
        }
      }
      if (num2 > 0L)
        return new double?((double) num1 / (double) num2);
      else
        return new double?();
    }

    [__DynamicallyInvokable]
    public static double Average(this IEnumerable<long> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      long num1 = 0L;
      long num2 = 0L;
      foreach (long num3 in source)
      {
        checked { num1 += num3; }
        checked { ++num2; }
      }
      if (num2 > 0L)
        return (double) num1 / (double) num2;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static double? Average(this IEnumerable<long?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      long num1 = 0L;
      long num2 = 0L;
      foreach (long? nullable in source)
      {
        if (nullable.HasValue)
        {
          checked { num1 += nullable.GetValueOrDefault(); }
          checked { ++num2; }
        }
      }
      if (num2 > 0L)
        return new double?((double) num1 / (double) num2);
      else
        return new double?();
    }

    [__DynamicallyInvokable]
    public static float Average(this IEnumerable<float> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double num1 = 0.0;
      long num2 = 0L;
      foreach (float num3 in source)
      {
        num1 += (double) num3;
        checked { ++num2; }
      }
      if (num2 > 0L)
        return (float) num1 / (float) num2;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static float? Average(this IEnumerable<float?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double num1 = 0.0;
      long num2 = 0L;
      foreach (float? nullable in source)
      {
        if (nullable.HasValue)
        {
          num1 += (double) nullable.GetValueOrDefault();
          checked { ++num2; }
        }
      }
      if (num2 > 0L)
        return new float?((float) num1 / (float) num2);
      else
        return new float?();
    }

    [__DynamicallyInvokable]
    public static double Average(this IEnumerable<double> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double num1 = 0.0;
      long num2 = 0L;
      foreach (double num3 in source)
      {
        num1 += num3;
        checked { ++num2; }
      }
      if (num2 > 0L)
        return num1 / (double) num2;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static double? Average(this IEnumerable<double?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      double num1 = 0.0;
      long num2 = 0L;
      foreach (double? nullable in source)
      {
        if (nullable.HasValue)
        {
          num1 += nullable.GetValueOrDefault();
          checked { ++num2; }
        }
      }
      if (num2 > 0L)
        return new double?(num1 / (double) num2);
      else
        return new double?();
    }

    [__DynamicallyInvokable]
    public static Decimal Average(this IEnumerable<Decimal> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      Decimal num1 = new Decimal(0);
      long num2 = 0L;
      foreach (Decimal num3 in source)
      {
        num1 += num3;
        checked { ++num2; }
      }
      if (num2 > 0L)
        return num1 / (Decimal) num2;
      else
        throw Error.NoElements();
    }

    [__DynamicallyInvokable]
    public static Decimal? Average(this IEnumerable<Decimal?> source)
    {
      if (source == null)
        throw Error.ArgumentNull("source");
      Decimal num1 = new Decimal(0);
      long num2 = 0L;
      foreach (Decimal? nullable in source)
      {
        if (nullable.HasValue)
        {
          num1 += nullable.GetValueOrDefault();
          checked { ++num2; }
        }
      }
      if (num2 > 0L)
        return new Decimal?(num1 / (Decimal) num2);
      else
        return new Decimal?();
    }

    [__DynamicallyInvokable]
    public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
    {
      return Enumerable.Average(Enumerable.Select<TSource, int>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
    {
      return Enumerable.Average(Enumerable.Select<TSource, int?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
    {
      return Enumerable.Average(Enumerable.Select<TSource, long>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
    {
      return Enumerable.Average(Enumerable.Select<TSource, long?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
    {
      return Enumerable.Average(Enumerable.Select<TSource, float>(source, selector));
    }

    [__DynamicallyInvokable]
    public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
    {
      return Enumerable.Average(Enumerable.Select<TSource, float?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
    {
      return Enumerable.Average(Enumerable.Select<TSource, double>(source, selector));
    }

    [__DynamicallyInvokable]
    public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
    {
      return Enumerable.Average(Enumerable.Select<TSource, double?>(source, selector));
    }

    [__DynamicallyInvokable]
    public static Decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, Decimal> selector)
    {
      return Enumerable.Average(Enumerable.Select<TSource, Decimal>(source, selector));
    }

    [__DynamicallyInvokable]
    public static Decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, Decimal?> selector)
    {
      return Enumerable.Average(Enumerable.Select<TSource, Decimal?>(source, selector));
    }

    private static IEnumerable<int> RangeIterator(int start, int count)
    {
      for (int i = 0; i < count; ++i)
        yield return start + i;
    }

    private abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IDisposable, IEnumerator
    {
      private int threadId;
      internal int state;
      internal TSource current;

      public TSource Current
      {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] get
        {
          return this.current;
        }
      }

      object IEnumerator.Current
      {
        get
        {
          return (object) this.Current;
        }
      }

      public Iterator()
      {
        this.threadId = Thread.CurrentThread.ManagedThreadId;
      }

      public abstract Enumerable.Iterator<TSource> Clone();

      public virtual void Dispose()
      {
        this.current = default (TSource);
        this.state = -1;
      }

      public IEnumerator<TSource> GetEnumerator()
      {
        if (this.threadId == Thread.CurrentThread.ManagedThreadId && this.state == 0)
        {
          this.state = 1;
          return (IEnumerator<TSource>) this;
        }
        else
        {
          Enumerable.Iterator<TSource> iterator = this.Clone();
          iterator.state = 1;
          return (IEnumerator<TSource>) iterator;
        }
      }

      public abstract bool MoveNext();

      public abstract IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector);

      public abstract IEnumerable<TSource> Where(Func<TSource, bool> predicate);

      [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
      IEnumerator IEnumerable.GetEnumerator()
      {
        return (IEnumerator) this.GetEnumerator();
      }

      void IEnumerator.Reset()
      {
        throw new NotImplementedException();
      }
    }

    private class WhereEnumerableIterator<TSource> : Enumerable.Iterator<TSource>
    {
      private IEnumerable<TSource> source;
      private Func<TSource, bool> predicate;
      private IEnumerator<TSource> enumerator;

      [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
      public WhereEnumerableIterator(IEnumerable<TSource> source, Func<TSource, bool> predicate)
      {
        this.source = source;
        this.predicate = predicate;
      }

      public override Enumerable.Iterator<TSource> Clone()
      {
        return (Enumerable.Iterator<TSource>) new Enumerable.WhereEnumerableIterator<TSource>(this.source, this.predicate);
      }

      public override void Dispose()
      {
        if (this.enumerator != null)
          this.enumerator.Dispose();
        this.enumerator = (IEnumerator<TSource>) null;
        base.Dispose();
      }

      public override bool MoveNext()
      {
        switch (this.state)
        {
          case 1:
            this.enumerator = this.source.GetEnumerator();
            this.state = 2;
            goto case 2;
          case 2:
            while (this.enumerator.MoveNext())
            {
              TSource current = this.enumerator.Current;
              if (this.predicate(current))
              {
                this.current = current;
                return true;
              }
            }
            this.Dispose();
            break;
        }
        return false;
      }

      public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
      {
        return (IEnumerable<TResult>) new Enumerable.WhereSelectEnumerableIterator<TSource, TResult>(this.source, this.predicate, selector);
      }

      public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
      {
        return (IEnumerable<TSource>) new Enumerable.WhereEnumerableIterator<TSource>(this.source, Enumerable.CombinePredicates<TSource>(this.predicate, predicate));
      }
    }

    private class WhereArrayIterator<TSource> : Enumerable.Iterator<TSource>
    {
      private TSource[] source;
      private Func<TSource, bool> predicate;
      private int index;

      [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
      public WhereArrayIterator(TSource[] source, Func<TSource, bool> predicate)
      {
        this.source = source;
        this.predicate = predicate;
      }

      public override Enumerable.Iterator<TSource> Clone()
      {
        return (Enumerable.Iterator<TSource>) new Enumerable.WhereArrayIterator<TSource>(this.source, this.predicate);
      }

      public override bool MoveNext()
      {
        if (this.state == 1)
        {
          while (this.index < this.source.Length)
          {
            TSource source = this.source[this.index];
            ++this.index;
            if (this.predicate(source))
            {
              this.current = source;
              return true;
            }
          }
          this.Dispose();
        }
        return false;
      }

      public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
      {
        return (IEnumerable<TResult>) new Enumerable.WhereSelectArrayIterator<TSource, TResult>(this.source, this.predicate, selector);
      }

      public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
      {
        return (IEnumerable<TSource>) new Enumerable.WhereArrayIterator<TSource>(this.source, Enumerable.CombinePredicates<TSource>(this.predicate, predicate));
      }
    }

    private class WhereListIterator<TSource> : Enumerable.Iterator<TSource>
    {
      private List<TSource> source;
      private Func<TSource, bool> predicate;
      private List<TSource>.Enumerator enumerator;

      [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
      public WhereListIterator(List<TSource> source, Func<TSource, bool> predicate)
      {
        this.source = source;
        this.predicate = predicate;
      }

      public override Enumerable.Iterator<TSource> Clone()
      {
        return (Enumerable.Iterator<TSource>) new Enumerable.WhereListIterator<TSource>(this.source, this.predicate);
      }

      public override bool MoveNext()
      {
        switch (this.state)
        {
          case 1:
            this.enumerator = this.source.GetEnumerator();
            this.state = 2;
            goto case 2;
          case 2:
            while (this.enumerator.MoveNext())
            {
              TSource current = this.enumerator.Current;
              if (this.predicate(current))
              {
                this.current = current;
                return true;
              }
            }
            this.Dispose();
            break;
        }
        return false;
      }

      public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
      {
        return (IEnumerable<TResult>) new Enumerable.WhereSelectListIterator<TSource, TResult>(this.source, this.predicate, selector);
      }

      public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
      {
        return (IEnumerable<TSource>) new Enumerable.WhereListIterator<TSource>(this.source, Enumerable.CombinePredicates<TSource>(this.predicate, predicate));
      }
    }

    private class WhereSelectEnumerableIterator<TSource, TResult> : Enumerable.Iterator<TResult>
    {
      private IEnumerable<TSource> source;
      private Func<TSource, bool> predicate;
      private Func<TSource, TResult> selector;
      private IEnumerator<TSource> enumerator;

      [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
      public WhereSelectEnumerableIterator(IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
      {
        this.source = source;
        this.predicate = predicate;
        this.selector = selector;
      }

      public override Enumerable.Iterator<TResult> Clone()
      {
        return (Enumerable.Iterator<TResult>) new Enumerable.WhereSelectEnumerableIterator<TSource, TResult>(this.source, this.predicate, this.selector);
      }

      public override void Dispose()
      {
        if (this.enumerator != null)
          this.enumerator.Dispose();
        this.enumerator = (IEnumerator<TSource>) null;
        base.Dispose();
      }

      public override bool MoveNext()
      {
        switch (this.state)
        {
          case 1:
            this.enumerator = this.source.GetEnumerator();
            this.state = 2;
            goto case 2;
          case 2:
            while (this.enumerator.MoveNext())
            {
              TSource current = this.enumerator.Current;
              if (this.predicate == null || this.predicate(current))
              {
                this.current = this.selector(current);
                return true;
              }
            }
            this.Dispose();
            break;
        }
        return false;
      }

      public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
      {
        return (IEnumerable<TResult2>) new Enumerable.WhereSelectEnumerableIterator<TSource, TResult2>(this.source, this.predicate, Enumerable.CombineSelectors<TSource, TResult, TResult2>(this.selector, selector));
      }

      public override IEnumerable<TResult> Where(Func<TResult, bool> predicate)
      {
        return (IEnumerable<TResult>) new Enumerable.WhereEnumerableIterator<TResult>((IEnumerable<TResult>) this, predicate);
      }
    }

    private class WhereSelectArrayIterator<TSource, TResult> : Enumerable.Iterator<TResult>
    {
      private TSource[] source;
      private Func<TSource, bool> predicate;
      private Func<TSource, TResult> selector;
      private int index;

      [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
      public WhereSelectArrayIterator(TSource[] source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
      {
        this.source = source;
        this.predicate = predicate;
        this.selector = selector;
      }

      public override Enumerable.Iterator<TResult> Clone()
      {
        return (Enumerable.Iterator<TResult>) new Enumerable.WhereSelectArrayIterator<TSource, TResult>(this.source, this.predicate, this.selector);
      }

      public override bool MoveNext()
      {
        if (this.state == 1)
        {
          while (this.index < this.source.Length)
          {
            TSource source = this.source[this.index];
            ++this.index;
            if (this.predicate == null || this.predicate(source))
            {
              this.current = this.selector(source);
              return true;
            }
          }
          this.Dispose();
        }
        return false;
      }

      public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
      {
        return (IEnumerable<TResult2>) new Enumerable.WhereSelectArrayIterator<TSource, TResult2>(this.source, this.predicate, Enumerable.CombineSelectors<TSource, TResult, TResult2>(this.selector, selector));
      }

      public override IEnumerable<TResult> Where(Func<TResult, bool> predicate)
      {
        return (IEnumerable<TResult>) new Enumerable.WhereEnumerableIterator<TResult>((IEnumerable<TResult>) this, predicate);
      }
    }

    private class WhereSelectListIterator<TSource, TResult> : Enumerable.Iterator<TResult>
    {
      private List<TSource> source;
      private Func<TSource, bool> predicate;
      private Func<TSource, TResult> selector;
      private List<TSource>.Enumerator enumerator;

      [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
      public WhereSelectListIterator(List<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
      {
        this.source = source;
        this.predicate = predicate;
        this.selector = selector;
      }

      public override Enumerable.Iterator<TResult> Clone()
      {
        return (Enumerable.Iterator<TResult>) new Enumerable.WhereSelectListIterator<TSource, TResult>(this.source, this.predicate, this.selector);
      }

      public override bool MoveNext()
      {
        switch (this.state)
        {
          case 1:
            this.enumerator = this.source.GetEnumerator();
            this.state = 2;
            goto case 2;
          case 2:
            while (this.enumerator.MoveNext())
            {
              TSource current = this.enumerator.Current;
              if (this.predicate == null || this.predicate(current))
              {
                this.current = this.selector(current);
                return true;
              }
            }
            this.Dispose();
            break;
        }
        return false;
      }

      public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
      {
        return (IEnumerable<TResult2>) new Enumerable.WhereSelectListIterator<TSource, TResult2>(this.source, this.predicate, Enumerable.CombineSelectors<TSource, TResult, TResult2>(this.selector, selector));
      }

      public override IEnumerable<TResult> Where(Func<TResult, bool> predicate)
      {
        return (IEnumerable<TResult>) new Enumerable.WhereEnumerableIterator<TResult>((IEnumerable<TResult>) this, predicate);
      }
    }
  }
}
