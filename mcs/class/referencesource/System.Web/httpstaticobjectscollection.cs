//------------------------------------------------------------------------------
// <copyright file="httpstaticobjectscollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Static objects collection for application and session state
 * with deferred creation
 */
namespace System.Web {
    using System.Runtime.InteropServices;

    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Web;
    using System.Web.Util;
    using System.Security.Permissions;

    //
    // Static objects collection class
    //

    /// <devdoc>
    ///    <para>Provides a static objects collection for the 
    ///       HTTPApplicationState.StaticObjects and TemplateParser.SessionObjects properties.</para>
    /// </devdoc>
    public sealed class HttpStaticObjectsCollection : ICollection {
        private IDictionary _objects = new Hashtable(StringComparer.OrdinalIgnoreCase);


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.HttpStaticObjectsCollection'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        public HttpStaticObjectsCollection() {
        }

        internal void Add(String name, Type t, bool lateBound) {
            _objects.Add(name, new HttpStaticObjectsEntry(name, t, lateBound));
        }

        // Expose the internal dictionary (for codegen purposes)
        internal IDictionary Objects {
            get { return _objects;}
        }

        /*
         * Create a copy without copying instances (names and types only)
         */
        internal HttpStaticObjectsCollection Clone() {
            HttpStaticObjectsCollection c = new HttpStaticObjectsCollection();

            IDictionaryEnumerator e = _objects.GetEnumerator();

            while (e.MoveNext()) {
                HttpStaticObjectsEntry entry = (HttpStaticObjectsEntry)e.Value;
                c.Add(entry.Name, entry.ObjectType, entry.LateBound);
            }

            return c;
        }

        /*
         * Get number of real instances created
         */
        internal int GetInstanceCount() {
            int count = 0;

            IDictionaryEnumerator e = _objects.GetEnumerator();

            while (e.MoveNext()) {
                HttpStaticObjectsEntry entry = (HttpStaticObjectsEntry)e.Value;

                if (entry.HasInstance)
                    count++;
            }

            return count;
        }


        /// <devdoc>
        /// </devdoc>
        public bool NeverAccessed {
            get {return (GetInstanceCount() == 0);}
        }

        //
        // Implementation of standard collection stuff
        //


        /// <devdoc>
        ///    <para>Gets the object in the collection with the given name (case 
        ///       insensitive). </para>
        /// </devdoc>
        public Object this[String name]
        {
            get {
                HttpStaticObjectsEntry e = (HttpStaticObjectsEntry)_objects[name];
                return(e != null) ? e.Instance : null;
            }
        }

        // Alternative to the default property

        /// <devdoc>
        ///    <para>Gets the object in the collection with the given name 
        ///       (case insensitive). Alternative to the <paramref name="this"/> accessor.</para>
        /// </devdoc>
        public Object GetObject(String name) {
            return this[name];
        }


        /// <devdoc>
        ///    <para>Gets the number of objects in the collection.</para>
        /// </devdoc>
        public int Count {
            get {
                return _objects.Count;
            }
        }


        /// <devdoc>
        ///    <para>Returns a dictionary enumerator used for iterating through the key/value 
        ///       pairs contained in the collection.</para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return new HttpStaticObjectsEnumerator((IDictionaryEnumerator)_objects.GetEnumerator());
        }


        /// <devdoc>
        ///    <para>Copies members of the collection into an array.</para>
        /// </devdoc>
        public void CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }


        /// <devdoc>
        ///    <para>Gets an object that can be used to synchronize access to the collection.</para>
        /// </devdoc>
        public Object SyncRoot {
            get { return this;}
        }


        /// <devdoc>
        ///    <para>Gets a value indicating whether the collection is read-only.</para>
        /// </devdoc>
        public bool IsReadOnly {
            get { return true;}
        }


        /// <devdoc>
        ///    <para>Gets a value indicating whether the collection is synchronized.</para>
        /// </devdoc>
        public bool IsSynchronized {
            get { return false;}
        }


        public void Serialize(BinaryWriter writer) {
            IDictionaryEnumerator   e;
            HttpStaticObjectsEntry  entry;
            bool                    hasInstance;

            writer.Write(Count);

            e = _objects.GetEnumerator();
            while (e.MoveNext()) {
                entry = (HttpStaticObjectsEntry)e.Value;
                writer.Write(entry.Name);
                hasInstance = entry.HasInstance;
                writer.Write(hasInstance);

                if (hasInstance) {
                    AltSerialization.WriteValueToStream(entry.Instance, writer);
                }
                else {
                    writer.Write(entry.ObjectType.FullName);
                    writer.Write(entry.LateBound);
                }
            }
        }


        static public HttpStaticObjectsCollection Deserialize(BinaryReader reader) {
            int     count;
            string  name;
            string  typename;
            bool    hasInstance;
            Object  instance;
            HttpStaticObjectsEntry  entry;
            HttpStaticObjectsCollection col;

            col = new HttpStaticObjectsCollection();

            count = reader.ReadInt32();
            while (count-- > 0) {
                name = reader.ReadString();
                hasInstance = reader.ReadBoolean();
                if (hasInstance) {
                    instance = AltSerialization.ReadValueFromStream(reader);
                    entry = new HttpStaticObjectsEntry(name, instance, 0);
                }
                else {
                    typename = reader.ReadString();
                    bool lateBound = reader.ReadBoolean();
                    entry = new HttpStaticObjectsEntry(name, Type.GetType(typename), lateBound);
                }

                col._objects.Add(name, entry);
            }

            return col;
        }
    }

//
// Dictionary entry class
//

    internal class HttpStaticObjectsEntry {
        private String _name;
        private Type   _type;
        private bool   _lateBound;
        private Object _instance;

        internal HttpStaticObjectsEntry(String name, Type t, bool lateBound) {
            _name = name;
            _type = t;
            _lateBound = lateBound;
            _instance = null;
        }

        internal HttpStaticObjectsEntry(String name, Object instance, int dummy) {
            _name = name;
            _type = instance.GetType();
            _instance = instance;
        }

        internal String Name {
            get { return _name;} 
        }

        internal Type ObjectType {
            get { return _type;} 
        }

        internal bool LateBound {
            get { return _lateBound;} 
        }

        internal Type DeclaredType {
            get { return _lateBound ? typeof(object) : ObjectType; }
        }

        internal bool HasInstance {
            get { return(_instance != null);}
        }

        internal Object Instance {
            get {
                if (_instance == null) {
                    lock (this) {
                        if (_instance == null)
                            _instance = Activator.CreateInstance(_type);
                    }
                }

                return _instance;
            }
        }
    }

    //
    // Enumerator class for static objects collection
    //
    internal class HttpStaticObjectsEnumerator : IDictionaryEnumerator {
        private IDictionaryEnumerator _enum;

        internal HttpStaticObjectsEnumerator(IDictionaryEnumerator e) {
            _enum = e;
        }

        // Dictionary enumarator implementation

        public void Reset() {
            _enum.Reset();
        }

        public bool MoveNext() {
            return _enum.MoveNext();
        }

        public Object Key {
            get {
                return _enum.Key;
            }
        }

        public Object Value {
            get {
                HttpStaticObjectsEntry e = (HttpStaticObjectsEntry)_enum.Value;

                if (e == null)
                    return null;

                return e.Instance;
            }
        }

        public Object Current {
            get {
                return Entry;
            }
        }

        public DictionaryEntry Entry {
            get {
                return new DictionaryEntry(_enum.Key, this.Value);
            }
        }
    }

}
