using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

// Include Silverlight's managed resources
#if SILVERLIGHT
using System.Core;
#endif //SILVERLIGHT

namespace System.Linq
{
    public static class Enumerable
    {
        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            if (source is Iterator<TSource>) return ((Iterator<TSource>)source).Where(predicate);
            if (source is TSource[]) return new WhereArrayIterator<TSource>((TSource[])source, predicate);
            if (source is List<TSource>) return new WhereListIterator<TSource>((List<TSource>)source, predicate);
            return new WhereEnumerableIterator<TSource>(source, predicate);
        }

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            return WhereIterator<TSource>(source, predicate);
        }

        static IEnumerable<TSource> WhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate) {
            int index = -1;
            foreach (TSource element in source) {
                checked { index++; }
                if (predicate(element, index)) yield return element;
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (selector == null) throw Error.ArgumentNull("selector");
            if (source is Iterator<TSource>) return ((Iterator<TSource>)source).Select(selector);
            if (source is TSource[]) return new WhereSelectArrayIterator<TSource, TResult>((TSource[])source, null, selector);
            if (source is List<TSource>) return new WhereSelectListIterator<TSource, TResult>((List<TSource>)source, null, selector);
            return new WhereSelectEnumerableIterator<TSource, TResult>(source, null, selector);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (selector == null) throw Error.ArgumentNull("selector");
            return SelectIterator<TSource, TResult>(source, selector);
        }

        static IEnumerable<TResult> SelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector) {
            int index = -1;
            foreach (TSource element in source) {
                checked { index++; }
                yield return selector(element, index);
            }
        }

        static Func<TSource, bool> CombinePredicates<TSource>(Func<TSource, bool> predicate1, Func<TSource, bool> predicate2) {
            return x => predicate1(x) && predicate2(x);
        }

        static Func<TSource, TResult> CombineSelectors<TSource, TMiddle, TResult>(Func<TSource, TMiddle> selector1, Func<TMiddle, TResult> selector2) {
            return x => selector2(selector1(x));
        }

        abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerator<TSource>
        {
            int threadId;
            internal int state;
            internal TSource current;

            public Iterator() {
                threadId = Thread.CurrentThread.ManagedThreadId;
            }

            public TSource Current {
                get { return current; }
            }

            public abstract Iterator<TSource> Clone();

            public virtual void Dispose() {
                current = default(TSource);
                state = -1;
            }

            public IEnumerator<TSource> GetEnumerator() {
                if (threadId == Thread.CurrentThread.ManagedThreadId && state == 0) {
                    state = 1;
                    return this;
                }
                Iterator<TSource> duplicate = Clone();
                duplicate.state = 1;
                return duplicate;
            }

            public abstract bool MoveNext();

            public abstract IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector);

            public abstract IEnumerable<TSource> Where(Func<TSource, bool> predicate);

            object IEnumerator.Current {
                get { return Current; }
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            void IEnumerator.Reset() {
                throw new NotImplementedException();
            }
        }

        class WhereEnumerableIterator<TSource> : Iterator<TSource>
        {
            IEnumerable<TSource> source;
            Func<TSource, bool> predicate;
            IEnumerator<TSource> enumerator;

            public WhereEnumerableIterator(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
                this.source = source;
                this.predicate = predicate;
            }

            public override Iterator<TSource> Clone() {
                return new WhereEnumerableIterator<TSource>(source, predicate);
            }

            public override void Dispose() {
                if (enumerator is IDisposable) ((IDisposable)enumerator).Dispose();
                enumerator = null;
                base.Dispose();
            }

            public override bool MoveNext() {
                switch (state) {
                    case 1:
                        enumerator = source.GetEnumerator();
                        state = 2;
                        goto case 2;
                    case 2:
                        while (enumerator.MoveNext()) {
                            TSource item = enumerator.Current;
                            if (predicate(item)) {
                                current = item;
                                return true;
                            }
                        }
                        Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector) {
                return new WhereSelectEnumerableIterator<TSource, TResult>(source, predicate, selector);
            }

            public override IEnumerable<TSource> Where(Func<TSource, bool> predicate) {
                return new WhereEnumerableIterator<TSource>(source, CombinePredicates(this.predicate, predicate));
            }
        }

        class WhereArrayIterator<TSource> : Iterator<TSource>
        {
            TSource[] source;
            Func<TSource, bool> predicate;
            int index;

            public WhereArrayIterator(TSource[] source, Func<TSource, bool> predicate) {
                this.source = source;
                this.predicate = predicate;
            }

            public override Iterator<TSource> Clone() {
                return new WhereArrayIterator<TSource>(source, predicate);
            }

            public override bool MoveNext() {
                if (state == 1) {
                    while (index < source.Length) {
                        TSource item = source[index];
                        index++;
                        if (predicate(item)) {
                            current = item;
                            return true;
                        }
                    }
                    Dispose();
                }
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector) {
                return new WhereSelectArrayIterator<TSource, TResult>(source, predicate, selector);
            }

            public override IEnumerable<TSource> Where(Func<TSource, bool> predicate) {
                return new WhereArrayIterator<TSource>(source, CombinePredicates(this.predicate, predicate));
            }
        }

        class WhereListIterator<TSource> : Iterator<TSource>
        {
            List<TSource> source;
            Func<TSource, bool> predicate;
            List<TSource>.Enumerator enumerator;

            public WhereListIterator(List<TSource> source, Func<TSource, bool> predicate) {
                this.source = source;
                this.predicate = predicate;
            }

            public override Iterator<TSource> Clone() {
                return new WhereListIterator<TSource>(source, predicate);
            }

            public override bool MoveNext() {
                switch (state) {
                    case 1:
                        enumerator = source.GetEnumerator();
                        state = 2;
                        goto case 2;
                    case 2:
                        while (enumerator.MoveNext()) {
                            TSource item = enumerator.Current;
                            if (predicate(item)) {
                                current = item;
                                return true;
                            }
                        }
                        Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector) {
                return new WhereSelectListIterator<TSource, TResult>(source, predicate, selector);
            }

            public override IEnumerable<TSource> Where(Func<TSource, bool> predicate) {
                return new WhereListIterator<TSource>(source, CombinePredicates(this.predicate, predicate));
            }
        }

        class WhereSelectEnumerableIterator<TSource, TResult> : Iterator<TResult>
        {
            IEnumerable<TSource> source;
            Func<TSource, bool> predicate;
            Func<TSource, TResult> selector;
            IEnumerator<TSource> enumerator;

            public WhereSelectEnumerableIterator(IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector) {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Iterator<TResult> Clone() {
                return new WhereSelectEnumerableIterator<TSource, TResult>(source, predicate, selector);
            }

            public override void Dispose() {
                if (enumerator is IDisposable) ((IDisposable)enumerator).Dispose();
                enumerator = null;
                base.Dispose();
            }

            public override bool MoveNext() {
                switch (state) {
                    case 1:
                        enumerator = source.GetEnumerator();
                        state = 2;
                        goto case 2;
                    case 2:
                        while (enumerator.MoveNext()) {
                            TSource item = enumerator.Current;
                            if (predicate == null || predicate(item)) {
                                current = selector(item);
                                return true;
                            }
                        }
                        Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector) {
                return new WhereSelectEnumerableIterator<TSource, TResult2>(source, predicate, CombineSelectors(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(Func<TResult, bool> predicate) {
                return new WhereEnumerableIterator<TResult>(this, predicate);
            }
        }

        class WhereSelectArrayIterator<TSource, TResult> : Iterator<TResult>
        {
            TSource[] source;
            Func<TSource, bool> predicate;
            Func<TSource, TResult> selector;
            int index;

            public WhereSelectArrayIterator(TSource[] source, Func<TSource, bool> predicate, Func<TSource, TResult> selector) {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Iterator<TResult> Clone() {
                return new WhereSelectArrayIterator<TSource, TResult>(source, predicate, selector);
            }

            public override bool MoveNext() {
                if (state == 1) {
                    while (index < source.Length) {
                        TSource item = source[index];
                        index++;
                        if (predicate == null || predicate(item)) {
                            current = selector(item);
                            return true;
                        }
                    }
                    Dispose();
                }
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector) {
                return new WhereSelectArrayIterator<TSource, TResult2>(source, predicate, CombineSelectors(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(Func<TResult, bool> predicate) {
                return new WhereEnumerableIterator<TResult>(this, predicate);
            }
        }

        class WhereSelectListIterator<TSource, TResult> : Iterator<TResult>
        {
            List<TSource> source;
            Func<TSource, bool> predicate;
            Func<TSource, TResult> selector;
            List<TSource>.Enumerator enumerator;

            public WhereSelectListIterator(List<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector) {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Iterator<TResult> Clone() {
                return new WhereSelectListIterator<TSource, TResult>(source, predicate, selector);
            }

            public override bool MoveNext() {
                switch (state) {
                    case 1:
                        enumerator = source.GetEnumerator();
                        state = 2;
                        goto case 2;
                    case 2:
                        while (enumerator.MoveNext()) {
                            TSource item = enumerator.Current;
                            if (predicate == null || predicate(item)) {
                                current = selector(item);
                                return true;
                            }
                        }
                        Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector) {
                return new WhereSelectListIterator<TSource, TResult2>(source, predicate, CombineSelectors(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(Func<TResult, bool> predicate) {
                return new WhereEnumerableIterator<TResult>(this, predicate);
            }
        }

        //public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
        //    if (source == null) throw Error.ArgumentNull("source");
        //    if (predicate == null) throw Error.ArgumentNull("predicate");
        //    return WhereIterator<TSource>(source, predicate);
        //}

        //static IEnumerable<TSource> WhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
        //    foreach (TSource element in source) {
        //        if (predicate(element)) yield return element;
        //    }
        //}

        //public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) {
        //    if (source == null) throw Error.ArgumentNull("source");
        //    if (selector == null) throw Error.ArgumentNull("selector");
        //    return SelectIterator<TSource, TResult>(source, selector);
        //}

        //static IEnumerable<TResult> SelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector) {
        //    foreach (TSource element in source) {
        //        yield return selector(element);
        //    }
        //}

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (selector == null) throw Error.ArgumentNull("selector");
            return SelectManyIterator<TSource, TResult>(source, selector);
        }

        static IEnumerable<TResult> SelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) {
            foreach (TSource element in source) {
                foreach (TResult subElement in selector(element)) {
                    yield return subElement;
                }
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (selector == null) throw Error.ArgumentNull("selector");
            return SelectManyIterator<TSource, TResult>(source, selector);
        }

        static IEnumerable<TResult> SelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector) {
            int index = -1;
            foreach (TSource element in source) {
                checked { index++; }
                foreach (TResult subElement in selector(element, index)) {
                    yield return subElement;
                }
            }
        }
        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null) throw Error.ArgumentNull("source");
            if (collectionSelector == null) throw Error.ArgumentNull("collectionSelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return SelectManyIterator<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        }

        static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector){
            int index = -1;
            foreach (TSource element in source){
                checked { index++; }
                foreach (TCollection subElement in collectionSelector(element, index)){
                    yield return resultSelector(element, subElement);
                }
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (collectionSelector == null) throw Error.ArgumentNull("collectionSelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return SelectManyIterator<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        }

        static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) {
            foreach (TSource element in source) {
                foreach (TCollection subElement in collectionSelector(element)) {
                    yield return resultSelector(element, subElement);
                }
            }
        }

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count) {
            if (source == null) throw Error.ArgumentNull("source");
            return TakeIterator<TSource>(source, count);
        }

        static IEnumerable<TSource> TakeIterator<TSource>(IEnumerable<TSource> source, int count) {
            if (count > 0) {
                foreach (TSource element in source) {
                    yield return element;
                    if (--count == 0) break;
                }
            }
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            return TakeWhileIterator<TSource>(source, predicate);
        }

        static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (TSource element in source) {
                if (!predicate(element)) break;
                yield return element;
            }
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            return TakeWhileIterator<TSource>(source, predicate);
        }

        static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate) {
            int index = -1;
            foreach (TSource element in source) {
                checked { index++; }
                if (!predicate(element, index)) break;
                yield return element;
            }
        }

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count) {
            if (source == null) throw Error.ArgumentNull("source");
            return SkipIterator<TSource>(source, count);
        }

        static IEnumerable<TSource> SkipIterator<TSource>(IEnumerable<TSource> source, int count) {
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                while (count > 0 && e.MoveNext()) count--;
                if (count <= 0) {
                    while (e.MoveNext()) yield return e.Current;
                }
            }
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            return SkipWhileIterator<TSource>(source, predicate);
        }

        static IEnumerable<TSource> SkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            bool yielding = false;
            foreach (TSource element in source) {
                if (!yielding && !predicate(element)) yielding = true;
                if (yielding) yield return element;
            }
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            return SkipWhileIterator<TSource>(source, predicate);
        }

        static IEnumerable<TSource> SkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate) {
            int index = -1;
            bool yielding = false;
            foreach (TSource element in source) {
                checked { index++; }
                if (!yielding && !predicate(element, index)) yielding = true;
                if (yielding) yield return element;
            }
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector) {
            if (outer == null) throw Error.ArgumentNull("outer");
            if (inner == null) throw Error.ArgumentNull("inner");
            if (outerKeySelector == null) throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null) throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return JoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            if (outer == null) throw Error.ArgumentNull("outer");
            if (inner == null) throw Error.ArgumentNull("inner");
            if (outerKeySelector == null) throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null) throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return JoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        static IEnumerable<TResult> JoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            Lookup<TKey, TInner> lookup = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
            foreach (TOuter item in outer) {
                Lookup<TKey, TInner>.Grouping g = lookup.GetGrouping(outerKeySelector(item), false);
                if (g != null) {
                    for (int i = 0; i < g.count; i++) {
                        yield return resultSelector(item, g.elements[i]);
                    }
                }
            }
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector) {
            if (outer == null) throw Error.ArgumentNull("outer");
            if (inner == null) throw Error.ArgumentNull("inner");
            if (outerKeySelector == null) throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null) throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return GroupJoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            if (outer == null) throw Error.ArgumentNull("outer");
            if (inner == null) throw Error.ArgumentNull("inner");
            if (outerKeySelector == null) throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null) throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return GroupJoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        static IEnumerable<TResult> GroupJoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            Lookup<TKey, TInner> lookup = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
            foreach (TOuter item in outer) {
                yield return resultSelector(item, lookup[outerKeySelector(item)]);
            }
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, false);
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, false);
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, true);
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, true);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            if (source == null) throw Error.ArgumentNull("source");
            return source.CreateOrderedEnumerable<TKey>(keySelector, null, false);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            return source.CreateOrderedEnumerable<TKey>(keySelector, comparer, false);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            if (source == null) throw Error.ArgumentNull("source");
            return source.CreateOrderedEnumerable<TKey>(keySelector, null, true);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            return source.CreateOrderedEnumerable<TKey>(keySelector, comparer, true);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return new GroupedEnumerable<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            return new GroupedEnumerable<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) {
            return new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            return new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
        }

       public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector){
           return  new GroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector){
           return new GroupedEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer){
            return  new GroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, comparer);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer){
            return  new GroupedEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, comparer);
        }

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return ConcatIterator<TSource>(first, second);
        }

        static IEnumerable<TSource> ConcatIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second) {
            foreach (TSource element in first) yield return element;
            foreach (TSource element in second) yield return element;
        }

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return ZipIterator(first, second, resultSelector);
        }

        static IEnumerable<TResult> ZipIterator<TFirst, TSecond, TResult>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) {
            using (IEnumerator<TFirst> e1 = first.GetEnumerator())
                using (IEnumerator<TSecond> e2 = second.GetEnumerator())
                    while (e1.MoveNext() && e2.MoveNext())
                        yield return resultSelector(e1.Current, e2.Current);
        }


        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            return DistinctIterator<TSource>(source, null);
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            return DistinctIterator<TSource>(source, comparer);
        }

        static IEnumerable<TSource> DistinctIterator<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) {
            Set<TSource> set = new Set<TSource>(comparer);
            foreach (TSource element in source)
                if (set.Add(element)) yield return element;
        }

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return UnionIterator<TSource>(first, second, null);
        }

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return UnionIterator<TSource>(first, second, comparer);
        }

        static IEnumerable<TSource> UnionIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            Set<TSource> set = new Set<TSource>(comparer);
            foreach (TSource element in first)
                if (set.Add(element)) yield return element;
            foreach (TSource element in second)
                if (set.Add(element)) yield return element;
        }

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return IntersectIterator<TSource>(first, second, null);
        }

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return IntersectIterator<TSource>(first, second, comparer);
        }

        static IEnumerable<TSource> IntersectIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            Set<TSource> set = new Set<TSource>(comparer);
            foreach (TSource element in second) set.Add(element);
            foreach (TSource element in first)
                if (set.Remove(element)) yield return element;
        }

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return ExceptIterator<TSource>(first, second, null);
        }

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return ExceptIterator<TSource>(first, second, comparer);
        }

        static IEnumerable<TSource> ExceptIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) {
            Set<TSource> set = new Set<TSource>(comparer);
            foreach (TSource element in second) set.Add(element);
            foreach (TSource element in first)
                if (set.Add(element)) yield return element;
        }

        public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            return ReverseIterator<TSource>(source);
        }

        static IEnumerable<TSource> ReverseIterator<TSource>(IEnumerable<TSource> source) {
            Buffer<TSource> buffer = new Buffer<TSource>(source);
            for (int i = buffer.count - 1; i >= 0; i--) yield return buffer.items[i];
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) {
            return SequenceEqual<TSource>(first, second, null);
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (comparer == null) comparer = EqualityComparer<TSource>.Default;
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            using (IEnumerator<TSource> e1 = first.GetEnumerator())
            using (IEnumerator<TSource> e2 = second.GetEnumerator())
            {
                while (e1.MoveNext())
                {
                    if (!(e2.MoveNext() && comparer.Equals(e1.Current, e2.Current))) return false;
                }
                if (e2.MoveNext()) return false;
            }
            return true;
        }

        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
        {
            return source;
        }

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            return new Buffer<TSource>(source).ToArray();
        }

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            return new List<TSource>(source);
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return ToDictionary<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            return ToDictionary<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) {
            return ToDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            if (keySelector == null) throw Error.ArgumentNull("keySelector");
            if (elementSelector == null) throw Error.ArgumentNull("elementSelector");
            Dictionary<TKey, TElement> d = new Dictionary<TKey, TElement>(comparer);
            foreach (TSource element in source) d.Add(keySelector(element), elementSelector(element));
            return d;
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return Lookup<TKey, TSource>.Create(source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            return Lookup<TKey, TSource>.Create(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) {
            return Lookup<TKey, TElement>.Create(source, keySelector, elementSelector, null);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            return Lookup<TKey, TElement>.Create(source, keySelector, elementSelector, comparer);
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source) {
            return DefaultIfEmpty(source, default(TSource));
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue) {
            if (source == null) throw Error.ArgumentNull("source");
            return DefaultIfEmptyIterator<TSource>(source, defaultValue);
        }

        static IEnumerable<TSource> DefaultIfEmptyIterator<TSource>(IEnumerable<TSource> source, TSource defaultValue) {
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                if (e.MoveNext()) {
                    do {
                        yield return e.Current;
                    } while (e.MoveNext());
                }
                else {
                    yield return defaultValue;
                }
            }
        }

        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source) {
            if (source == null) throw Error.ArgumentNull("source");
            return OfTypeIterator<TResult>(source);
        }

        static IEnumerable<TResult> OfTypeIterator<TResult>(IEnumerable source) {
            foreach (object obj in source) {
                if (obj is TResult) yield return (TResult)obj;
            }
        }

        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source) {
            IEnumerable<TResult> typedSource = source as IEnumerable<TResult>;
            if (typedSource != null) return typedSource;
            if (source == null) throw Error.ArgumentNull("source");
            return CastIterator<TResult>(source);
        }

        static IEnumerable<TResult> CastIterator<TResult>(IEnumerable source) {
            foreach (object obj in source) yield return (TResult)obj;
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                if (list.Count > 0) return list[0];
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (e.MoveNext()) return e.Current;
                }
            }
            throw Error.NoElements();
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            foreach (TSource element in source) {
                if (predicate(element)) return element;
            }
            throw Error.NoMatch();
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                if (list.Count > 0) return list[0];
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (e.MoveNext()) return e.Current;
                }
            }
            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            foreach (TSource element in source) {
                if (predicate(element)) return element;
            }
            return default(TSource);
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                int count = list.Count;
                if (count > 0) return list[count - 1];
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (e.MoveNext()) {
                        TSource result;
                        do {
                            result = e.Current;
                        } while (e.MoveNext());
                        return result;
                    }
                }
            }
            throw Error.NoElements();
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            TSource result = default(TSource);
            bool found = false;
            foreach (TSource element in source) {
                if (predicate(element)) {
                    result = element;
                    found = true;
                }
            }
            if (found) return result;
            throw Error.NoMatch();
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                int count = list.Count;
                if (count > 0) return list[count - 1];
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (e.MoveNext()) {
                        TSource result;
                        do {
                            result = e.Current;
                        } while (e.MoveNext());
                        return result;
                    }
                }
            }
            return default(TSource);
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            TSource result = default(TSource);
            foreach (TSource element in source) {
                if (predicate(element)) {
                    result = element;
                }
            }
            return result;
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                switch (list.Count) {
                    case 0: throw Error.NoElements();
                    case 1: return list[0];
                }
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (!e.MoveNext()) throw Error.NoElements();
                    TSource result = e.Current;
                    if (!e.MoveNext()) return result;
                }
            }
            throw Error.MoreThanOneElement();
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            TSource result = default(TSource);
            long count = 0;
            foreach (TSource element in source) {
                if (predicate(element)) {
                    result = element;
                    checked { count++; }
                }
            }
            switch (count) {
                case 0: throw Error.NoMatch();
                case 1: return result;
            }
            throw Error.MoreThanOneMatch();
        }

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                switch (list.Count) {
                    case 0: return default(TSource);
                    case 1: return list[0];
                }
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (!e.MoveNext()) return default(TSource);
                    TSource result = e.Current;
                    if (!e.MoveNext()) return result;
                }
            }
            throw Error.MoreThanOneElement();
        }

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            TSource result = default(TSource);
            long count = 0;
            foreach (TSource element in source) {
                if (predicate(element)) {
                    result = element;
                    checked { count++; }
                }
            }
            switch (count) {
                case 0: return default(TSource);
                case 1: return result;
            }
            throw Error.MoreThanOneMatch();
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) return list[index];
            if (index < 0) throw Error.ArgumentOutOfRange("index");
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                while (true) {
                    if (!e.MoveNext()) throw Error.ArgumentOutOfRange("index");
                    if (index == 0) return e.Current;
                    index--;
                }
            }
        }

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index) {
            if (source == null) throw Error.ArgumentNull("source");
            if (index >= 0) {
                IList<TSource> list = source as IList<TSource>;
                if (list != null) {
                    if (index < list.Count) return list[index];
                }
                else {
                    using (IEnumerator<TSource> e = source.GetEnumerator()) {
                        while (true) {
                            if (!e.MoveNext()) break;
                            if (index == 0) return e.Current;
                            index--;
                        }
                    }
                }
            }
            return default(TSource);
        }

        public static IEnumerable<int> Range(int start, int count) {
            long max = ((long)start) + count - 1;
            if (count < 0 || max > Int32.MaxValue) throw Error.ArgumentOutOfRange("count");
            return RangeIterator(start, count);
        }

        static IEnumerable<int> RangeIterator(int start, int count) {
            for (int i = 0; i < count; i++) yield return start + i;
        }

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count) {
            if (count < 0) throw Error.ArgumentOutOfRange("count");
            return RepeatIterator<TResult>(element, count);
        }

        static IEnumerable<TResult> RepeatIterator<TResult>(TResult element, int count) {
            for (int i = 0; i < count; i++) yield return element;
        }

        public static IEnumerable<TResult> Empty<TResult>() {
            return EmptyEnumerable<TResult>.Instance;
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                if (e.MoveNext()) return true;
            }
            return false;
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            foreach (TSource element in source) {
                if (predicate(element)) return true;
            }
            return false;
        }

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            foreach (TSource element in source) {
                if (!predicate(element)) return false;
            }
            return true;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            ICollection<TSource> collectionoft = source as ICollection<TSource>;
            if (collectionoft != null) return collectionoft.Count;
            ICollection collection = source as ICollection;
            if (collection != null) return collection.Count;
            int count = 0;
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                checked {
                    while (e.MoveNext()) count++;
                }
            }
            return count;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            int count = 0;
            foreach (TSource element in source) {
                checked {
                    if (predicate(element)) count++;
                }
            }
            return count;
        }

        public static long LongCount<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long count = 0;
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                checked {
                    while (e.MoveNext()) count++;
                }
            }
            return count;
        }

        public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            long count = 0;
            foreach (TSource element in source) {
                checked {
                    if (predicate(element)) count++;
                }
            }
            return count;
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value) {
            ICollection<TSource> collection = source as ICollection<TSource>;
            if (collection != null) return collection.Contains(value);
            return Contains<TSource>(source, value, null);
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            if (comparer == null) comparer = EqualityComparer<TSource>.Default;
            if (source == null) throw Error.ArgumentNull("source");
            foreach (TSource element in source)
                if (comparer.Equals(element, value)) return true;
            return false;
        }

        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            if (source == null) throw Error.ArgumentNull("source");
            if (func == null) throw Error.ArgumentNull("func");
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw Error.NoElements();
                TSource result = e.Current;
                while (e.MoveNext()) result = func(result, e.Current);
                return result;
            }
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) {
            if (source == null) throw Error.ArgumentNull("source");
            if (func == null) throw Error.ArgumentNull("func");
            TAccumulate result = seed;
            foreach (TSource element in source) result = func(result, element);
            return result;
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (func == null) throw Error.ArgumentNull("func");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            TAccumulate result = seed;
            foreach (TSource element in source) result = func(result, element);
            return resultSelector(result);
        }

        public static int Sum(this IEnumerable<int> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int sum = 0;
            checked {
                foreach (int v in source) sum += v;
            }
            return sum;
        }

        public static int? Sum(this IEnumerable<int?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int sum = 0;
            checked {
                foreach (int? v in source) {
                    if (v != null) sum += v.GetValueOrDefault();
                }
            }
            return sum;
        }

        public static long Sum(this IEnumerable<long> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            checked {
                foreach (long v in source) sum += v;
            }
            return sum;
        }

        public static long? Sum(this IEnumerable<long?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            checked {
                foreach (long? v in source) {
                    if (v != null) sum += v.GetValueOrDefault();
                }
            }
            return sum;
        }

        public static float Sum(this IEnumerable<float> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            foreach (float v in source) sum += v;
            return (float)sum;
        }

        public static float? Sum(this IEnumerable<float?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            foreach (float? v in source) {
                if (v != null) sum += v.GetValueOrDefault();
            }
            return (float)sum;
        }

        public static double Sum(this IEnumerable<double> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            foreach (double v in source) sum += v;
            return sum;
        }

        public static double? Sum(this IEnumerable<double?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            foreach (double? v in source) {
                if (v != null) sum += v.GetValueOrDefault();
            }
            return sum;
        }

        public static decimal Sum(this IEnumerable<decimal> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal sum = 0;
            foreach (decimal v in source) sum += v;
            return sum;
        }

        public static decimal? Sum(this IEnumerable<decimal?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal sum = 0;
            foreach (decimal? v in source) {
                if (v != null) sum += v.GetValueOrDefault();
            }
            return sum;
        }

        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) {
            return Enumerable.Sum(Enumerable.Select(source, selector));
        }

        public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) {
            return Enumerable.Sum(Enumerable.Select(source, selector));
        }

        public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) {
            return Enumerable.Sum(Enumerable.Select(source, selector));
        }

        public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) {
            return Enumerable.Sum(Enumerable.Select(source, selector));
        }

        public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) {
            return Enumerable.Sum(Enumerable.Select(source, selector));
        }

        public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) {
            return Enumerable.Sum(Enumerable.Select(source, selector));
        }

        public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) {
            return Enumerable.Sum(Enumerable.Select(source, selector));
        }

        public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) {
            return Enumerable.Sum(Enumerable.Select(source, selector));
        }

        public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) {
            return Enumerable.Sum(Enumerable.Select(source, selector));
        }

        public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) {
            return Enumerable.Sum(Enumerable.Select(source, selector));
        }

        public static int Min(this IEnumerable<int> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int value = 0;
            bool hasValue = false;
            foreach (int x in source) {
                if (hasValue) {
                    if (x < value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static int? Min(this IEnumerable<int?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int? value = null;
            foreach (int? x in source) {
                if (value == null || x < value)
                    value = x;
            }
            return value;
        }

        public static long Min(this IEnumerable<long> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long value = 0;
            bool hasValue = false;
            foreach (long x in source) {
                if (hasValue) {
                    if (x < value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static long? Min(this IEnumerable<long?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long? value = null;
            foreach (long? x in source) {
                if (value == null || x < value) value = x;
            }
            return value;
        }

        public static float Min(this IEnumerable<float> source) {
            if (source == null) throw Error.ArgumentNull("source");
            float value = 0;
            bool hasValue = false;
            foreach (float x in source) {
                if (hasValue) {
                    // Normally NaN < anything is false, as is anything < NaN
                    // However, this leads to some irksome outcomes in Min and Max.
                    // If we use those semantics then Min(NaN, 5.0) is NaN, but
                    // Min(5.0, NaN) is 5.0!  To fix this, we impose a total
                    // ordering where NaN is smaller than every value, including
                    // negative infinity.
                    if (x < value || System.Single.IsNaN(x)) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static float? Min(this IEnumerable<float?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            float? value = null;
            foreach (float? x in source) {
                if (x == null) continue;
                if (value == null || x < value || System.Single.IsNaN((float)x)) value = x;
            }
            return value;
        }

        public static double Min(this IEnumerable<double> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double value = 0;
            bool hasValue = false;
            foreach (double x in source) {
                if (hasValue) {
                    if (x < value || System.Double.IsNaN(x)) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static double? Min(this IEnumerable<double?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double? value = null;
            foreach (double? x in source) {
                if (x == null) continue;
                if (value == null || x < value || System.Double.IsNaN((double)x)) value = x;
            }
            return value;
        }

        public static decimal Min(this IEnumerable<decimal> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal value = 0;
            bool hasValue = false;
            foreach (decimal x in source) {
                if (hasValue) {
                    if (x < value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static decimal? Min(this IEnumerable<decimal?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal? value = null;
            foreach (decimal? x in source) {
                if (value == null || x < value) value = x;
            }
            return value;
        }

        public static TSource Min<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            Comparer<TSource> comparer = Comparer<TSource>.Default;
            TSource value = default(TSource);
            if (value == null) {
                foreach (TSource x in source) {
                    if (x != null && (value == null || comparer.Compare(x, value) < 0))
                        value = x;
                }
                return value;
            }
            else {
                bool hasValue = false;
                foreach (TSource x in source) {
                    if (hasValue) {
                        if (comparer.Compare(x, value) < 0)
                            value = x;
                    }
                    else {
                        value = x;
                        hasValue = true;
                    }
                }
                if (hasValue) return value;
                throw Error.NoElements();
            }
        }

        public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) {
            return Enumerable.Min(Enumerable.Select(source, selector));
        }

        public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) {
            return Enumerable.Min(Enumerable.Select(source, selector));
        }

        public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) {
            return Enumerable.Min(Enumerable.Select(source, selector));
        }

        public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) {
            return Enumerable.Min(Enumerable.Select(source, selector));
        }

        public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) {
            return Enumerable.Min(Enumerable.Select(source, selector));
        }

        public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) {
            return Enumerable.Min(Enumerable.Select(source, selector));
        }

        public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) {
            return Enumerable.Min(Enumerable.Select(source, selector));
        }

        public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) {
            return Enumerable.Min(Enumerable.Select(source, selector));
        }

        public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) {
            return Enumerable.Min(Enumerable.Select(source, selector));
        }

        public static decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) {
            return Enumerable.Min(Enumerable.Select(source, selector));
        }

        public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            return Enumerable.Min(Enumerable.Select(source, selector));
        }

        public static int Max(this IEnumerable<int> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int value = 0;
            bool hasValue = false;
            foreach (int x in source) {
                if (hasValue) {
                    if (x > value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static int? Max(this IEnumerable<int?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int? value = null;
            foreach (int? x in source) {
                if (value == null || x > value) value = x;
            }
            return value;
        }

        public static long Max(this IEnumerable<long> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long value = 0;
            bool hasValue = false;
            foreach (long x in source) {
                if (hasValue) {
                    if (x > value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static long? Max(this IEnumerable<long?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long? value = null;
            foreach (long? x in source) {
                if (value == null || x > value) value = x;
            }
            return value;
        }

        public static double Max(this IEnumerable<double> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double value = 0;
            bool hasValue = false;
            foreach (double x in source) {
                if (hasValue) {
                    if (x > value || System.Double.IsNaN(value)) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static double? Max(this IEnumerable<double?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double? value = null;
            foreach (double? x in source) {
                if (x == null) continue;
                if (value == null || x > value || System.Double.IsNaN((double)value)) value = x;
            }
            return value;
        }

        public static float Max(this IEnumerable<float> source) {
            if (source == null) throw Error.ArgumentNull("source");
            float value = 0;
            bool hasValue = false;
            foreach (float x in source) {
                if (hasValue) {
                    if (x > value || System.Double.IsNaN(value)) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static float? Max(this IEnumerable<float?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            float? value = null;
            foreach (float? x in source) {
                if (x == null) continue;
                if (value == null || x > value || System.Single.IsNaN((float)value)) value = x;
            }
            return value;
        }

        public static decimal Max(this IEnumerable<decimal> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal value = 0;
            bool hasValue = false;
            foreach (decimal x in source) {
                if (hasValue) {
                    if (x > value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static decimal? Max(this IEnumerable<decimal?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal? value = null;
            foreach (decimal? x in source) {
                if (value == null || x > value) value = x;
            }
            return value;
        }

        public static TSource Max<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            Comparer<TSource> comparer = Comparer<TSource>.Default;
            TSource value = default(TSource);
            if (value == null) {
                foreach (TSource x in source) {
                    if (x != null && (value == null || comparer.Compare(x, value) > 0))
                        value = x;
                }
                return value;
            }
            else {
                bool hasValue = false;
                foreach (TSource x in source) {
                    if (hasValue) {
                        if (comparer.Compare(x, value) > 0)
                            value = x;
                    }
                    else {
                        value = x;
                        hasValue = true;
                    }
                }
                if (hasValue) return value;
                throw Error.NoElements();
            }
        }

        public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) {
            return Enumerable.Max(Enumerable.Select(source, selector));
        }

        public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) {
            return Enumerable.Max(Enumerable.Select(source, selector));
        }

        public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) {
            return Enumerable.Max(Enumerable.Select(source, selector));
        }

        public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) {
            return Enumerable.Max(Enumerable.Select(source, selector));
        }

        public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) {
            return Enumerable.Max(Enumerable.Select(source, selector));
        }

        public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) {
            return Enumerable.Max(Enumerable.Select(source, selector));
        }

        public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) {
            return Enumerable.Max(Enumerable.Select(source, selector));
        }

        public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) {
            return Enumerable.Max(Enumerable.Select(source, selector));
        }

        public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) {
            return Enumerable.Max(Enumerable.Select(source, selector));
        }

        public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) {
            return Enumerable.Max(Enumerable.Select(source, selector));
        }

        public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            return Enumerable.Max(Enumerable.Select(source, selector));
        }

        public static double Average(this IEnumerable<int> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            long count = 0;
            checked {
                foreach (int v in source) {
                    sum += v;
                    count++;
                }
            }
            if (count > 0) return (double)sum / count;
            throw Error.NoElements();
        }

        public static double? Average(this IEnumerable<int?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            long count = 0;
            checked {
                foreach (int? v in source) {
                    if (v != null) {
                        sum += v.GetValueOrDefault();
                        count++;
                    }
                }
            }
            if (count > 0) return (double)sum / count;
            return null;
        }

        public static double Average(this IEnumerable<long> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            long count = 0;
            checked {
                foreach (long v in source) {
                    sum += v;
                    count++;
                }
            }
            if (count > 0) return (double)sum / count;
            throw Error.NoElements();
        }

        public static double? Average(this IEnumerable<long?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            long count = 0;
            checked {
                foreach (long? v in source) {
                    if (v != null) {
                        sum += v.GetValueOrDefault();
                        count++;
                    }
                }
            }
            if (count > 0) return (double)sum / count;
            return null;
        }

        public static float Average(this IEnumerable<float> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            long count = 0;
            checked {
                foreach (float v in source) {
                    sum += v;
                    count++;
                }
            }
            if (count > 0) return (float)(sum / count);
            throw Error.NoElements();
        }

        public static float? Average(this IEnumerable<float?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            long count = 0;
            checked {
                foreach (float? v in source) {
                    if (v != null) {
                        sum += v.GetValueOrDefault();
                        count++;
                    }
                }
            }
            if (count > 0) return (float)(sum / count);
            return null;
        }

        public static double Average(this IEnumerable<double> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            long count = 0;
            checked {
                foreach (double v in source) {
                    sum += v;
                    count++;
                }
            }
            if (count > 0) return sum / count;
            throw Error.NoElements();
        }

        public static double? Average(this IEnumerable<double?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            long count = 0;
            checked {
                foreach (double? v in source) {
                    if (v != null) {
                        sum += v.GetValueOrDefault();
                        count++;
                    }
                }
            }
            if (count > 0) return sum / count;
            return null;
        }

        public static decimal Average(this IEnumerable<decimal> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal sum = 0;
            long count = 0;
            checked {
                foreach (decimal v in source) {
                    sum += v;
                    count++;
                }
            }
            if (count > 0) return sum / count;
            throw Error.NoElements();
        }

        public static decimal? Average(this IEnumerable<decimal?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal sum = 0;
            long count = 0;
            checked {
                foreach (decimal? v in source) {
                    if (v != null) {
                        sum += v.GetValueOrDefault();
                        count++;
                    }
                }
            }
            if (count > 0) return sum / count;
            return null;
        }

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) {
            return Enumerable.Average(Enumerable.Select(source, selector));
        }

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) {
            return Enumerable.Average(Enumerable.Select(source, selector));
        }

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) {
            return Enumerable.Average(Enumerable.Select(source, selector));
        }

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) {
            return Enumerable.Average(Enumerable.Select(source, selector));
        }

        public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) {
            return Enumerable.Average(Enumerable.Select(source, selector));
        }

        public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) {
            return Enumerable.Average(Enumerable.Select(source, selector));
        }

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) {
            return Enumerable.Average(Enumerable.Select(source, selector));
        }

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) {
            return Enumerable.Average(Enumerable.Select(source, selector));
        }

        public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) {
            return Enumerable.Average(Enumerable.Select(source, selector));
        }

        public static decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) {
            return Enumerable.Average(Enumerable.Select(source, selector));
        }
    }

    internal class EmptyEnumerable<TElement>
    {
        static volatile TElement[] instance;

        public static IEnumerable<TElement> Instance {
            get {
                if (instance == null) instance = new TElement[0];
                return instance;
            }
        }
    }

    internal class IdentityFunction<TElement>
    {
        public static Func<TElement, TElement> Instance {
            get { return x => x; }
        }
    }

    public interface IOrderedEnumerable<TElement> : IEnumerable<TElement>
    {
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }

#if SILVERLIGHT && !FEATURE_NETCORE
    public interface IGrouping<TKey, TElement> : IEnumerable<TElement>
#else
    public interface IGrouping<out TKey, out TElement> : IEnumerable<TElement>
#endif
    {
        TKey Key { get; }
    }

    public interface ILookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>{
        int Count { get; }
        IEnumerable<TElement> this[TKey key] { get; }
        bool Contains(TKey key);
    }

    public class Lookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, ILookup<TKey, TElement>{
        IEqualityComparer<TKey> comparer;
        Grouping[] groupings;
        Grouping lastGrouping;
        int count;

        internal static Lookup<TKey, TElement> Create<TSource>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            if (keySelector == null) throw Error.ArgumentNull("keySelector");
            if (elementSelector == null) throw Error.ArgumentNull("elementSelector");
            Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
            foreach (TSource item in source) {
                lookup.GetGrouping(keySelector(item), true).Add(elementSelector(item));
            }
            return lookup;
        }

        internal static Lookup<TKey, TElement> CreateForJoin(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
            foreach (TElement item in source) {
                TKey key = keySelector(item);
                if (key != null) lookup.GetGrouping(key, true).Add(item);
            }
            return lookup;
        }

        Lookup(IEqualityComparer<TKey> comparer) {
            if (comparer == null) comparer = EqualityComparer<TKey>.Default;
            this.comparer = comparer;
            groupings = new Grouping[7];
        }

        public int Count {
            get { return count; }
        }

        public IEnumerable<TElement> this[TKey key] {
            get {
                Grouping grouping = GetGrouping(key, false);
                if (grouping != null) return grouping;
                return EmptyEnumerable<TElement>.Instance;
            }
        }

        public bool Contains(TKey key) {
            return GetGrouping(key, false) != null;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() {
            Grouping g = lastGrouping;
            if (g != null) {
                do {
                    g = g.next;
                    yield return g;
                } while (g != lastGrouping);
            }
        }

        public IEnumerable<TResult> ApplyResultSelector<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector){
            Grouping g = lastGrouping;
            if (g != null) {
                do {
                    g = g.next;
                    if (g.count != g.elements.Length) { Array.Resize<TElement>(ref g.elements, g.count); }
                    yield return resultSelector(g.key, g.elements);
                }while (g != lastGrouping);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        internal int InternalGetHashCode(TKey key)
        {
            //[....] DevDivBugs 171937. work around comparer implementations that throw when passed null
            return (key == null) ? 0 : comparer.GetHashCode(key) & 0x7FFFFFFF;
        }

        internal Grouping GetGrouping(TKey key, bool create) {
            int hashCode = InternalGetHashCode(key);
            for (Grouping g = groupings[hashCode % groupings.Length]; g != null; g = g.hashNext)
                if (g.hashCode == hashCode && comparer.Equals(g.key, key)) return g;
            if (create) {
                if (count == groupings.Length) Resize();
                int index = hashCode % groupings.Length;
                Grouping g = new Grouping();
                g.key = key;
                g.hashCode = hashCode;
                g.elements = new TElement[1];
                g.hashNext = groupings[index];
                groupings[index] = g;
                if (lastGrouping == null) {
                    g.next = g;
                }
                else {
                    g.next = lastGrouping.next;
                    lastGrouping.next = g;
                }
                lastGrouping = g;
                count++;
                return g;
            }
            return null;
        }

        void Resize() {
            int newSize = checked(count * 2 + 1);
            Grouping[] newGroupings = new Grouping[newSize];
            Grouping g = lastGrouping;
            do {
                g = g.next;
                int index = g.hashCode % newSize;
                g.hashNext = newGroupings[index];
                newGroupings[index] = g;
            } while (g != lastGrouping);
            groupings = newGroupings;
        }

        internal class Grouping : IGrouping<TKey, TElement>, IList<TElement>
        {
            internal TKey key;
            internal int hashCode;
            internal TElement[] elements;
            internal int count;
            internal Grouping hashNext;
            internal Grouping next;

            internal void Add(TElement element) {
                if (elements.Length == count) Array.Resize(ref elements, checked(count * 2));
                elements[count] = element;
                count++;
            }

            public IEnumerator<TElement> GetEnumerator() {
                for (int i = 0; i < count; i++) yield return elements[i];
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            // DDB195907: implement IGrouping<>.Key implicitly
            // so that WPF binding works on this property.
            public TKey Key {
                get { return key; }
            }

            int ICollection<TElement>.Count {
                get { return count; }
            }

            bool ICollection<TElement>.IsReadOnly {
                get { return true; }
            }

            void ICollection<TElement>.Add(TElement item) {
                throw Error.NotSupported();
            }

            void ICollection<TElement>.Clear() {
                throw Error.NotSupported();
            }

            bool ICollection<TElement>.Contains(TElement item) {
                return Array.IndexOf(elements, item, 0, count) >= 0;
            }

            void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex) {
                Array.Copy(elements, 0, array, arrayIndex, count);
            }

            bool ICollection<TElement>.Remove(TElement item) {
                throw Error.NotSupported();
            }

            int IList<TElement>.IndexOf(TElement item) {
                return Array.IndexOf(elements, item, 0, count);
            }

            void IList<TElement>.Insert(int index, TElement item) {
                throw Error.NotSupported();
            }

            void IList<TElement>.RemoveAt(int index) {
                throw Error.NotSupported();
            }

            TElement IList<TElement>.this[int index] {
                get {
                    if (index < 0 || index >= count) throw Error.ArgumentOutOfRange("index");
                    return elements[index];
                }
                set {
                    throw Error.NotSupported();
                }
            }
        }
    }

    // @
    internal class Set<TElement>
    {
        int[] buckets;
        Slot[] slots;
        int count;
        int freeList;
        IEqualityComparer<TElement> comparer;

        public Set() : this(null) { }

        public Set(IEqualityComparer<TElement> comparer) {
            if (comparer == null) comparer = EqualityComparer<TElement>.Default;
            this.comparer = comparer;
            buckets = new int[7];
            slots = new Slot[7];
            freeList = -1;
        }

        // If value is not in set, add it and return true; otherwise return false
        public bool Add(TElement value) {
            return !Find(value, true);
        }

        // Check whether value is in set
        public bool Contains(TElement value) {
            return Find(value, false);
        }

        // If value is in set, remove it and return true; otherwise return false
        public bool Remove(TElement value) {
            int hashCode = InternalGetHashCode(value);
            int bucket = hashCode % buckets.Length;
            int last = -1;
            for (int i = buckets[bucket] - 1; i >= 0; last = i, i = slots[i].next) {
                if (slots[i].hashCode == hashCode && comparer.Equals(slots[i].value, value)) {
                    if (last < 0) {
                        buckets[bucket] = slots[i].next + 1;
                    }
                    else {
                        slots[last].next = slots[i].next;
                    }
                    slots[i].hashCode = -1;
                    slots[i].value = default(TElement);
                    slots[i].next = freeList;
                    freeList = i;
                    return true;
                }
            }
            return false;
        }

        bool Find(TElement value, bool add) {
            int hashCode = InternalGetHashCode(value);
            for (int i = buckets[hashCode % buckets.Length] - 1; i >= 0; i = slots[i].next) {
                if (slots[i].hashCode == hashCode && comparer.Equals(slots[i].value, value)) return true;
            }
            if (add) {
                int index;
                if (freeList >= 0) {
                    index = freeList;
                    freeList = slots[index].next;
                }
                else {
                    if (count == slots.Length) Resize();
                    index = count;
                    count++;
                }
                int bucket = hashCode % buckets.Length;
                slots[index].hashCode = hashCode;
                slots[index].value = value;
                slots[index].next = buckets[bucket] - 1;
                buckets[bucket] = index + 1;
            }
            return false;
        }

        void Resize() {
            int newSize = checked(count * 2 + 1);
            int[] newBuckets = new int[newSize];
            Slot[] newSlots = new Slot[newSize];
            Array.Copy(slots, 0, newSlots, 0, count);
            for (int i = 0; i < count; i++) {
                int bucket = newSlots[i].hashCode % newSize;
                newSlots[i].next = newBuckets[bucket] - 1;
                newBuckets[bucket] = i + 1;
            }
            buckets = newBuckets;
            slots = newSlots;
        }

        internal int InternalGetHashCode(TElement value)
        {
            //[....] DevDivBugs 171937. work around comparer implementations that throw when passed null
            return (value == null) ? 0 : comparer.GetHashCode(value) & 0x7FFFFFFF;
        }

        internal struct Slot
        {
            internal int hashCode;
            internal TElement value;
            internal int next;
        }
    }

    internal class GroupedEnumerable<TSource, TKey, TElement, TResult> : IEnumerable<TResult>{
        IEnumerable<TSource> source;
        Func<TSource, TKey> keySelector;
        Func<TSource, TElement> elementSelector;
        IEqualityComparer<TKey> comparer;
        Func<TKey, IEnumerable<TElement>, TResult> resultSelector;

        public GroupedEnumerable(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer){
            if (source == null) throw Error.ArgumentNull("source");
            if (keySelector == null) throw Error.ArgumentNull("keySelector");
            if (elementSelector == null) throw Error.ArgumentNull("elementSelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            this.source = source;
            this.keySelector = keySelector;
            this.elementSelector = elementSelector;
            this.comparer = comparer;
            this.resultSelector = resultSelector;
        }

        public IEnumerator<TResult> GetEnumerator(){
            Lookup<TKey, TElement> lookup = Lookup<TKey, TElement>.Create<TSource>(source, keySelector, elementSelector, comparer);
            return lookup.ApplyResultSelector(resultSelector).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }
    }

    internal class GroupedEnumerable<TSource, TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>
    {
        IEnumerable<TSource> source;
        Func<TSource, TKey> keySelector;
        Func<TSource, TElement> elementSelector;
        IEqualityComparer<TKey> comparer;

        public GroupedEnumerable(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            if (keySelector == null) throw Error.ArgumentNull("keySelector");
            if (elementSelector == null) throw Error.ArgumentNull("elementSelector");
            this.source = source;
            this.keySelector = keySelector;
            this.elementSelector = elementSelector;
            this.comparer = comparer;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() {
            return Lookup<TKey, TElement>.Create<TSource>(source, keySelector, elementSelector, comparer).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    internal abstract class OrderedEnumerable<TElement> : IOrderedEnumerable<TElement>
    {
        internal IEnumerable<TElement> source;

        public IEnumerator<TElement> GetEnumerator() {
            Buffer<TElement> buffer = new Buffer<TElement>(source);
            if (buffer.count > 0) {
                EnumerableSorter<TElement> sorter = GetEnumerableSorter(null);
                int[] map = sorter.Sort(buffer.items, buffer.count);
                sorter = null;
                for (int i = 0; i < buffer.count; i++) yield return buffer.items[map[i]];
            }
        }

        internal abstract EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next);

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending) {
            OrderedEnumerable<TElement, TKey> result = new OrderedEnumerable<TElement, TKey>(source, keySelector, comparer, descending);
            result.parent = this;
            return result;
        }
    }

    internal class OrderedEnumerable<TElement, TKey> : OrderedEnumerable<TElement>
    {
        internal OrderedEnumerable<TElement> parent;
        internal Func<TElement, TKey> keySelector;
        internal IComparer<TKey> comparer;
        internal bool descending;

        internal OrderedEnumerable(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending) {
            if (source == null) throw Error.ArgumentNull("source");
            if (keySelector == null) throw Error.ArgumentNull("keySelector");
            this.source = source;
            this.parent = null;
            this.keySelector = keySelector;
            this.comparer = comparer != null ? comparer : Comparer<TKey>.Default;
            this.descending = descending;
        }

        internal override EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next) {
            EnumerableSorter<TElement> sorter = new EnumerableSorter<TElement, TKey>(keySelector, comparer, descending, next);
            if (parent != null) sorter = parent.GetEnumerableSorter(sorter);
            return sorter;
        }
    }

    internal abstract class EnumerableSorter<TElement>
    {
        internal abstract void ComputeKeys(TElement[] elements, int count);

        internal abstract int CompareKeys(int index1, int index2);

        internal int[] Sort(TElement[] elements, int count) {
            ComputeKeys(elements, count);
            int[] map = new int[count];
            for (int i = 0; i < count; i++) map[i] = i;
            QuickSort(map, 0, count - 1);
            return map;
        }

        void QuickSort(int[] map, int left, int right) {
            do {
                int i = left;
                int j = right;
                int x = map[i + ((j - i) >> 1)];
                do {
                    while (i < map.Length && CompareKeys(x, map[i]) > 0) i++;
                    while (j >= 0 && CompareKeys(x, map[j]) < 0) j--;
                    if (i > j) break;
                    if (i < j) {
                        int temp = map[i];
                        map[i] = map[j];
                        map[j] = temp;
                    }
                    i++;
                    j--;
                } while (i <= j);
                if (j - left <= right - i) {
                    if (left < j) QuickSort(map, left, j);
                    left = i;
                }
                else {
                    if (i < right) QuickSort(map, i, right);
                    right = j;
                }
            } while (left < right);
        }
    }

    internal class EnumerableSorter<TElement, TKey> : EnumerableSorter<TElement>
    {
        internal Func<TElement, TKey> keySelector;
        internal IComparer<TKey> comparer;
        internal bool descending;
        internal EnumerableSorter<TElement> next;
        internal TKey[] keys;

        internal EnumerableSorter(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, EnumerableSorter<TElement> next) {
            this.keySelector = keySelector;
            this.comparer = comparer;
            this.descending = descending;
            this.next = next;
        }

        internal override void ComputeKeys(TElement[] elements, int count) {
            keys = new TKey[count];
            for (int i = 0; i < count; i++) keys[i] = keySelector(elements[i]);
            if (next != null) next.ComputeKeys(elements, count);
        }

        internal override int CompareKeys(int index1, int index2) {
            int c = comparer.Compare(keys[index1], keys[index2]);
            if (c == 0) {
                if (next == null) return index1 - index2;
                return next.CompareKeys(index1, index2);
            }
            return descending ? -c : c;
        }
    }

    struct Buffer<TElement>
    {
        internal TElement[] items;
        internal int count;

        internal Buffer(IEnumerable<TElement> source) {
            TElement[] items = null;
            int count = 0;
            ICollection<TElement> collection = source as ICollection<TElement>;
            if (collection != null) {
                count = collection.Count;
                if (count > 0) {
                    items = new TElement[count];
                    collection.CopyTo(items, 0);
                }
            }
            else {
                foreach (TElement item in source) {
                    if (items == null) {
                        items = new TElement[4];
                    }
                    else if (items.Length == count) {
                        TElement[] newItems = new TElement[checked(count * 2)];
                        Array.Copy(items, 0, newItems, 0, count);
                        items = newItems;
                    }
                    items[count] = item;
                    count++;
                }
            }
            this.items = items;
            this.count = count;
        }

        internal TElement[] ToArray() {
            if (count == 0) return new TElement[0];
            if (items.Length == count) return items;
            TElement[] result = new TElement[count];
            Array.Copy(items, 0, result, 0, count);
            return result;
        }
    }

    /// <summary>
    /// This class provides the items view for the Enumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class SystemCore_EnumerableDebugView<T>
    {
        public SystemCore_EnumerableDebugView(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            this.enumerable = enumerable;
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                List<T> tempList = new List<T>();
                IEnumerator<T> currentEnumerator = this.enumerable.GetEnumerator();

                if (currentEnumerator != null)
                {
                    for(count = 0; currentEnumerator.MoveNext(); count++)
                    {
                        tempList.Add(currentEnumerator.Current);
                    }
                }
                if (count == 0)
                {
                    throw new SystemCore_EnumerableDebugViewEmptyException();
                }
                cachedCollection = new T[this.count];
                tempList.CopyTo(cachedCollection, 0);
                return cachedCollection;
            }
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private IEnumerable<T> enumerable;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private T[] cachedCollection;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int count;
    }

    internal sealed class SystemCore_EnumerableDebugViewEmptyException : Exception
    {
        public string Empty
        {
            get
            {
                return Strings.EmptyEnumerable;
            }
        }
    }

    internal sealed class SystemCore_EnumerableDebugView
    {
        public SystemCore_EnumerableDebugView(IEnumerable enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            this.enumerable = enumerable;
            count = 0;
            cachedCollection = null;
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public object[] Items
        {
            get
            {
                List<object> tempList = new List<object>();
                IEnumerator currentEnumerator = this.enumerable.GetEnumerator();

                if (currentEnumerator != null)
                {
                    for (count = 0; currentEnumerator.MoveNext(); count++)
                    {
                        tempList.Add(currentEnumerator.Current);
                    }
                }
                if (count == 0)
                {
                    throw new SystemCore_EnumerableDebugViewEmptyException();
                }
                cachedCollection = new object[this.count];
                tempList.CopyTo(cachedCollection, 0);
                return cachedCollection;
            }
        }
        
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private IEnumerable enumerable;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private object[] cachedCollection;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int count;
    }
}
