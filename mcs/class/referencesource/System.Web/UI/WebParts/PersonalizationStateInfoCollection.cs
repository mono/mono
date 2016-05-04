//------------------------------------------------------------------------------
// <copyright file="PersonalizationStateInfoCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.Util;

    [Serializable]
    public sealed class PersonalizationStateInfoCollection : ICollection {

        private Dictionary<Key, int> _indices;
        private bool _readOnly;

        // Tried to use the generic type List<T> instead, but it's readonly
        // implementation returns a different type that is not assignable to List<T>.
        // That would require to maintain two different fields, one for the readonly
        // collection when set and one for the modifiable collection version.
        //
        // So, the cleaner solution is to use ArrayList, though it might have a
        // slightly slower perf because of explicit casting.
        private ArrayList _values;

        public PersonalizationStateInfoCollection() {
            _indices = new Dictionary<Key, int>(KeyComparer.Default);
            _values = new ArrayList();
        }

        public int Count {
            get {
                return _values.Count;
            }
        }

        public PersonalizationStateInfo this[string path, string username] {
            get {
                if (path == null) {
                    throw new ArgumentNullException("path");
                }

                Key key = new Key(path, username);
                int index;
                if (!_indices.TryGetValue(key, out index)) {
                    return null;
                }
                return (PersonalizationStateInfo) _values[index];
            }
        }

        public PersonalizationStateInfo this[int index] {
            get {
                return (PersonalizationStateInfo) _values[index];
            }
        }

        public void Add(PersonalizationStateInfo data) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }

            Key key;
            UserPersonalizationStateInfo userData = data as UserPersonalizationStateInfo;
            if (userData != null) {
                key = new Key(userData.Path, userData.Username);
            }
            else {
                key = new Key(data.Path, null);
            }

            // VSWhidbey 376063: avoid key duplicate, we check here first before we add.
            if (_indices.ContainsKey(key)) {
                if (userData != null) {
                    throw new ArgumentException(SR.GetString(SR.PersonalizationStateInfoCollection_CouldNotAddUserStateInfo,
                            key.Path, key.Username));
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.PersonalizationStateInfoCollection_CouldNotAddSharedStateInfo,
                            key.Path));
                }
            }

            int pos = _values.Add(data);
            try {
                _indices.Add(key, pos);
            }
            catch {
                // Roll back the first addition to make the whole addition atomic
                _values.RemoveAt(pos);
                throw;
            }
        }

        public void Clear() {
            _values.Clear();
            _indices.Clear();
        }

        public void CopyTo(PersonalizationStateInfo[] array, int index) {
            _values.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator() {
            return _values.GetEnumerator();
        }

        public bool IsSynchronized {
            get {
                return false;
            }
        }

        public void Remove(string path, string username) {
            if (path == null) {
                throw new ArgumentNullException("path");
            }

            Key key = new Key(path, username);
            int ipos;
            if (!_indices.TryGetValue(key, out ipos)) {
                return;
            }
            Debug.Assert(ipos >= 0 && ipos < _values.Count);

            _indices.Remove(key);
            try {
                _values.RemoveAt(ipos);
            }
            catch {
                // Roll back the first addition to make the whole addition atomic
                _indices.Add(key, ipos);
                throw;
            }

            // Readjust the values' indices by -1 where the indices are greater than the removed index
            ArrayList al = new ArrayList();
            foreach(KeyValuePair<Key,int> de in _indices) {
                if (de.Value > ipos) {
                    al.Add(de.Key);
                }
            }
            foreach (Key k in al) {
                _indices[k] = ((int) _indices[k]) - 1;
            }
        }

        public void SetReadOnly() {
            if (_readOnly) {
                return;
            }
            _readOnly = true;
            _values = ArrayList.ReadOnly(_values);
        }

        public object SyncRoot {
            get {
                return this;
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            _values.CopyTo(array, index);
        }

        [Serializable]
        private sealed class Key {
            public string Path;
            public string Username;

            internal Key(string path, string username) {
                Debug.Assert(path != null);
                Path = path;
                Username = username;
            }
        }

        [Serializable]
        private sealed class KeyComparer : IEqualityComparer<Key> {
            internal static readonly IEqualityComparer<Key> Default = new KeyComparer();

            bool IEqualityComparer<Key>.Equals(Key x, Key y) {
                return (Compare(x, y) == 0);
            }

            int IEqualityComparer<Key>.GetHashCode(Key key) {
                if (key == null) {
                    return 0;
                }

                Debug.Assert(key.Path != null);
                int pathHashCode = key.Path.ToLowerInvariant().GetHashCode();
                int usernameHashCode = 0;
                if (key.Username != null) {
                    usernameHashCode = key.Username.ToLowerInvariant().GetHashCode();
                }
                return HashCodeCombiner.CombineHashCodes(pathHashCode, usernameHashCode);
            }

            private int Compare(Key x, Key y) {
                if (x == null && y == null) {
                    return 0;
                }
                if (x == null) {
                    return -1;
                }
                if (y == null) {
                    return 1;
                }

                int pathDiff = String.Compare(x.Path, y.Path,
                                              StringComparison.OrdinalIgnoreCase);
                if (pathDiff != 0) {
                    return pathDiff;
                }
                else {
                    return String.Compare(x.Username, y.Username,
                                          StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }
}
