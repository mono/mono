//------------------------------------------------------------------------------
// <copyright file="SessionStateItemCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * SessionStateItemCollection
 *
 * Copyright (c) 1998-1999, Microsoft Corporation
 *
 */

namespace System.Web.SessionState {

    using System.IO;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.Util;
    using System.Security;
    using System.Security.Permissions;
    using System.Diagnostics.CodeAnalysis;

    public interface ISessionStateItemCollection : ICollection {

        Object this[String name]
        {
            get;
            set;
        }

        Object this[int index]
        {
            get;
            set;
        }

        void Remove(String name);

        void RemoveAt(int index);

        void Clear();

        NameObjectCollectionBase.KeysCollection Keys {
            get;
        }

        bool Dirty {
            get;
            set;
        }
    }

    public sealed class SessionStateItemCollection : NameObjectCollectionBase, ISessionStateItemCollection {

        class KeyedCollection : NameObjectCollectionBase {

            internal KeyedCollection(int count) : base(count, Misc.CaseInsensitiveInvariantKeyComparer) {
            }

            internal Object this[String name]
            {
                get {
                    return BaseGet(name);
                }

                set {
                    Object oldValue = BaseGet(name);
                    if (oldValue == null && value == null)
                        return;

                    BaseSet(name, value);
                }
            }

            internal Object this[int index]
            {
                get {
                    return BaseGet(index);
                }
            }

            internal void Remove(String name) {
                BaseRemove(name);
            }

            internal void RemoveAt(int index) {
                BaseRemoveAt(index);
            }

            internal void Clear() {
                BaseClear();
            }

            internal string GetKey(  int index) {
                return BaseGetKey(index);
            }

            internal bool ContainsKey(string name) {
                // Please note that we don't expect null value to be inserted.
                return (BaseGet(name) != null);
            }
        }

        class SerializedItemPosition {
            int _offset;
            int _dataLength;

            internal SerializedItemPosition(int offset, int dataLength) {
                this._offset = offset;
                this._dataLength = dataLength;
            }

            internal int Offset {
                get { return _offset; }
            }

            internal int DataLength {
                get { return _dataLength; }
            }

            // Mark the item as deserialized by making the offset -1.
            internal void MarkDeserializedOffset() {
                _offset = -1;
            }

            internal void MarkDeserializedOffsetAndCheck() {
                if (_offset >= 0) {
                    MarkDeserializedOffset();
                }
                else {
                    Debug.Fail("Offset shouldn't be negative inside MarkDeserializedOffsetAndCheck.");
                }
            }

            internal bool IsDeserialized {
                get { return _offset < 0; }
            }
        }

        static Hashtable s_immutableTypes;
        const int       NO_NULL_KEY = -1;
        const int       SIZE_OF_INT32 = 4;
        bool            _dirty;
        KeyedCollection _serializedItems;
        Stream          _stream;
        int             _iLastOffset;
        object          _serializedItemsLock = new object();

        public SessionStateItemCollection() : base(Misc.CaseInsensitiveInvariantKeyComparer) {
        }

        static SessionStateItemCollection() {
            Type t;
            s_immutableTypes = new Hashtable(19);

            t=typeof(String);
            s_immutableTypes.Add(t, t);
            t=typeof(Int32);
            s_immutableTypes.Add(t, t);
            t=typeof(Boolean);
            s_immutableTypes.Add(t, t);
            t=typeof(DateTime);
            s_immutableTypes.Add(t, t);
            t=typeof(Decimal);
            s_immutableTypes.Add(t, t);
            t=typeof(Byte);
            s_immutableTypes.Add(t, t);
            t=typeof(Char);
            s_immutableTypes.Add(t, t);
            t=typeof(Single);
            s_immutableTypes.Add(t, t);
            t=typeof(Double);
            s_immutableTypes.Add(t, t);
            t=typeof(SByte);
            s_immutableTypes.Add(t, t);
            t=typeof(Int16);
            s_immutableTypes.Add(t, t);
            t=typeof(Int64);
            s_immutableTypes.Add(t, t);
            t=typeof(UInt16);
            s_immutableTypes.Add(t, t);
            t=typeof(UInt32);
            s_immutableTypes.Add(t, t);
            t=typeof(UInt64);
            s_immutableTypes.Add(t, t);
            t=typeof(TimeSpan);
            s_immutableTypes.Add(t, t);
            t=typeof(Guid);
            s_immutableTypes.Add(t, t);
            t=typeof(IntPtr);
            s_immutableTypes.Add(t, t);
            t=typeof(UIntPtr);
            s_immutableTypes.Add(t, t);
        }

        static internal bool IsImmutable(Object o) {
            return s_immutableTypes[o.GetType()] != null;
        }

        internal void DeserializeAllItems() {
            if (_serializedItems == null) {
                return;
            }

            lock (_serializedItemsLock) {
                for (int i = 0; i < _serializedItems.Count; i++) {
                    DeserializeItem(_serializedItems.GetKey(i), false);
                }
            }
        }

        void DeserializeItem(int index) {
            // No-op if SessionStateItemCollection is not deserialized from a persistent storage.
            if (_serializedItems == null) {
                return;
            }

#if DBG
            // The keys in _serializedItems should match the beginning part of
            // the list in NameObjectCollectionBase
            for (int i=0; i < _serializedItems.Count; i++) {
                Debug.Assert(_serializedItems.GetKey(i) == BaseGetKey(i));
            }
#endif

            lock (_serializedItemsLock) {
                // No-op if the item isn't serialized.
                if (index >= _serializedItems.Count) {
                    return;
                }

                DeserializeItem(_serializedItems.GetKey(index), false);
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        private object ReadValueFromStreamWithAssert() {
            return AltSerialization.ReadValueFromStream(new BinaryReader(_stream));
        }

        void DeserializeItem(String name, bool check) {
            object          val;

            lock (_serializedItemsLock) {
                if (check) {
                    // No-op if SessionStateItemCollection is not deserialized from a persistent storage,
                    if (_serializedItems == null) {
                        return;
                    }

                    // User is asking for an item we don't have.
                    if (!_serializedItems.ContainsKey(name)) {
                        return;
                    }
                }

                Debug.Assert(_serializedItems != null);
                Debug.Assert(_stream != null);

                SerializedItemPosition position = (SerializedItemPosition)_serializedItems[name];
                if (position.IsDeserialized) {
                    // It has been deserialized already.
                    return;
                }

                // Position the stream to the place where the item is stored.
                _stream.Seek(position.Offset, SeekOrigin.Begin);

                // Set the value
                Debug.Trace("SessionStateItemCollection", "Deserialized an item: keyname=" + name);

                if (!HttpRuntime.DisableProcessRequestInApplicationTrust) {
                    // VSWhidbey 427316: Sandbox Serialization in non full trust cases
                    if (HttpRuntime.NamedPermissionSet != null && HttpRuntime.ProcessRequestInApplicationTrust) {
                        HttpRuntime.NamedPermissionSet.PermitOnly();
                    }
                }

                // This deserialization work used to be done in AcquireRequestState event when
                // there is no user code on the stack.
                // In whidbey we added this on-demand deserialization for performance reason.  However,
                // in medium and low trust cases the page doesn't have permission to do it.
                // So we have to assert the permission.
                // (See VSWhidbey 275003)
                val = ReadValueFromStreamWithAssert();

                BaseSet(name, val);

                // At the end, mark the item as deserialized by making the offset -1
                position.MarkDeserializedOffsetAndCheck();
            }

        }

        void MarkItemDeserialized(String name) {
            // No-op if SessionStateItemCollection is not deserialized from a persistent storage,
            if (_serializedItems == null) {
                return;
            }

            lock (_serializedItemsLock) {
                // If the serialized collection contains this key, mark it deserialized
                if (_serializedItems.ContainsKey(name)) {
                    // Mark the item as deserialized by making it -1.
                    ((SerializedItemPosition)_serializedItems[name]).MarkDeserializedOffset();
                }
            }
        }

        void MarkItemDeserialized(int index) {
            // No-op if SessionStateItemCollection is not deserialized from a persistent storage,
            if (_serializedItems == null) {
                return;
            }

#if DBG
            // The keys in _serializedItems should match the beginning part of
            // the list in NameObjectCollectionBase
            for (int i=0; i < _serializedItems.Count; i++) {
                Debug.Assert(_serializedItems.GetKey(i) == BaseGetKey(i));
            }
#endif

            lock (_serializedItemsLock) {
                // No-op if the item isn't serialized.
                if (index >= _serializedItems.Count) {
                    return;
                }

               ((SerializedItemPosition)_serializedItems[index]).MarkDeserializedOffset();
            }
        }

        public bool Dirty {
            get {return _dirty;}
            set {_dirty = value;}
        }

        public Object this[String name]
        {
            get {
                DeserializeItem(name, true);

                Object obj = BaseGet(name);
                if (obj != null) {
                    if (!IsImmutable(obj)) {
                        // If the item is immutable (e.g. an array), then the caller has the ability to change
                        // its content without calling our setter.  So we have to mark the collection
                        // as dirty.
                        Debug.Trace("SessionStateItemCollection", "Setting _dirty to true in get");
                        _dirty = true;
                    }
                }

                return obj;
            }

            set {
                MarkItemDeserialized(name);
                Debug.Trace("SessionStateItemCollection", "Setting _dirty to true in set");
                BaseSet(name, value);
                _dirty = true;
            }
        }

        public Object this[int index]
        {
            get {
                DeserializeItem(index);

                Object obj = BaseGet(index);
                if (obj != null) {
                    if (!IsImmutable(obj)) {
                        Debug.Trace("SessionStateItemCollection", "Setting _dirty to true in get");
                        _dirty = true;
                    }
                }

                return obj;
            }

            set {
                MarkItemDeserialized(index);
                Debug.Trace("SessionStateItemCollection", "Setting _dirty to true in set");
                BaseSet(index, value);
                _dirty = true;
            }
        }


        public void Remove(String name) {
            lock (_serializedItemsLock) {
                if (_serializedItems != null) {
                    _serializedItems.Remove(name);
                }

                BaseRemove(name);
                _dirty = true;
            }
        }

        public void RemoveAt(int index) {
            lock (_serializedItemsLock) {
                if (_serializedItems != null && index < _serializedItems.Count) {
                    _serializedItems.RemoveAt(index);
                }

                BaseRemoveAt(index);
                _dirty = true;
            }
        }

        public void Clear() {
            lock (_serializedItemsLock) {
                if (_serializedItems != null) {
                    _serializedItems.Clear();
                }
                BaseClear();
                _dirty = true;
            }
        }

        public override IEnumerator GetEnumerator() {
            // Have to deserialize all items; otherwise the enumerator won't
            // work because we'll keep on changing the collection during
            // individual item deserialization
            DeserializeAllItems();

            return base.GetEnumerator();
        }

        public override NameObjectCollectionBase.KeysCollection Keys {
            get {
                // Unfortunately, we have to deserialize all items first, because
                // Keys.GetEnumerator might be called and we have the same problem
                // as in GetEnumerator() above.
                DeserializeAllItems();

                return base.Keys;
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        private void WriteValueToStreamWithAssert(object value, BinaryWriter writer) {
            AltSerialization.WriteValueToStream(value, writer);
        }

        [SuppressMessage("Microsoft.Security", "CA2107:ReviewDenyAndPermitOnlyUsage",
           Justification = "Not a new FxCop warning suppression -- this proped up again because Serialize(BinaryWriter writer) function was changed Serialize(BinaryWriter writer, bool assertSerializationFormatterPermission)")]
        public void Serialize(BinaryWriter writer) {
            int     count;
            int     i;
            long    iOffsetStart;
            long    iValueStart;
            string  key;
            object  value;
            long    curPos;
            byte[]  buffer = null;
            Stream  baseStream = writer.BaseStream;

            if (!HttpRuntime.DisableProcessRequestInApplicationTrust) {
                // VSWhidbey 427316: Sandbox Serialization in non full trust cases
                if (HttpRuntime.NamedPermissionSet != null && HttpRuntime.ProcessRequestInApplicationTrust) {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }
            }

            lock (_serializedItemsLock) {
                count = Count;
                writer.Write(count);

                if (count > 0) {
                    if (BaseGet(null) != null) {
                        // We have a value with a null key.  Find its index.
                        for (i = 0; i < count; i++) {
                            key = BaseGetKey(i);
                            if (key == null) {
                                writer.Write(i);
                                break;
                            }
                        }

                        Debug.Assert(i != count);
                    }
                    else {
                        writer.Write(NO_NULL_KEY);
                    }

                    // Write out all the keys.
                    for (i = 0; i < count; i++) {
                        key = BaseGetKey(i);
                        if (key != null) {
                            writer.Write(key);
                        }
                    }

                    // Next, allocate space to store the offset:
                    // - We won't store the offset of first item because it's always zero.
                    // - The offset of an item is counted from the beginning of serialized values
                    // - But we will store the offset of the first byte off the last item because
                    //   we need that to calculate the size of the last item.
                    iOffsetStart = baseStream.Position;
                    baseStream.Seek(SIZE_OF_INT32 * count, SeekOrigin.Current);

                    iValueStart = baseStream.Position;

                    for (i = 0; i < count; i++) {
                        // See if that item has not be deserialized yet.
                        if (_serializedItems != null &&
                            i < _serializedItems.Count &&
                            !((SerializedItemPosition)_serializedItems[i]).IsDeserialized) {

                            SerializedItemPosition position = (SerializedItemPosition)_serializedItems[i];

                            Debug.Assert(_stream != null);

                            // The item is read as serialized data from a store, and it's still
                            // serialized, meaning no one has referenced it.  Just copy
                            // the bytes over.

                            // Move the stream to the serialized data and copy it over to writer
                            _stream.Seek(position.Offset, SeekOrigin.Begin);

                            if (buffer == null || buffer.Length < position.DataLength) {
                                buffer = new Byte[position.DataLength];
                            }
#if DBG
                            int read =
#endif
                            _stream.Read(buffer, 0, position.DataLength);
#if DBG
                            Debug.Assert(read == position.DataLength);
#endif

                            baseStream.Write(buffer, 0, position.DataLength);
                        }
                        else {
                            value = BaseGet(i);
                            WriteValueToStreamWithAssert(value, writer);
                        }

                        curPos = baseStream.Position;

                        // Write the offset
                        baseStream.Seek(i * SIZE_OF_INT32 + iOffsetStart, SeekOrigin.Begin);
                        writer.Write((int)(curPos - iValueStart));

                        // Move back to current position
                        baseStream.Seek(curPos, SeekOrigin.Begin);

                        Debug.Trace("SessionStateItemCollection",
                            "Serialize: curPost=" + curPos + ", offset= " + (int)(curPos - iValueStart));
                    }
                }
#if DBG
                writer.Write((byte)0xff);
#endif
            }
        }

        public static SessionStateItemCollection Deserialize(BinaryReader reader) {
            SessionStateItemCollection   d = new SessionStateItemCollection();
            int                 count;
            int                 nullKey;
            String              key;
            int                 i;
            byte[]              buffer;

            count = reader.ReadInt32();

            if (count > 0) {
                nullKey = reader.ReadInt32();

                d._serializedItems = new KeyedCollection(count);

                // First, deserialize all the keys
                for (i = 0; i < count; i++) {
                    if (i == nullKey) {
                        key = null;
                    }
                    else {
                        key = reader.ReadString();
                    }

                    // Need to set them with null value first, so that
                    // the order of them items is correct.
                    d.BaseSet(key, null);
                }

                // Next, deserialize all the offsets
                // First offset will be 0, and the data length will be the first read offset
                int offset0 = reader.ReadInt32();
                d._serializedItems[d.BaseGetKey(0)] = new SerializedItemPosition(0, offset0);

                int offset1 = 0;
                for (i = 1; i < count; i++) {
                    offset1 = reader.ReadInt32();
                    d._serializedItems[d.BaseGetKey(i)] = new SerializedItemPosition(offset0, offset1 - offset0);
                    offset0 = offset1;
                }

                // 
                d._iLastOffset = offset0;

                Debug.Trace("SessionStateItemCollection",
                    "Deserialize: _iLastOffset= " + d._iLastOffset);

                // _iLastOffset is the first byte past the last item, which equals
                // the total length of all serialized data
                buffer = new byte[d._iLastOffset];
                int bytesRead = reader.BaseStream.Read(buffer, 0, d._iLastOffset);
                if (bytesRead != d._iLastOffset) {
                    throw new HttpException(SR.GetString(SR.Invalid_session_state));
                }
                d._stream = new MemoryStream(buffer);
            }

    #if DBG
            Debug.Assert(reader.ReadByte() == 0xff);
    #endif

            d._dirty = false;

            return d;
        }
    }
}
