// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Xaml
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    public static class AttachablePropertyServices
    {
        static DefaultAttachedPropertyStore attachedProperties = new DefaultAttachedPropertyStore();

        public static int GetAttachedPropertyCount(object instance)
        {
            if (instance == null)
            {
                return 0;
            }

            IAttachedPropertyStore ap = instance as IAttachedPropertyStore;
            if (ap != null)
            {
                return ap.PropertyCount;
            }

            return attachedProperties.GetPropertyCount(instance);
        }

        public static void CopyPropertiesTo(object instance, KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
        {
            if (instance == null)
            {
                return;
            }

            IAttachedPropertyStore ap = instance as IAttachedPropertyStore;
            if (ap != null)
            {
                ap.CopyPropertiesTo(array, index);
            }
            else
            {
                attachedProperties.CopyPropertiesTo(instance, array, index);
            }
        }

        public static bool RemoveProperty(object instance, AttachableMemberIdentifier name)
        {
            if (instance == null)
            {
                return false;
            }

            IAttachedPropertyStore ap = instance as IAttachedPropertyStore;
            if (ap != null)
            {
                return ap.RemoveProperty(name);
            }

            return attachedProperties.RemoveProperty(instance, name);
        }

        public static void SetProperty(object instance, AttachableMemberIdentifier name, object value)
        {
            if (instance == null)
            {
                return;
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            IAttachedPropertyStore ap = instance as IAttachedPropertyStore;
            if (ap != null)
            {
                ap.SetProperty(name, value);
                return;
            }

            attachedProperties.SetProperty(instance, name, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1007")]
        public static bool TryGetProperty(object instance, AttachableMemberIdentifier name, out object value)
        {
            return TryGetProperty<object>(instance, name, out value);
        }

        public static bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
        {
            if (instance == null)
            {
                value = default(T);
                return false;
            }

            IAttachedPropertyStore ap = instance as IAttachedPropertyStore;
            if (ap != null)
            {
                object obj;
                bool result = ap.TryGetProperty(name, out obj);
                if (result)
                {
                    if (obj is T)
                    {
                        value = (T)obj;
                        return true;
                    }
                }
                value = default(T);
                return false;
            }

            return attachedProperties.TryGetProperty(instance, name, out value);
        }

        // DefaultAttachedPropertyStore is used by the global AttachedPropertyServices to implement
        //  global attached properties for types which don't implement IAttachedProperties or DO/Dependency Property
        //  integration for their attached properties.
        //
 #if !TARGETTING35SP1
        sealed class DefaultAttachedPropertyStore
        {
            Lazy<ConditionalWeakTable<object, Dictionary<AttachableMemberIdentifier, object>>> instanceStorage =
                new Lazy<ConditionalWeakTable<object, Dictionary<AttachableMemberIdentifier, object>>>();

            public void CopyPropertiesTo(object instance, KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
            {
                if (instanceStorage.IsValueCreated)
                {
                    Dictionary<AttachableMemberIdentifier, object> instanceProperties;
                    if (instanceStorage.Value.TryGetValue(instance, out instanceProperties))
                    {
                        lock (instanceProperties)
                        {
                            ((ICollection<KeyValuePair<AttachableMemberIdentifier, object>>)instanceProperties).CopyTo(array, index);
                        }
                    }
                }
            }

            public int GetPropertyCount(object instance)
            {
                if (instanceStorage.IsValueCreated)
                {
                    Dictionary<AttachableMemberIdentifier, object> instanceProperties;
                    if (instanceStorage.Value.TryGetValue(instance, out instanceProperties))
                    {
                        lock (instanceProperties)
                        {
                            return instanceProperties.Count;
                        }
                    }
                }
                return 0;
            }

            // <summary>
            // Remove the property 'name'. If the property doesn't exist it returns false.
            // </summary>
            public bool RemoveProperty(object instance, AttachableMemberIdentifier name)
            {
                if (instanceStorage.IsValueCreated)
                {
                    Dictionary<AttachableMemberIdentifier, object> instanceProperties;
                    if (instanceStorage.Value.TryGetValue(instance, out instanceProperties))
                    {
                        lock (instanceProperties)
                        {
                            return instanceProperties.Remove(name);
                        }
                    }
                }
                return false;
            }

            // <summary>
            // Set the property 'name' value to 'value', if the property doesn't currently exist this will add the property
            // </summary>
            public void SetProperty(object instance, AttachableMemberIdentifier name, object value)
            {
                Dictionary<AttachableMemberIdentifier, object> instanceProperties;
                if (!instanceStorage.Value.TryGetValue(instance, out instanceProperties))
                {
                    instanceProperties = new Dictionary<AttachableMemberIdentifier, object>();
                    //
                    // Workaround lack of TryAdd for ConditionalWeakTable
                    try
                    {
                        instanceStorage.Value.Add(instance, instanceProperties);
                    }
                    catch (ArgumentException)
                    {
                        //
                        // If Add fails we raced and the item should exist
                        if (!instanceStorage.Value.TryGetValue(instanceStorage, out instanceProperties))
                        {
                            //
                            // If for some reason it doesn't, throw.
                            throw new InvalidOperationException(SR.Get(SRID.DefaultAttachablePropertyStoreCannotAddInstance));
                        }
                    }
                }

                lock (instanceProperties)
                {
                    instanceProperties[name] = value;
                }
            }

            // <summary>
            // Retrieve the value of the attached property 'name'. If there is not attached property then return false.
            // </summary>
            public bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
            {
                if (instanceStorage.IsValueCreated)
                {
                    Dictionary<AttachableMemberIdentifier, object> instanceProperties;
                    if (instanceStorage.Value.TryGetValue(instance, out instanceProperties))
                    {
                        lock (instanceProperties)
                        {
                            object valueAsObj;
                            if (instanceProperties.TryGetValue(name, out valueAsObj) &&
                                valueAsObj is T)
                            {
                                value = (T)valueAsObj;
                                return true;
                            }
                        }
                    }
                }
                value = default(T);
                return false;
            }
        }
#else        
        //***********************************************WARNING************************************************************
        // In CLR4.0 there is ConditionalWeakTable.  This implementation is for 3.5 and uses a WeakKey dictionary.
        //  This implementation does not handle the problem where a key is rooted in a value (or graph of a value).
        //  This will "leak" (never get cleaned up).  If we ship a 3.5 version of System.Xaml.dll we should
        //  consider adding logic that detects such cycles on Add and throws.
        //******************************************************************************************************************
        sealed class DefaultAttachedPropertyStore
        {
            Lazy<WeakDictionary<object, Dictionary<AttachableMemberIdentifier, object>>> instanceStorage = 
                new Lazy<WeakDictionary<object, Dictionary<AttachableMemberIdentifier, object>>>(
                    () => new WeakDictionary<object, Dictionary<AttachableMemberIdentifier, object>>(), LazyInitMode.AllowMultipleThreadSafeExecution);

            public void CopyPropertiesTo(object instance, KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
            {
                if (instanceStorage.IsInitialized)
                {
                    lock (instanceStorage.Value.SyncObject)
                    {
                        Dictionary<AttachableMemberIdentifier, object> instanceProperties;
                        if (instanceStorage.Value.TryGetValue(instance, out instanceProperties))
                        {
                            ((ICollection<KeyValuePair<AttachableMemberIdentifier, object>>)instanceProperties).CopyTo(array, index);
                        }
                    }
                }
            }

            public int GetPropertyCount(object instance)
            {
                if (instanceStorage.IsInitialized)
                {
                    lock(instanceStorage.Value.SyncObject)
                    {
                        Dictionary<AttachableMemberIdentifier, object> instanceProperties;
                        if (instanceStorage.Value.TryGetValue(instance, out instanceProperties))
                        {
                            return instanceProperties.Count;
                        }
                    }
                }
                return 0;
            }

            // <summary>
            // Remove the property 'name'. If the property doesn't exist it returns false.
            // </summary>
            public bool RemoveProperty(object instance, AttachableMemberIdentifier name)
            {
                if (instanceStorage.IsInitialized)
                {
                    lock (instanceStorage.Value.SyncObject)
                    {
                        Dictionary<AttachableMemberIdentifier, object> instanceProperties;
                        if (instanceStorage.Value.TryGetValue(instance, out instanceProperties))
                        {
                            return instanceProperties.Remove(name);
                        }
                    }
                }
                return false;
            }

            // <summary>
            // Set the property 'name' value to 'value', if the property doesn't currently exist this will add the property
            // </summary>
            public void SetProperty(object instance, AttachableMemberIdentifier name, object value)
            {
                //
                // Accessing Value forces initialization
                lock (instanceStorage.Value.SyncObject)
                {
                    Dictionary<AttachableMemberIdentifier, object> instanceProperties;
                    if (!instanceStorage.Value.TryGetValue(instance, out instanceProperties))
                    {
                        instanceProperties = new Dictionary<AttachableMemberIdentifier, object>();
                        instanceStorage.Value.Add(instance, instanceProperties);
                    }
                    instanceProperties[name] = value;
                }
            }

            // <summary>
            // Retrieve the value of the attached property 'name'. If there is not attached property then return false.
            // </summary>
            public bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
            {
                if (instanceStorage.IsInitialized)
                {
                    lock (instanceStorage.Value.SyncObject)
                    {
                        Dictionary<AttachableMemberIdentifier, object> attachedProperties;
                        if (instanceStorage.Value.TryGetValue(instance, out attachedProperties))
                        {
                            object valueAsObj;
                            if (attachedProperties.TryGetValue(name, out valueAsObj) &&
                                valueAsObj is T)
                            {
                                value = (T)valueAsObj;
                                return true;
                            }
                        }
                    }
                }
                value = default(T);
                return false;
            }

            public class WeakDictionary<K, V>
                where K : class
                where V : class
            {
                // Optimally this would be a _real_ dictionary implementation that when it ran across WeakKey's that had 
                //  gone out of scope would immediately at least clear their value and from time to time schedule a real
                //  cleanup.
                Dictionary<WeakKey, V> storage;
                object cleanupSyncObject;

                public WeakDictionary()
                {
                    if (typeof(K) == typeof(WeakReference))
                    {
                        throw new InvalidOperationException();
                    }

                    this.cleanupSyncObject = new object();
                    storage = new Dictionary<WeakDictionary<K, V>.WeakKey, V>();
                    // kick off the cleanup process...
                    WeakDictionaryCleanupToken.CreateCleanupToken(this);
                }

                public int Count
                {
                    get { return storage.Count; }
                }

                public void Add(K key, V value)
                {
                    storage.Add(new WeakKey(key, false), value);
                }

                internal object SyncObject
                {
                    get { return this.cleanupSyncObject; }
                }

                public void Cleanup()
                {
                    List<WeakKey> toClean = null;

                    // First determine what items have died and can be removed...
                    lock(cleanupSyncObject)
                    {
                        foreach (var key in storage.Keys)
                        {
                            bool isAlive, needsCleanup;
                            key.GetValue(out isAlive, out needsCleanup);
                            if (!isAlive)
                            {
                                if (toClean == null)
                                {
                                    toClean = new List<WeakKey>();
                                }
                                toClean.Add(key);
                            }
                        }
                    }

                    if (toClean != null)
                    {
                        // Second go and remove them...
                        lock(cleanupSyncObject)
                        {
                            // If toClean is > some % of the total size it is probably better
                            // just to create a new dictionary and migrate some set of items over
                            // wholesale? It is unclear whether or not this is true.
                            foreach (var key in toClean)
                            {
                                storage.Remove(key);
                            }
                        }
                    }

                    WeakDictionaryCleanupToken.CreateCleanupToken(this);
                }

                public bool Remove(K key)
                {
                    return storage.Remove(new WeakKey(key, true));
                }

                public bool TryGetValue(K key, out V value)
                {
                    return storage.TryGetValue(new WeakKey(key, true), out value);
                }

                public struct WeakKey : IEquatable<WeakKey>
                {
                    int hashCode;
                    object reference;

                    public WeakKey(K key, bool lookup)
                    {
                        hashCode = key.GetHashCode();
                        reference = lookup ? key : (object)new WeakReference(key);
                    }

                    public override int GetHashCode()
                    {
                        return hashCode;
                    }

                    public K GetValue(out bool isAlive, out bool needsCleanup)
                    {
                        if (reference == null)
                        {
                            isAlive = false;
                            needsCleanup = false;
                            return (K)reference;
                        }

                        K value;
                        WeakReference wr = reference as WeakReference;
                        if (wr != null)
                        {
                            value = (K)wr.Target;
                            isAlive = value != null;
                            needsCleanup = !isAlive;
                            if (needsCleanup)
                            {
                                // This cleans up the WeakReference now that we know the
                                //  target object is dead...
                                reference = null;
                            }
                            return value;
                        }

                        value = reference as K;
                        if (value != null)
                        {
                            isAlive = true;
                            needsCleanup = false;
                            return value;
                        }

                        throw new InvalidOperationException();
                    }

                    public bool Equals(WeakKey other)
                    {
                        WeakKey x = this;
                        WeakKey y = other;
                        bool xIsAlive, yIsAlive;
                        bool xNeedsCleanup, yNeedsCleanup;
                        K xKey = x.GetValue(out xIsAlive, out xNeedsCleanup);
                        K yKey = y.GetValue(out yIsAlive, out yNeedsCleanup);

                        // If they are both not alive then they are equivalent
                        if (!xIsAlive && !yIsAlive)
                        {
                            return true;
                        }

                        if (!xIsAlive || !yIsAlive)
                        {
                            return false;
                        }

                        return xKey == yKey;
                    }
                }

                // This cleanup token will be immediately thrown away and as a result it will 
                //  (a couple of GCs later) make it into the finalization queue and when finalized
                //  will kick off a thread-pool job to cleanup the dictionary.

                public class WeakDictionaryCleanupToken
                {
                    WeakDictionary<K, V> storage;

                    WeakDictionaryCleanupToken(WeakDictionary<K, V> storage)
                    {
                        this.storage = storage;
                    }

                    ~WeakDictionaryCleanupToken()
                    {
                        // Schedule cleanup
                        ThreadPool.QueueUserWorkItem((WaitCallback)delegate
                        {
                            storage.Cleanup();
                        });
                    }

                    [SuppressMessage("Microsoft.Usage", "CA1806",
                        Justification = "The point of this method is to create one of these and let it float away... To be finalized...")]
                    [SuppressMessage("Microsoft.Performance", "CA1804",
                        Justification = "The point of this method is to create one of these and let it float away... To be finalized...")]
                    public static void CreateCleanupToken(WeakDictionary<K, V> storage)
                    {
                        // Create one of these, the work is done when it is finalized which 
                        //  will be at some indeterminate point in the future...
                        WeakDictionaryCleanupToken token = new WeakDictionaryCleanupToken(storage);
                    }
                }

            }
        }
#endif
    }
}
