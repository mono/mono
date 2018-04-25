using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;

namespace System.Data.Linq {
    using System.Data.Linq.Mapping;
    using System.Data.Linq.Provider;

    internal abstract class IdentityManager {
        internal abstract object InsertLookup(MetaType type, object instance);
        internal abstract bool RemoveLike(MetaType type, object instance);
        internal abstract object Find(MetaType type, object[] keyValues);
        internal abstract object FindLike(MetaType type, object instance);

        internal static IdentityManager CreateIdentityManager(bool asReadOnly) {
            if (asReadOnly) {
                return new ReadOnlyIdentityManager();
            }
            else {
                return new StandardIdentityManager();
            }
        }

        class StandardIdentityManager : IdentityManager {
            Dictionary<MetaType, IdentityCache> caches;
            IdentityCache currentCache;
            MetaType currentType;

            internal StandardIdentityManager() {
                this.caches = new Dictionary<MetaType, IdentityCache>();
            }
         
            internal override object InsertLookup(MetaType type, object instance) {
                this.SetCurrent(type);
                return this.currentCache.InsertLookup(instance);
            }

            internal override bool RemoveLike(MetaType type, object instance) {
                this.SetCurrent(type);
                return this.currentCache.RemoveLike(instance);
            }

            internal override object Find(MetaType type, object[] keyValues) {
                this.SetCurrent(type);
                return this.currentCache.Find(keyValues);
            }
    
            internal override object FindLike(MetaType type, object instance) {
                this.SetCurrent(type);
                return this.currentCache.FindLike(instance);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            private void SetCurrent(MetaType type) {
                type = type.InheritanceRoot;
                if (this.currentType != type) {
                    if (!this.caches.TryGetValue(type, out this.currentCache)) {
                        KeyManager km = GetKeyManager(type);
                        this.currentCache = (IdentityCache)Activator.CreateInstance(
                            typeof(IdentityCache<,>).MakeGenericType(type.Type, km.KeyType),
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                            new object[] { km }, null
                            );
                        this.caches.Add(type, this.currentCache);
                    }
                    this.currentType = type;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            static KeyManager GetKeyManager(MetaType type) {
                int n = type.IdentityMembers.Count;
                MetaDataMember mm = type.IdentityMembers[0];

                KeyManager km = (KeyManager)Activator.CreateInstance(
                            typeof(SingleKeyManager<,>).MakeGenericType(type.Type, mm.Type),
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                            new object[] { mm.StorageAccessor, 0 }, null
                            );
                for (int i = 1; i < n; i++) {
                    mm = type.IdentityMembers[i];
                    km = (KeyManager)
                        Activator.CreateInstance(
                            typeof(MultiKeyManager<,,>).MakeGenericType(type.Type, mm.Type, km.KeyType),
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                            new object[] { mm.StorageAccessor, i, km }, null
                            );
                }

                return km;
            }

            #region Nested type definitions
            // These types are internal rather than private to work around
            // CLR bug #117419 related to type visibility under partial trust
            // in nested class scenarios.

            internal abstract class KeyManager {
                internal abstract Type KeyType { get; }
            }

            internal abstract class KeyManager<T, K> : KeyManager {
                internal abstract K CreateKeyFromInstance(T instance);
                internal abstract bool TryCreateKeyFromValues(object[] values, out K k);
                internal abstract IEqualityComparer<K> Comparer { get; }
            }

            internal class SingleKeyManager<T, V> : KeyManager<T, V> {
                bool isKeyNullAssignable;
                MetaAccessor<T, V> accessor;
                int offset;
                IEqualityComparer<V> comparer;

                internal SingleKeyManager(MetaAccessor<T, V> accessor, int offset) {
                    this.accessor = accessor;
                    this.offset = offset;
                    this.isKeyNullAssignable = System.Data.Linq.SqlClient.TypeSystem.IsNullAssignable(typeof(V));
                }

                internal override V CreateKeyFromInstance(T instance) {
                    return this.accessor.GetValue(instance);
                }

                internal override bool TryCreateKeyFromValues(object[] values, out V v) {
                    object o = values[this.offset];
                    if (o == null && !this.isKeyNullAssignable) {
                        v = default(V);
                        return false;
                    }
                    v = (V)o;
                    return true;
                }

                internal override Type KeyType {
                    get { return typeof(V); }
                }

                internal override IEqualityComparer<V> Comparer {
                    get {
                        if (this.comparer == null) {
                            this.comparer = EqualityComparer<V>.Default;
                        }
                        return this.comparer;
                    }
                }
            }

            internal class MultiKeyManager<T, V1, V2> : KeyManager<T, MultiKey<V1, V2>> {
                MetaAccessor<T, V1> accessor;
                int offset;
                KeyManager<T, V2> next;
                IEqualityComparer<MultiKey<V1, V2>> comparer;

                internal MultiKeyManager(MetaAccessor<T, V1> accessor, int offset, KeyManager<T, V2> next) {
                    this.accessor = accessor;
                    this.next = next;
                    this.offset = offset;
                }

                internal override MultiKey<V1, V2> CreateKeyFromInstance(T instance) {
                    return new MultiKey<V1, V2>(
                        this.accessor.GetValue(instance),
                        this.next.CreateKeyFromInstance(instance)
                        );
                }

                internal override bool TryCreateKeyFromValues(object[] values, out MultiKey<V1, V2> k) {
                    System.Diagnostics.Debug.Assert(this.offset < values.Length, "offset is outside the bounds of the values array");

                    object o = values[this.offset];
                    if (o == null && typeof(V1).IsValueType) {
                        k = default(MultiKey<V1, V2>);
                        return false;
                    }
                    V2 v2;
                    if (!this.next.TryCreateKeyFromValues(values, out v2)) {
                        k = default(MultiKey<V1, V2>);
                        return false;
                    }
                    k = new MultiKey<V1, V2>((V1)o, v2);
                    return true;
                }

                internal override Type KeyType {
                    get { return typeof(MultiKey<V1, V2>); }
                }

                internal override IEqualityComparer<MultiKey<V1, V2>> Comparer {
                    get {
                        if (this.comparer == null) {
                            this.comparer = new MultiKey<V1, V2>.Comparer(EqualityComparer<V1>.Default, next.Comparer);
                        }
                        return this.comparer;
                    }
                }
            }

            internal struct MultiKey<T1, T2> {
                T1 value1;
                T2 value2;

                internal MultiKey(T1 value1, T2 value2) {
                    this.value1 = value1;
                    this.value2 = value2;
                }

                internal class Comparer : IEqualityComparer<MultiKey<T1, T2>>, IEqualityComparer {
                    IEqualityComparer<T1> comparer1;
                    IEqualityComparer<T2> comparer2;

                    internal Comparer(IEqualityComparer<T1> comparer1, IEqualityComparer<T2> comparer2) {
                        this.comparer1 = comparer1;
                        this.comparer2 = comparer2;
                    }

                    public bool Equals(MultiKey<T1, T2> x, MultiKey<T1, T2> y) {
                        return this.comparer1.Equals(x.value1, y.value1) &&
                               this.comparer2.Equals(x.value2, y.value2);
                    }

                    public int GetHashCode(MultiKey<T1, T2> x) {
                        return this.comparer1.GetHashCode(x.value1) ^ this.comparer2.GetHashCode(x.value2);
                    }

                    bool IEqualityComparer.Equals(object x, object y) {
                        return this.Equals((MultiKey<T1, T2>)x, (MultiKey<T1, T2>)y);
                    }

                    int IEqualityComparer.GetHashCode(object x) {
                        return this.GetHashCode((MultiKey<T1, T2>)x);
                    }
                }
            }

            internal abstract class IdentityCache {
                internal abstract object Find(object[] keyValues);
                internal abstract object FindLike(object instance);
                internal abstract object InsertLookup(object instance);
                internal abstract bool RemoveLike(object instance);
            }

            internal class IdentityCache<T, K> : IdentityCache {
                int[] buckets;
                Slot[] slots;
                int count;
                int freeList;
                KeyManager<T, K> keyManager;
                IEqualityComparer<K> comparer;

                public IdentityCache(KeyManager<T, K> keyManager) {
                    this.keyManager = keyManager;
                    this.comparer = keyManager.Comparer;
                    buckets = new int[7];
                    slots = new Slot[7];
                    freeList = -1;
                }

                internal override object InsertLookup(object instance) {
                    T value = (T)instance;
                    K key = this.keyManager.CreateKeyFromInstance(value);
                    Find(key, ref value, true);
                    return value;
                }

                internal override bool RemoveLike(object instance) {
                    T value = (T)instance;
                    K key = this.keyManager.CreateKeyFromInstance(value);

                    int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                    int bucket = hashCode % buckets.Length;
                    int last = -1;
                    for (int i = buckets[bucket] - 1; i >= 0; last = i, i = slots[i].next) {
                        if (slots[i].hashCode == hashCode && comparer.Equals(slots[i].key, key)) {
                            if (last < 0) {
                                buckets[bucket] = slots[i].next + 1;
                            }
                            else {
                                slots[last].next = slots[i].next;
                            }
                            slots[i].hashCode = -1;
                            slots[i].value = default(T);
                            slots[i].next = freeList;
                            freeList = i;
                            return true;
                        }
                    }
                    return false;
                }

                internal override object Find(object[] keyValues) {
                    K key;
                    if (this.keyManager.TryCreateKeyFromValues(keyValues, out key)) {
                        T value = default(T);
                        if (Find(key, ref value, false))
                            return value;
                    }
                    return null;
                }

                internal override object FindLike(object instance) {
                    T value = (T)instance;
                    K key = this.keyManager.CreateKeyFromInstance(value);
                    if (Find(key, ref value, false))
                        return value;
                    return null;
                }

                bool Find(K key, ref T value, bool add) {
                    int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                    for (int i = buckets[hashCode % buckets.Length] - 1; i >= 0; i = slots[i].next) {
                        if (slots[i].hashCode == hashCode && comparer.Equals(slots[i].key, key)) {
                            value = slots[i].value;
                            return true;
                        }
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
                        slots[index].key = key;
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

                internal struct Slot {
                    internal int hashCode;
                    internal K key;
                    internal T value;
                    internal int next;
                }
            }
            #endregion
        }

        /// <summary>
        /// This is the noop implementation used when object tracking is disabled.
        /// </summary>
        class ReadOnlyIdentityManager : IdentityManager {
            internal ReadOnlyIdentityManager() { }
            internal override object InsertLookup(MetaType type, object instance) { return instance; }
            internal override bool RemoveLike(MetaType type, object instance) { return false; }
            internal override object Find(MetaType type, object[] keyValues) { return null; }
            internal override object FindLike(MetaType type, object instance) { return null; }
        }
    }
}
