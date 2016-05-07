//------------------------------------------------------------------------------
// <copyright file="HttpListenerPrefixCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class ListenerPrefixEnumerator:IEnumerator<string>{
        IEnumerator enumerator;

        internal ListenerPrefixEnumerator(IEnumerator enumerator){
            this.enumerator = enumerator;
        }

        public string Current{
            get{
                return (string)enumerator.Current;
            }
        }
        public bool MoveNext(){
            return enumerator.MoveNext();
        }

        public void Dispose(){
        }

        void System.Collections.IEnumerator.Reset(){
            enumerator.Reset();
        }

        object System.Collections.IEnumerator.Current{
            get{
                return enumerator.Current;
            }
        }
    }


    public class HttpListenerPrefixCollection : ICollection<string> {
        private HttpListener m_HttpListener;

        internal HttpListenerPrefixCollection(HttpListener listener) {
            m_HttpListener = listener;
        }

        public void CopyTo(Array array, int offset) {
            m_HttpListener.CheckDisposed();
            if (Count>array.Length) {
                throw new ArgumentOutOfRangeException("array", SR.GetString(SR.net_array_too_small));
            }
            if (offset+Count>array.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            int index = 0;
            foreach (string uriPrefix in m_HttpListener.m_UriPrefixes.Keys) {
                array.SetValue(uriPrefix, offset + index++);
            }
        }

        public void CopyTo(string[] array, int offset) {
            m_HttpListener.CheckDisposed();
            if (Count>array.Length) {
                throw new ArgumentOutOfRangeException("array", SR.GetString(SR.net_array_too_small));
            }
            if (offset+Count>array.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            int index = 0;
            foreach (string uriPrefix in m_HttpListener.m_UriPrefixes.Keys) {
                array[offset + index++] = uriPrefix;
            }
        }
        
        public int Count {
            get {
                return m_HttpListener.m_UriPrefixes.Count;
            }
        }
        public bool IsSynchronized {
            get {
                return false;
            }
        }
        
        public bool IsReadOnly {
            get {
                return false;
            }
        }

        public void Add(string uriPrefix) {
            m_HttpListener.AddPrefix(uriPrefix);
        }

        public bool Contains(string uriPrefix) {
            return m_HttpListener.m_UriPrefixes.Contains(uriPrefix);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator() {
            return new ListenerPrefixEnumerator(m_HttpListener.m_UriPrefixes.Keys.GetEnumerator());
        }

        public bool Remove(string uriPrefix) {
            return m_HttpListener.RemovePrefix(uriPrefix);
        }

        public void Clear() {
            m_HttpListener.RemoveAll(true);
        }
    }
}

