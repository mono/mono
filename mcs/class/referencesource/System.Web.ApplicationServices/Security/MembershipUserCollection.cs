//------------------------------------------------------------------------------
// <copyright file="MembershipUserCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security {

    using System;
    using System.Collections;
    using System.Configuration.Provider;
    using System.Runtime.CompilerServices;    

    [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [Serializable]
    public sealed class MembershipUserCollection : IEnumerable, ICollection {
        private Hashtable _Indices = null;
        private ArrayList _Values = null;
        private bool _ReadOnly = false;

        public MembershipUserCollection() {
            _Indices = new Hashtable(10, StringComparer.CurrentCultureIgnoreCase);
            _Values = new ArrayList();
        }

        public void Add(MembershipUser user) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }

            if (_ReadOnly)
                throw new NotSupportedException();

            int pos = _Values.Add(user);
            try {
                _Indices.Add(user.UserName, pos);
            }
            catch {
                _Values.RemoveAt(pos);
                throw;
            }
        }

        public void Remove(string name) {
            if (_ReadOnly)
                throw new NotSupportedException();

            object pos = _Indices[name];
            if (pos == null || !(pos is int))
                return;
            int ipos = (int)pos;
            if (ipos >= _Values.Count)
                return;
            _Values.RemoveAt(ipos);
            _Indices.Remove(name);
            ArrayList al = new ArrayList();
            foreach (DictionaryEntry de in _Indices)
                if ((int)de.Value > ipos)
                    al.Add(de.Key);
            foreach (string key in al)
                _Indices[key] = ((int)_Indices[key]) - 1;
        }

        public MembershipUser this[string name] {
            get {
                object pos = _Indices[name];
                if (pos == null || !(pos is int))
                    return null;
                int ipos = (int)pos;
                if (ipos >= _Values.Count)
                    return null;
                return (MembershipUser)_Values[ipos];
            }
        }

        public IEnumerator GetEnumerator() {
            return _Values.GetEnumerator();
        }

        public void SetReadOnly() {
            if (_ReadOnly)
                return;
            _ReadOnly = true;
            _Values = ArrayList.ReadOnly(_Values);
        }

        public void Clear() {
            _Values.Clear();
            _Indices.Clear();
        }

        public int Count { get { return _Values.Count; } }

        public bool IsSynchronized { get { return false; } }

        public object SyncRoot { get { return this; } }


        void ICollection.CopyTo(Array array, int index) {
            _Values.CopyTo(array, index);
        }

        public void CopyTo(MembershipUser[] array, int index) {
            _Values.CopyTo(array, index);
        }
    }
}
