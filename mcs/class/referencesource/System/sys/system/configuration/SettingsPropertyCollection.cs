//------------------------------------------------------------------------------
// <copyright file="SettingsPropertyCollection.cs" company="Microsoft">
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

   ////////////////////////////////////////////////////////////
   ////////////////////////////////////////////////////////////
   ////////////////////////////////////////////////////////////
   public class SettingsPropertyCollection : IEnumerable, ICloneable, ICollection
   {
       private Hashtable _Hashtable = null;
       private bool      _ReadOnly = false;


       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       public SettingsPropertyCollection()
       {
           _Hashtable = new Hashtable(10, StringComparer.CurrentCultureIgnoreCase);
       }

       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       public void Add(SettingsProperty property)
       {
           if (_ReadOnly)
               throw new NotSupportedException();

           OnAdd(property);
           _Hashtable.Add(property.Name, property);
           try {
               OnAddComplete(property);
           }
           catch {
               _Hashtable.Remove(property.Name);
               throw;
           }
       }

       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       public void Remove(string name)
       {
           if (_ReadOnly)
               throw new NotSupportedException();
           SettingsProperty toRemove = (SettingsProperty)_Hashtable[name];
           if (toRemove == null)
               return;
           OnRemove(toRemove);
           _Hashtable.Remove(name);
           try {
               OnRemoveComplete(toRemove);
           }
           catch {
               _Hashtable.Add(name, toRemove);
               throw;
           }

       }

       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       public SettingsProperty this[string name]
       {
           get {
               return _Hashtable[name] as SettingsProperty;
           }
       }

       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       public IEnumerator GetEnumerator()
       {
           return _Hashtable.Values.GetEnumerator();
       }

       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       public object Clone()
       {
           return new SettingsPropertyCollection(_Hashtable);
       }

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
       public void Clear() {
           if (_ReadOnly)
               throw new NotSupportedException();
           OnClear();
           _Hashtable.Clear();
           OnClearComplete();
       }


       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       // On* Methods for deriving classes to override.  These have
       // been modeled after the CollectionBase class but have
       // been stripped of their "index" parameters as there is no
       // visible index to this collection.
       
       protected virtual void OnAdd(SettingsProperty property) {}

       protected virtual void OnAddComplete(SettingsProperty property) {}

       protected virtual void OnClear() {}

       protected virtual void OnClearComplete() {}

       protected virtual void OnRemove(SettingsProperty property) {}

       protected virtual void OnRemoveComplete(SettingsProperty property) {}


       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       // ICollection interface
       public int       Count           { get { return _Hashtable.Count; } }
       public bool      IsSynchronized  { get { return false; } }
       public object    SyncRoot        { get { return this; } }

       public void CopyTo(Array array, int index)
       {
           _Hashtable.Values.CopyTo(array, index);
       }

       /////////////////////////////////////////////////////////////////////////////
       /////////////////////////////////////////////////////////////////////////////
       private SettingsPropertyCollection(Hashtable h)
       {
           _Hashtable = (Hashtable)h.Clone();
       }
   }
}
