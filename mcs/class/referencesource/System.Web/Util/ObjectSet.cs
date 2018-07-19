//------------------------------------------------------------------------------
// <copyright file="ObjectSet.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * ObjectSet class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

// Generics are causing perf regressions, so don't use them for now until we can figure
// it out (VSWhidbey 463572)
//#define USEGENERICSET

namespace System.Web.Util {

using System.Reflection;
using System.Collections;
using System.Collections.Generic;

#if USEGENERICSET
/*
 * Holds a set of unique objects of a specific type
 */
internal class ObjectSet<T> : ICollection<T>, ICollection {

    protected const int StartingCapacity = 8;

    private class EmptyEnumerator : IEnumerator<T> {
        object IEnumerator.Current { get { return null; } }
        T IEnumerator<T>.Current { get { return default(T); } }
        bool IEnumerator.MoveNext() { return false; }
        void IEnumerator.Reset() { }
        void IDisposable.Dispose() { }
    }

    private static EmptyEnumerator _emptyEnumerator = new EmptyEnumerator();
    private Dictionary<T, object> _objects;

    protected virtual Dictionary<T, object> CreateDictionary() {
        return new Dictionary<T, object>(StartingCapacity);
    }

    public void AddCollection(ICollection c) {
        foreach (T o in c) {
            Add(o);
        }
    }

    public void Add(T o) {
        if (_objects == null) {
            _objects = CreateDictionary();
        }

        _objects[o] = null;
    }

    public bool Remove(T o) {
        if (_objects == null)
            return false;

        return _objects.Remove(o);
    }

    public bool Contains(T o) {
        if (_objects == null)
            return false;

        return _objects.ContainsKey(o);
    }

    bool ICollection<T>.IsReadOnly {
        get {
            return true;
        }
    }

    public void Clear() {
        if (_objects != null)
            _objects.Clear();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        if (_objects == null)
            return _emptyEnumerator;

        return _objects.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        if (_objects == null)
            return _emptyEnumerator;

        return _objects.Keys.GetEnumerator();
    }

    public int Count {
        get {
            if (_objects == null)
                return 0;
            return _objects.Keys.Count;
        }
    }

    void ICollection<T>.CopyTo(T[] array, int index) {
        if (_objects != null)
            _objects.Keys.CopyTo(array, index);
    }

    bool ICollection.IsSynchronized {
        get {
            if (_objects == null)
                return true;
            return ((ICollection)_objects.Keys).IsSynchronized;
        }
    }

    object ICollection.SyncRoot {
        get {
            if (_objects == null)
                return this;
            return ((ICollection)_objects.Keys).SyncRoot;
        }
    }

    public void CopyTo(Array array, int index) {
        if (_objects != null)
            ((ICollection)_objects.Keys).CopyTo(array, index);
    }
}

internal class StringSet : ObjectSet<String> { }

internal class CaseInsensitiveStringSet : StringSet {
    protected override Dictionary<String, object> CreateDictionary() {
        return new Dictionary<String, object>(StartingCapacity, StringComparer.InvariantCultureIgnoreCase);
    }
}

internal class VirtualPathSet : ObjectSet<VirtualPath> { }

internal class AssemblySet : ObjectSet<Assembly> {
    internal static AssemblySet Create(ICollection c) {
        AssemblySet objectSet = new AssemblySet();
        objectSet.AddCollection(c);
        return objectSet;
    }
}

internal class BuildProviderSet : ObjectSet<System.Web.Compilation.BuildProvider> { }

internal class ControlSet : ObjectSet<System.Web.UI.Control> { }
#else

/*
 * Holds a set of unique objects
 */
internal class ObjectSet: ICollection {

    private class EmptyEnumerator: IEnumerator {
        public object Current { get { return null; } }
        public bool MoveNext() { return false; }
        public void Reset() {}
    }

    private static EmptyEnumerator _emptyEnumerator = new EmptyEnumerator();
    private IDictionary _objects;

    internal ObjectSet() {}

    // By default, it's case sensitive
    protected virtual bool CaseInsensitive { get { return false; } }

    public void Add(object o) {
        if (_objects == null)
            _objects = new System.Collections.Specialized.HybridDictionary(CaseInsensitive);

        _objects[o] = null;
    }

    public void AddCollection(ICollection c) {
        foreach (object o in c) {
            Add(o);
        }
    }

    public void Remove(object o) {
        if (_objects == null)
            return;

        _objects.Remove(o);
    }

    public bool Contains(object o) {
        if (_objects == null)
            return false;

        return _objects.Contains(o);
    }

    IEnumerator IEnumerable.GetEnumerator() {
        if (_objects == null)
            return _emptyEnumerator;

        return _objects.Keys.GetEnumerator();
    }

    public int Count {
        get {
            if (_objects == null)
                return 0;
            return _objects.Keys.Count;
        }
    }

    bool ICollection.IsSynchronized {
        get {
            if (_objects == null)
                return true;
            return _objects.Keys.IsSynchronized;
        }
    }

    object ICollection.SyncRoot {
        get {
            if (_objects == null)
                return this;
            return _objects.Keys.SyncRoot;
        }
    }

    public void CopyTo(Array array, int index) {
        if (_objects != null)
            _objects.Keys.CopyTo(array, index);
    }
}

internal class StringSet: ObjectSet {
    internal StringSet() {}
}

internal class CaseInsensitiveStringSet: StringSet {
    protected override bool CaseInsensitive { get { return true; } }
}

internal class VirtualPathSet : ObjectSet {
    internal VirtualPathSet() { }
}

internal class AssemblySet : ObjectSet {
    internal AssemblySet() { }

    internal static AssemblySet Create(ICollection c) {
        AssemblySet objectSet = new AssemblySet();
        objectSet.AddCollection(c);
        return objectSet;
    }
}

internal class BuildProviderSet : ObjectSet {
    internal BuildProviderSet() { }
}

internal class ControlSet : ObjectSet {
    internal ControlSet() { }
}

#endif

}
