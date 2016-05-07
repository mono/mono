/*++
Copyright (c) Microsoft Corporation

Module Name:

    _SpnDictionary.cs

Abstract:
    This internal class implements a static mutlithreaded dictionary for user-registered SPNs.
    An SPN is mapped based on a Uri prefix that contains scheme, host and port.


Author:

    Alexei Vopilov    15-Nov-2003

Revision History:

--*/

namespace System.Net {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Security.Permissions;

    internal class SpnDictionary : StringDictionary {

        //
        //A Hashtable can support one writer and multiple readers concurrently
        //

        // Maps Uri keys to SpnToken values.  The SpnTokens should not be exposed publicly.
        private Hashtable m_SyncTable = Hashtable.Synchronized(new Hashtable());
        private ValueCollection m_ValuesWrapper;

        //
        //
        internal SpnDictionary():base() {
        }
        //
        //
        //
        public override int Count {
            get {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return m_SyncTable.Count;
            }
        }
        //
        // We are thread safe
        //
        public override bool IsSynchronized {
            get {
                return true;
            }
        }
        //
        // Internal lookup, bypasses security checks
        //
        internal SpnToken InternalGet(string canonicalKey)
        {
            int lastLength = 0;
            string key = null;

            // This lock is required to avoid getting InvalidOperationException
            // because the collection was modified during enumeration. By design 
            // a Synchronized Hashtable throws if modifications occur while an 
            // enumeration is in progress. Manually locking the Hashtable to 
            // prevent modification during enumeration is the best solution. 
            // Catching the exception and retrying could potentially never
            // succeed in the face of significant updates.
            lock (m_SyncTable.SyncRoot) {
                foreach (object o in m_SyncTable.Keys){
                    string s = (string) o;
                    if(s != null && s.Length > lastLength){
                        if(String.Compare(s,0,canonicalKey,0,s.Length,StringComparison.OrdinalIgnoreCase) == 0){
                             lastLength = s.Length;
                             key = s;
                        }
                    }
                }  
            }
            return (key != null) ? (SpnToken)m_SyncTable[key] : null;
        }

        internal void InternalSet(string canonicalKey, SpnToken spnToken)
        {
            m_SyncTable[canonicalKey] = spnToken;
        }
        //
        // Public lookup method
        //
        public override string this[string key] {
            get {
                key = GetCanonicalKey(key);
                SpnToken token = InternalGet(key);
                return (token == null ? null : token.Spn);
            }
            set {
                key = GetCanonicalKey(key);
                // Value may be null
                InternalSet(key, new SpnToken(value));
            }
        }
        //
        public override ICollection Keys {
            get {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return m_SyncTable.Keys;
            }
        }
        //
        public override object SyncRoot {
            [HostProtection(Synchronization=true)]
            get {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return m_SyncTable;
            }
        }
        //
        public override ICollection Values {
            get {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (m_ValuesWrapper == null)
                {
                    m_ValuesWrapper = new ValueCollection(this);
                }
                return m_ValuesWrapper;
            }
        }
        //
        public override void Add(string key, string value) {
            key = GetCanonicalKey(key);
            m_SyncTable.Add(key, new SpnToken(value));
        }
        //
        public override void Clear() {
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            m_SyncTable.Clear();
        }
        //
        public override bool ContainsKey(string key) {
            key = GetCanonicalKey(key);
            return m_SyncTable.ContainsKey(key);
        }
        //
        public override bool ContainsValue(string value) {
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            foreach (SpnToken spnToken in m_SyncTable.Values)
            {
                if (spnToken.Spn == value)
                    return true;
            }
            return false;
        }

        // We have to unwrap the SpnKey and just expose the Spn
        public override void CopyTo(Array array, int index) {
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            CheckCopyToArguments(array, index, Count);

            int offset = 0;
            foreach (object entry in this)
            {
                array.SetValue(entry, offset + index);
                offset++;
            }
        }
        //
        public override IEnumerator GetEnumerator() {
            ExceptionHelper.WebPermissionUnrestricted.Demand();

            foreach (string key in m_SyncTable.Keys)
            {
                // We must unwrap the SpnToken and not expose it publicly
                SpnToken spnToken = (SpnToken)m_SyncTable[key];
                yield return new DictionaryEntry(key, spnToken.Spn);
            }
        }
        //
        public override void Remove(string key) {
            key = GetCanonicalKey(key);
            m_SyncTable.Remove(key);
        }

        //
        // Private stuff: We want to serialize on updates on one thread
        //
        private static string GetCanonicalKey(string key)
        {
            if( key == null ) {
                throw new ArgumentNullException("key");
            }
            try {
                Uri uri = new Uri(key);
                key = uri.GetParts(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.SafeUnescaped);
                new WebPermission(NetworkAccess.Connect, new Uri(key)).Demand();
            }
            catch(UriFormatException e) {
                throw new ArgumentException(SR.GetString(SR.net_mustbeuri, "key"), "key", e);
            }
            return key;
        }

        private static void CheckCopyToArguments(Array array, int index, int count)
        {
            // Coppied from HashTable.CopyTo
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(SR.GetString(SR.Arg_RankMultiDimNotSupported));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }
            if ((array.Length - index) < count)
            {
                throw new ArgumentException(SR.GetString(SR.Arg_ArrayPlusOffTooSmall));
            }
        }

        // Wrap HashTable.Values so we can unwrap the SpnTokens
        private class ValueCollection : ICollection
        {
            private SpnDictionary spnDictionary;

            internal ValueCollection(SpnDictionary spnDictionary)
            {
                this.spnDictionary = spnDictionary;
            }

            public void CopyTo(Array array, int index)
            {
                CheckCopyToArguments(array, index, Count);
                
                int offset = 0;
                foreach (object entry in this)
                {
                    array.SetValue(entry, offset + index);
                    offset++;
                }
            }

            public int Count
            {
                get { return spnDictionary.m_SyncTable.Values.Count; }
            }

            public bool IsSynchronized
            {
                get { return true; }
            }

            public object SyncRoot
            {
                get { return spnDictionary.m_SyncTable.SyncRoot; }
            }

            public IEnumerator GetEnumerator()
            {
                foreach (SpnToken spnToken in spnDictionary.m_SyncTable.Values)
                {
                    yield return (spnToken != null ? spnToken.Spn : null);
                }
            }
        }
    }

    internal class SpnToken
    {
        private readonly string spn;
        private bool isTrusted;

        // Assume the spn is trusted unless a specific reason is found not to trust it.
        internal bool IsTrusted
        {
            get { return isTrusted; }
            set { isTrusted = false; }
        }

        internal string Spn
        {
            get { return spn; }
        }

        internal SpnToken(string spn)
            : this(spn, true)
        { }

        internal SpnToken(string spn, bool trusted)
        {
            this.spn = spn;
            this.isTrusted = trusted;
        }
    }
}
