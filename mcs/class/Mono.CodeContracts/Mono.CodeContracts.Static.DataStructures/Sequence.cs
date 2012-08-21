// 
// Sequence.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Text;

namespace Mono.CodeContracts.Static.DataStructures {
        public class Sequence<T> : IEquatable<Sequence<T>> {
                public static readonly Sequence<T> Empty = null;
                readonly int count;
                readonly T element;
                readonly Sequence<T> tail;

                Sequence (T elem, Sequence<T> tail)
                {
                        this.element = elem;
                        this.tail = tail;
                        this.count = LengthOf (tail) + 1;
                }

                public T Head { get { return this.element; } }

                public Sequence<T> Tail { get { return this.tail; } }

                public static Sequence<T> Cons (T elem, Sequence<T> tail)
                {
                        return new Sequence<T> (elem, tail);
                }

                public static bool Contains (Sequence<T> l, T o)
                {
                        if (l == null)
                                return false;
                        var equatable = o as IEquatable<T>;
                        if (equatable != null) {
                                if (equatable.Equals (l.element))
                                        return true;
                        }
                        else if (o.Equals (l.element))
                                return true;

                        return Contains (l.tail, o);
                }

                public static int LengthOf (Sequence<T> list)
                {
                        if (list == null)
                                return 0;
                        return list.count;
                }

                public static void Apply (Sequence<T> list, Action<T> action)
                {
                        for (; list != null; list = list.tail)
                                action (list.Head);
                }

                public static IEnumerable<T> PrivateGetEnumerable (Sequence<T> list)
                {
                        Sequence<T> current = list;
                        while (current != null) {
                                T next = current.Head;
                                current = current.tail;
                                yield return next;
                        }
                }

                public static Sequence<S> Select<S> (Sequence<T> list, Func<T, S> selector)
                {
                        if (list == null)
                                return null;
                        return list.tail.Select (selector).Cons (selector (list.Head));
                }

                public static Sequence<T> From (params T[] elems)
                {
                        Sequence<T> result = null;
                        foreach (T elem in elems)
                                result = result.Cons (elem);

                        return result.Reverse ();
                }

                public static Sequence<T> From (IEnumerable<T> elems)
                {
                        Sequence<T> result = null;
                        foreach (T elem in elems)
                                result = result.Cons (elem);

                        return result.Reverse ();
                }

                public bool Equals (Sequence<T> other)
                {
                        if (ReferenceEquals (null, other)) return false;
                        if (ReferenceEquals (this, other)) return true;
                        return Equals (this.tail, other.tail) && EqualityComparer<T>.Default.Equals (this.element, other.element);
                }

                public override bool Equals (object obj)
                {
                        if (ReferenceEquals (null, obj)) return false;
                        if (ReferenceEquals (this, obj)) return true;
                        if (obj.GetType () != this.GetType ()) return false;
                        return Equals ((Sequence<T>) obj);
                }

                public override int GetHashCode ()
                {
                        unchecked {
                                return ((this.tail != null ? this.tail.GetHashCode () : 0) * 397) ^
                                       EqualityComparer<T>.Default.GetHashCode (this.element);
                        }
                }

                public override string ToString ()
                {
                        var sb = new StringBuilder ();
                        this.BuildString (sb);

                        return sb.ToString ();
                }

                void BuildString (StringBuilder sb)
                {
                        sb.Append (this.element == null ? "<null>" : this.element.ToString ());
                        if (this.tail != null) {
                                sb.Append (",");
                                this.tail.BuildString (sb);
                        }
                }

                public static Sequence<T> Singleton (T value)
                {
                        return new Sequence<T> (value, null);
                }
        }
}