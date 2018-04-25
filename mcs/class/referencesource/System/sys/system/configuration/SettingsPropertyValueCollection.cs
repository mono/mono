//------------------------------------------------------------------------------
// <copyright file="SettingsPropertyValueCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using  System.Collections;
    using  System.Collections.Specialized;
    using  System.Runtime.Serialization;
    using  System.Configuration.Provider;
    using  System.Globalization;
    using  System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Reflection;

    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////
    public class SettingsPropertyValueCollection : IEnumerable, ICloneable, ICollection
    {
        private Hashtable _Indices = null;

        private ArrayList _Values = null;

        private bool _ReadOnly = false;

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public SettingsPropertyValueCollection()
        {
            _Indices = new Hashtable(10, StringComparer.CurrentCultureIgnoreCase);
            _Values = new ArrayList();
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public void Add(SettingsPropertyValue property)
        {
            if (_ReadOnly)
                throw new NotSupportedException();

            int pos = _Values.Add(property);

            try
            {
                _Indices.Add(property.Name, pos);
            }
            catch (Exception)
            {
                _Values.RemoveAt(pos);
                throw;
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public void Remove(string name)
        {
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

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public SettingsPropertyValue this[string name]
        {
            get
            {
                object pos = _Indices[name];

                if (pos == null || !(pos is int))
                    return null;

                int ipos = (int)pos;

                if (ipos >= _Values.Count)
                    return null;

                return (SettingsPropertyValue)_Values[ipos];
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public IEnumerator GetEnumerator()
        {
            return _Values.GetEnumerator();
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public object Clone()
        {
            return new SettingsPropertyValueCollection(_Indices, _Values);
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public void SetReadOnly()
        {
            if (_ReadOnly)
                return;

            _ReadOnly = true;
            _Values = ArrayList.ReadOnly(_Values);
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public void Clear()
        {
            _Values.Clear();
            _Indices.Clear();
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // ICollection interface
        public int Count { get { return _Values.Count; } }

        public bool IsSynchronized { get { return false; } }

        public object SyncRoot { get { return this; } }

        public void CopyTo(Array array, int index)
        {
            _Values.CopyTo(array, index);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private SettingsPropertyValueCollection(Hashtable indices, ArrayList values)
        {
            _Indices = (Hashtable)indices.Clone();
            _Values = (ArrayList)values.Clone();
        }
    }
}
