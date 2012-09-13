// 
// EnvironmentDomain.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Lattices {
        class EnvironmentDomain<K, V> : IAbstractDomain<EnvironmentDomain<K, V>>
                where V : IAbstractDomain<V>
                where K : IEquatable<K> {
                readonly IImmutableMapFactory<K, V> factory;
                readonly IImmutableMap<K, V> map;

                EnvironmentDomain (IImmutableMapFactory<K, V> factory)
                        : this (factory, factory.Empty)
                {
                }

                EnvironmentDomain (IImmutableMap<K, V> map)
                        : this (map.Factory (), map)
                {
                }

                EnvironmentDomain (IImmutableMapFactory<K, V> factory, IImmutableMap<K, V> map)
                {
                        this.factory = factory;
                        this.map = map;
                }

                public V this [K key] { get { return map == null ? default(V) : map[key]; } }

                public IEnumerable<K> Keys { get { return map.Keys; } }

                #region IAbstractDomain<EnvironmentDomain<K,V>> Members

                public EnvironmentDomain<K, V> Top { get { return new EnvironmentDomain<K, V> (factory.Empty); } }

                public EnvironmentDomain<K, V> Bottom { get { return new EnvironmentDomain<K, V> (factory, null); } }

                public bool IsTop { get { return map != null && map.Count == 0; } }

                public bool IsBottom { get { return map == null; } }

                public EnvironmentDomain<K, V> Join (EnvironmentDomain<K, V> that)
                {
                        return JoinOrWiden (that, (a, b) => a.Join (b));
                }

                public EnvironmentDomain<K, V> Widen (EnvironmentDomain<K, V> that)
                {
                        return JoinOrWiden (that, (a, b) => a.Widen (b));
                }

                EnvironmentDomain<K, V> JoinOrWiden (EnvironmentDomain<K, V> that, Func<V, V, V> op)
                {
                        if (ReferenceEquals (map, that.map) || IsBottom)
                                return that;
                        if (that.IsBottom)
                                return this;

                        IImmutableMap<K, V> min;
                        IImmutableMap<K, V> max;
                        GetMinAndMaxByCount (map, that.map, out min, out max);

                        var result = min; // intersection of keys
                        foreach (var key in min.Keys) {
                                V thatValue;
                                if (max.TryGetValue (key, out thatValue)) {
                                        var join = op (min[key], thatValue);
                                        if (join.IsBottom)
                                                return Bottom;

                                        result = join.IsTop ? result.Remove (key) : result.Add (key, join);
                                }
                                else
                                        result = result.Remove (key);
                        }

                        return new EnvironmentDomain<K, V> (result);
                }

                public EnvironmentDomain<K, V> Join (EnvironmentDomain<K, V> that, bool widening, out bool weaker)
                {
                        //todo: remove it

                        weaker = false;
                        if (map == that.map || IsTop)
                                return this;
                        if (that.IsTop) {
                                weaker = !IsTop;
                                return that;
                        }
                        if (IsBottom) {
                                weaker = !that.IsBottom;
                                return that;
                        }
                        if (that.IsBottom)
                                return this;

                        IImmutableMap<K, V> min;
                        IImmutableMap<K, V> max;
                        GetMinAndMaxByCount (map, that.map, out min, out max);

                        var intersect = min;
                        foreach (var key in min.Keys) {
                                if (!max.ContainsKey (key))
                                        intersect = intersect.Remove (key);
                                else {
                                        bool keyWeaker;
                                        var join = min[key].Join (max[key], widening, out keyWeaker);
                                        if (keyWeaker) {
                                                weaker = true;
                                                intersect = join.IsTop ? intersect.Remove (key) : intersect.Add (key, join);
                                        }
                                }
                        }

                        weaker |= intersect.Count < map.Count;
                        return new EnvironmentDomain<K, V> (intersect);
                }

                public EnvironmentDomain<K, V> Meet (EnvironmentDomain<K, V> that)
                {
                        if (ReferenceEquals (map, that.map))
                                return this;
                        if (IsTop)
                                return that;
                        if (that.IsTop || IsBottom)
                                return this;
                        if (that.IsBottom)
                                return that;

                        IImmutableMap<K, V> min;
                        IImmutableMap<K, V> max;
                        GetMinAndMaxByCount (map, that.map, out min, out max);

                        var union = max;
                        foreach (var key in min.Keys) {
                                if (!max.ContainsKey (key))
                                        union = union.Add (key, min[key]);
                                else {
                                        var meet = min[key].Meet (max[key]);
                                        union = union.Add (key, meet);
                                }
                        }

                        return new EnvironmentDomain<K, V> (union);
                }

                public bool LessEqual (EnvironmentDomain<K, V> that)
                {
                        bool result;
                        if (this.TryTrivialLessEqual (that, out result))
                                return result;

                        if (map.Count < that.map.Count)
                                return false;

                        return that.map.Keys.All (key => map.ContainsKey (key) && map[key].LessEqual (that.map[key]));
                }

                public EnvironmentDomain<K, V> ImmutableVersion ()
                {
                        return this;
                }

                public EnvironmentDomain<K, V> Clone ()
                {
                        return this;
                }

                public void Dump (TextWriter tw)
                {
                        if (IsTop)
                                tw.WriteLine ("Top");
                        else if (IsBottom)
                                tw.WriteLine (this.BottomSymbolIfAny ());
                        else {
                                map.Visit ((k, v) => {
                                        tw.WriteLine ("{0} -> {1}", k, v);
                                        return VisitStatus.ContinueVisit;
                                });
                        }
                }

                #endregion

                public static EnvironmentDomain<K, V> TopValue (Func<K, int> keyConverter)
                {
                        if (keyConverter == null)
                                throw new ArgumentNullException ("keyConverter");

                        return new EnvironmentDomain<K, V> (ImmutableIntKeyMap<K, V>.Empty (keyConverter));
                }

                public static EnvironmentDomain<K, V> TopValue ()
                {
                        return new EnvironmentDomain<K, V> (ImmutableMap<K, V>.Empty);
                }

                public static EnvironmentDomain<K, V> BottomValue (Func<K, int> keyConverter)
                {
                        if (keyConverter == null)
                                throw new ArgumentNullException ("keyConverter");

                        return new EnvironmentDomain<K, V> (ImmutableIntKeyMap<K, V>.Empty (keyConverter).Factory (), null);
                }

                public static EnvironmentDomain<K, V> BottomValue ()
                {
                        return new EnvironmentDomain<K, V> (ImmutableMap<K, V>.Empty.Factory (), null);
                }

                public EnvironmentDomain<K, V> With (K key, V value)
                {
                        if (value.IsTop)
                                return Without (key);

                        return new EnvironmentDomain<K, V> (map.Add (key, value));
                }

                public EnvironmentDomain<K, V> RefineWith (K key, V value)
                {
                        V old;
                        if (map.TryGetValue (key, out old))
                                value = value.Meet (old);

                        return With (key, value);
                }

                public EnvironmentDomain<K, V> Without (K key)
                {
                        return new EnvironmentDomain<K, V> (map.Remove (key));
                }

                public bool Contains (K key)
                {
                        return map != null && map.ContainsKey (key);
                }

                public bool TryGetValue (K key, out V value)
                {
                        if (map == null)
                                return false.Without (out value);

                        return map.TryGetValue (key, out value);
                }

                public EnvironmentDomain<K, V> Empty ()
                {
                        return new EnvironmentDomain<K, V> (factory.Empty);
                }

                static bool GetMinAndMaxByCount
                        (IImmutableMap<K, V> a, IImmutableMap<K, V> b,
                         out IImmutableMap<K, V> min, out IImmutableMap<K, V> max)
                {
                        if (a.Count < b.Count) {
                                min = a;
                                max = b;
                                return true;
                        }
                        max = a;
                        min = b;
                        return false;
                }
                }
}