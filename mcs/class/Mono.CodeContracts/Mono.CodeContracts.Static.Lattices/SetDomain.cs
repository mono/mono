// 
// SetDomain.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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
using System.IO;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Lattices {
        struct SetDomain<T> : IAbstractDomain<SetDomain<T>>
                where T : IEquatable<T> {
                public static readonly SetDomain<T> TopValue = new SetDomain<T> (ImmutableSet<T>.Empty ());
                public static readonly SetDomain<T> BottomValue = new SetDomain<T> ((IImmutableSet<T>) null);

                readonly IImmutableSet<T> set;

                SetDomain (IImmutableSet<T> set)
                {
                        this.set = set;
                }

                public SetDomain (Func<T, int> keyConverter)
                {
                        set = ImmutableSet<T>.Empty (keyConverter);
                }

                public SetDomain<T> Top { get { return TopValue; } }

                public SetDomain<T> Bottom { get { return BottomValue; } }

                public bool IsTop { get { return set != null && set.Count == 0; } }

                public bool IsBottom { get { return set == null; } }

                public SetDomain<T> Join (SetDomain<T> that)
                {
                        SetDomain<T> result;
                        if (this.TryTrivialJoin (that, out result))
                                return result;

                        return new SetDomain<T> (set.Intersect (that.set));
                }

                public SetDomain<T> Join (SetDomain<T> that, bool widening, out bool weaker)
                {
                        if (set == that.set) {
                                weaker = false;
                                return this;
                        }
                        if (IsBottom) {
                                weaker = !that.IsBottom;
                                return that;
                        }
                        if (that.IsBottom || IsTop) {
                                weaker = false;
                                return this;
                        }
                        if (that.IsTop) {
                                weaker = !IsTop;
                                return that;
                        }

                        var join = set.Intersect (that.set);

                        weaker = join.Count < set.Count;
                        return new SetDomain<T> (join);
                }

                public SetDomain<T> Widen (SetDomain<T> that)
                {
                        //no widening - finite lattice

                        return Join (that);
                }

                public SetDomain<T> Meet (SetDomain<T> that)
                {
                        SetDomain<T> result;
                        if (this.TryTrivialMeet (that, out result))
                                return result;

                        return new SetDomain<T> (set.Union (that.set));
                }

                public bool LessEqual (SetDomain<T> that)
                {
                        if (IsBottom)
                                return true;
                        if (that.IsBottom)
                                return false;

                        return that.set.IsContainedIn (set);
                }

                public SetDomain<T> ImmutableVersion ()
                {
                        return this;
                }

                public SetDomain<T> Clone ()
                {
                        return this;
                }

                public SetDomain<T> With (T elem)
                {
                        return new SetDomain<T> (set.Add (elem));
                }

                public SetDomain<T> Without (T elem)
                {
                        return new SetDomain<T> (set.Remove (elem));
                }

                public bool Contains (T item)
                {
                        return set.Contains (item);
                }

                public void Dump (TextWriter tw)
                {
                        if (IsBottom)
                                tw.WriteLine ("Bot");
                        else if (IsTop)
                                tw.WriteLine ("Top");
                        else
                                set.Dump (tw);
                }
                }
}