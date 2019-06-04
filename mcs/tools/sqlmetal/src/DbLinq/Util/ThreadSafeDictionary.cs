using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace DbLinq.Util
{
    /// <summary>
    /// Thread Safe Dictionary Implementation (Brian Rudolph's code)
    /// </summary>
    /// <typeparam name="TKey">Type of Keys</typeparam>
    /// <typeparam name="TValue">Type of Values</typeparam>
#if !MONO_STRICT
    public
#endif
    class ThreadSafeDictionary<TKey, TValue> : IThreadSafeDictionary<TKey, TValue>
    {
        //This is the internal dictionary that we are wrapping
        IDictionary<TKey, TValue> dict;

        public ThreadSafeDictionary()
        {
            this.dict = new Dictionary<TKey, TValue>();
        }

        public ThreadSafeDictionary(int capacity)
        {
            dict = new Dictionary<TKey, TValue>(capacity);
        }

        public ThreadSafeDictionary(IEqualityComparer<TKey> comparer)
        {
            dict = new Dictionary<TKey, TValue>(comparer);
        }

        [NonSerialized]
        ReaderWriterLockSlim dictionaryLock = Locks.GetLockInstance(LockRecursionPolicy.NoRecursion); //setup the lock;

        /// <summary>
        /// This is a blind remove. Prevents the need to check for existence first.
        /// </summary>
        /// <param name="key">Key to remove</param>
        public void RemoveSafe(TKey key)
        {
            using (new ReadLock(this.dictionaryLock))
            {
                if (this.dict.ContainsKey(key))
                {
                    using (new WriteLock(this.dictionaryLock))
                    {
                        this.dict.Remove(key);
                    }
                }
            }
        }
        
        /// <summary>
        /// Merge does a blind remove, and then add.  Basically a blind Upsert.  
        /// </summary>
        /// <param name="key">Key to lookup</param>
        /// <param name="newValue">New Value</param>
        public void MergeSafe(TKey key, TValue newValue)
        {
            using (new WriteLock(this.dictionaryLock)) // take a writelock immediately since we will always be writing
            {
                if (this.dict.ContainsKey(key))
                {
                    this.dict.Remove(key);
                }

                this.dict.Add(key, newValue);
            }
        }
        
        public virtual bool Remove(TKey key)
        {
            using (new WriteLock(this.dictionaryLock))
            {
                return this.dict.Remove(key);
            }
        }

        public virtual bool ContainsKey(TKey key)
        {
            using (new ReadOnlyLock(this.dictionaryLock))
            {
                return this.dict.ContainsKey(key);
            }
        }
        
        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            using (new ReadOnlyLock(this.dictionaryLock))
            {
                return this.dict.TryGetValue(key, out value);
            }
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                using (new ReadOnlyLock(this.dictionaryLock))
                {
                    return this.dict[key];
                }
            }
            set
            {
                using (new WriteLock(this.dictionaryLock))
                {
                    this.dict[key] = value;
                }
            }
        }
        
        public virtual ICollection<TKey> Keys
        {
            get
            {
                using (new ReadOnlyLock(this.dictionaryLock))
                {
                    return new List<TKey>(this.dict.Keys);
                }
            }
        }
        
        public virtual ICollection<TValue> Values
        {
            get
            {
                using (new ReadOnlyLock(this.dictionaryLock))
                {
                    return new List<TValue>(this.dict.Values);
                }
            }
        }
        
        public virtual void Clear()
        {
            using (new WriteLock(this.dictionaryLock))
            {
                this.dict.Clear();
            }
        }
        
        public virtual int Count
        {
            get
            {
                using (new ReadOnlyLock(this.dictionaryLock))
                {
                    return this.dict.Count;
                }
            }
        }
        
        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            using (new ReadOnlyLock(this.dictionaryLock))
            {
                return this.dict.Contains(item);
            }
        }
        
        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            using (new WriteLock(this.dictionaryLock))
            {
                this.dict.Add(item);
            }
        }
        
        public virtual void Add(TKey key, TValue value)
        {
            using (new WriteLock(this.dictionaryLock))
            {
                this.dict.Add(key, value);
            }
        }
        
        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            using (new WriteLock(this.dictionaryLock))
            {
                return this.dict.Remove(item);
            }
        }
        
        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            using (new ReadOnlyLock(this.dictionaryLock))
            {
                this.dict.CopyTo(array, arrayIndex);
            }
        }
        
        public virtual bool IsReadOnly
        {
            get
            {
                using (new ReadOnlyLock(this.dictionaryLock))
                {
                    return this.dict.IsReadOnly;
                }
            }
        }
        
        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotSupportedException("Cannot enumerate a threadsafe dictionary.  Instead, enumerate the keys or values collection");
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException("Cannot enumerate a threadsafe dictionary.  Instead, enumerate the keys or values collection");
        }
    }

    public static class Locks
    {
        public static void GetReadLock(ReaderWriterLockSlim locks)
        {
            bool lockAcquired = false;
            while (!lockAcquired)
                lockAcquired = locks.TryEnterUpgradeableReadLock(1);
        }
        
        public static void GetReadOnlyLock(ReaderWriterLockSlim locks)
        {
            bool lockAcquired = false;
            while (!lockAcquired)
                lockAcquired = locks.TryEnterReadLock(1);
        }
        
        public static void GetWriteLock(ReaderWriterLockSlim locks)
        {
            bool lockAcquired = false;
            while (!lockAcquired)
                lockAcquired = locks.TryEnterWriteLock(1);
        }
        
        public static void ReleaseReadOnlyLock(ReaderWriterLockSlim locks)
        {
            if (locks.IsReadLockHeld)
                locks.ExitReadLock();
        }
        
        public static void ReleaseReadLock(ReaderWriterLockSlim locks)
        {
            if (locks.IsUpgradeableReadLockHeld)
                locks.ExitUpgradeableReadLock();
        }
        
        public static void ReleaseWriteLock(ReaderWriterLockSlim locks)
        {
            if (locks.IsWriteLockHeld)
                locks.ExitWriteLock();
        }
        
        public static void ReleaseLock(ReaderWriterLockSlim locks)
        {
            ReleaseWriteLock(locks);
            ReleaseReadLock(locks);
            ReleaseReadOnlyLock(locks);
        }
        
        public static ReaderWriterLockSlim GetLockInstance()
        {
            return GetLockInstance(LockRecursionPolicy.SupportsRecursion);
        }
        
        public static ReaderWriterLockSlim GetLockInstance(LockRecursionPolicy recursionPolicy)
        {
            return new ReaderWriterLockSlim(recursionPolicy);
        }
    }
    
    public abstract class BaseLock : IDisposable
    {
        protected ReaderWriterLockSlim _Locks;
        
        public BaseLock(ReaderWriterLockSlim locks)
        {
            _Locks = locks;
        }
        
        public abstract void Dispose();
    }
    
    public class ReadLock : BaseLock
    {
        public ReadLock(ReaderWriterLockSlim locks)
            : base(locks)
        {
            Locks.GetReadLock(this._Locks);
        }
        
        public override void Dispose()
        {
            Locks.ReleaseReadLock(this._Locks);
        }
    }
    
    public class ReadOnlyLock : BaseLock
    {
        public ReadOnlyLock(ReaderWriterLockSlim locks)
            : base(locks)
        {
            Locks.GetReadOnlyLock(this._Locks);
        }
        
        public override void Dispose()
        {
            Locks.ReleaseReadOnlyLock(this._Locks);
        }
    }
    
    public class WriteLock : BaseLock
    {
        public WriteLock(ReaderWriterLockSlim locks)
            : base(locks)
        {
            Locks.GetWriteLock(this._Locks);
        }
        
        public override void Dispose()
        {
            Locks.ReleaseWriteLock(this._Locks);
        }
    }
}
