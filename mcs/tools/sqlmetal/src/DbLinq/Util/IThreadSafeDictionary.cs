using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbLinq.Util
{
    /// <summary>
    /// Thread safe dictionary (Brian Rudolph's code)
    /// </summary>
    /// <typeparam name="TKey">Type of Keys</typeparam>
    /// <typeparam name="TValue">Type of Values</typeparam>
#if !MONO_STRICT
    public
#endif
    interface IThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// Merge is similar to the SQL merge or upsert statement.  
        /// </summary>
        /// <param name="key">Key to lookup</param>
        /// <param name="newValue">New Value</param>
        void MergeSafe(TKey key, TValue newValue);


        /// <summary>
        /// This is a blind remove. Prevents the need to check for existence first.
        /// </summary>
        /// <param name="key">Key to Remove</param>
        void RemoveSafe(TKey key);
    }
}
