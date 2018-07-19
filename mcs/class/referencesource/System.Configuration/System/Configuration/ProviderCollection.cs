//------------------------------------------------------------------------------
// <copyright file="ProviderCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration.Provider {
    using  System.Collections;
    using  System.Collections.Specialized;
    using  System.Runtime.Serialization;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>

    public class ProviderCollection : IEnumerable, ICollection //, ICloneable
    {
        private Hashtable _Hashtable   = null;
        private bool      _ReadOnly = false;

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public ProviderCollection()
        {
            _Hashtable = new Hashtable(10, StringComparer.OrdinalIgnoreCase);
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public virtual void Add(ProviderBase provider)
        {
            if (_ReadOnly)
                throw new NotSupportedException(SR.GetString(SR.CollectionReadOnly));

            if (provider == null)
                throw new ArgumentNullException("provider");

            if (provider.Name == null || provider.Name.Length < 1)
                throw new ArgumentException(SR.GetString(SR.Config_provider_name_null_or_empty));

            _Hashtable.Add(provider.Name, provider);
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public void Remove(string name)
        {
            if (_ReadOnly)
                throw new NotSupportedException(SR.GetString(SR.CollectionReadOnly));
            _Hashtable.Remove(name);
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public ProviderBase this[string name]
        {
            get {
                return _Hashtable[name] as ProviderBase;
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public IEnumerator GetEnumerator()
        {
            return _Hashtable.Values.GetEnumerator();
        }
        //public object Clone(){
        //    return new ProviderCollection(_Indices, _Values);
        //}

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public void SetReadOnly()
        {
            if (_ReadOnly)
                return;
            _ReadOnly = true;
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public void Clear()
        {
            if (_ReadOnly)
                throw new NotSupportedException(SR.GetString(SR.CollectionReadOnly));
            _Hashtable.Clear();
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // ICollection interface
        public int      Count           { get { return _Hashtable.Count; }}
        public bool     IsSynchronized  { get { return false; } }
        public object   SyncRoot        { get { return this; } }

        public void     CopyTo(ProviderBase[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            _Hashtable.Values.CopyTo(array, index);
        }

#if UNUSED_CODE
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private ProviderCollection(Hashtable h)
        {
            _Hashtable = (Hashtable)h.Clone();
        }
#endif
    }
}
