using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq {
    internal static class SourceState<T> {
        internal static readonly IEnumerable<T> Loaded = (IEnumerable<T>)new T[] { };
        internal static readonly IEnumerable<T> Assigned = (IEnumerable<T>)new T[] { };
    }

    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "[....]: Types are never compared to each other.  When comparisons happen it is against the entities that are represented by these constructs.")]
    public struct Link<T> {
        T underlyingValue;
        IEnumerable<T> source;

        public Link(T value) {
            this.underlyingValue = value;
            this.source = null;
        }

        public Link(IEnumerable<T> source) {
            this.source = source;
            this.underlyingValue = default(T);
        }

        public Link(Link<T> link) {
            this.underlyingValue = link.underlyingValue;
            this.source = link.source;
        }

        public bool HasValue {
            get { return this.source == null || this.HasLoadedValue || this.HasAssignedValue; }
        }

        public bool HasLoadedOrAssignedValue {
            get { return this.HasLoadedValue || this.HasAssignedValue; }
        }

        internal bool HasLoadedValue {
            get { return this.source == SourceState<T>.Loaded; }
        }

        internal bool HasAssignedValue {
            get { return this.source == SourceState<T>.Assigned; }
        }

        internal T UnderlyingValue {
            get { return this.underlyingValue; }
        }

        internal IEnumerable<T> Source {
            get { return this.source; }
        }

        internal bool HasSource {
            get { return this.source != null && !this.HasAssignedValue && !this.HasLoadedValue; }
        }

        public T Value {
            get {
                if (this.HasSource) {
                    this.underlyingValue = Enumerable.SingleOrDefault(this.source);
                    this.source = SourceState<T>.Loaded;
                }
                return this.underlyingValue;
            }
            set {
                this.underlyingValue = value;
                this.source = SourceState<T>.Assigned;
            }
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification="[....]: Types are never compared to each other.  When comparisons happen it is against the entities that are represented by these constructs.")]
    public struct EntityRef<TEntity>
        where TEntity : class {
        IEnumerable<TEntity> source;
        TEntity entity;

        public EntityRef(TEntity entity) {
            this.entity = entity;
            this.source = SourceState<TEntity>.Assigned;
        }

        public EntityRef(IEnumerable<TEntity> source) {
            this.source = source;
            this.entity = default(TEntity);
        }

        public EntityRef(EntityRef<TEntity> entityRef) {
            this.source = entityRef.source;
            this.entity = entityRef.entity;
        }

        public TEntity Entity {
            get {
                if (this.HasSource) {
                    
                    IEnumerable<TEntity> src = this.source;
                    this.entity = Enumerable.SingleOrDefault(src);
                    this.source = SourceState<TEntity>.Loaded;
                }
                return this.entity;
            }
            set {
                this.entity = value;
                this.source = SourceState<TEntity>.Assigned;
            }
        }

        public bool HasLoadedOrAssignedValue {
            get { return this.HasLoadedValue || this.HasAssignedValue; }
        }

        internal bool HasValue {
            get { return this.source == null || this.HasLoadedValue || this.HasAssignedValue; }
        }

        internal bool HasLoadedValue {
            get { return this.source == SourceState<TEntity>.Loaded; }
        }

        internal bool HasAssignedValue {
            get { return this.source == SourceState<TEntity>.Assigned; }
        }

        internal bool HasSource {
            get { return this.source != null && !this.HasLoadedValue && !this.HasAssignedValue; }
        }

        internal IEnumerable<TEntity> Source {
            get { return this.source; }
        }

        internal TEntity UnderlyingValue {
            get { return this.entity; }
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification="[....]: Naming chosen to represent a different concept from a collection because it is delayed loaded.")]
    public sealed class EntitySet<TEntity> : IList, IList<TEntity>, IListSource
        where TEntity : class {
        IEnumerable<TEntity> source;
        ItemList<TEntity> entities;
        ItemList<TEntity> removedEntities;
        Action<TEntity> onAdd;
        Action<TEntity> onRemove;
        TEntity onAddEntity;
        TEntity onRemoveEntity;
        int version;
        private ListChangedEventHandler onListChanged;
        private bool isModified;
        private bool isLoaded;
        bool listChanged;

        public EntitySet() {
        }

        public EntitySet(Action<TEntity> onAdd, Action<TEntity> onRemove) {
            this.onAdd = onAdd;
            this.onRemove = onRemove;
        }

        internal EntitySet(EntitySet<TEntity> es, bool copyNotifications) {
            this.source = es.source;
            foreach (TEntity e in es.entities) entities.Add(e);
            foreach (TEntity e in es.removedEntities) removedEntities.Add(e);
            this.version = es.version;
            if (copyNotifications) {
                this.onAdd = es.onAdd;
                this.onRemove = es.onRemove;
            }
        }

        public int Count {
            get {
                Load();
                return entities.Count;
            }
        }

        public TEntity this[int index] {
            get {
                Load();
                if (index < 0 || index >= entities.Count)
                    throw Error.ArgumentOutOfRange("index");
                return entities[index];
            }
            set {
                Load();
                if (index < 0 || index >= entities.Count)
                    throw Error.ArgumentOutOfRange("index");
                if (value == null || IndexOf(value) >= 0)
                    throw Error.ArgumentOutOfRange("value");
                CheckModify();
                TEntity old = entities[index];
                OnRemove(old);
                OnListChanged(ListChangedType.ItemDeleted, index);

                OnAdd(value);
                entities[index] = value;
                OnModified();
                OnListChanged(ListChangedType.ItemAdded, index);
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#", Justification = "[....]: Naming the parameter entity makes it more discoverable because it is clear what type of data should be added to this collection.")]
        public void Add(TEntity entity) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            if (entity != onAddEntity) {
                CheckModify();
                if (!entities.Contains(entity)) {
                    OnAdd(entity);
                    if (this.HasSource) removedEntities.Remove(entity);
                    entities.Add(entity);
                    OnListChanged(ListChangedType.ItemAdded, entities.IndexOf(entity));
                }
                OnModified();
            }
        }

        public void AddRange(IEnumerable<TEntity> collection) {
            if (collection == null)
                throw Error.ArgumentNull("collection");
            CheckModify();
            // convert to List in case adding elements here removes them from the 'collection' (ie entityset to entityset assignment)
            collection = collection.ToList();
            foreach (TEntity e in collection) {
                if (!entities.Contains(e)) {
                    OnAdd(e);
                    if (this.HasSource) removedEntities.Remove(e);
                    entities.Add(e);
                    OnListChanged(ListChangedType.ItemAdded, entities.IndexOf(e));
                }
            }
            OnModified();
        }

        public void Assign(IEnumerable<TEntity> entitySource) {
            // No-op if assigning the same object to itself
            if (Object.ReferenceEquals(this, entitySource)) {
                return;
            }

            Clear();
            if (entitySource != null)
                AddRange(entitySource);

            // When an entity set is assigned, it is considered loaded.
            // Since with defer loading enabled, a load is triggered
            // anyways, this is only necessary in cases where defer loading
            // is disabled.  In such cases, the materializer assigns a 
            // prefetched collection and we want IsLoaded to be true.
            this.isLoaded = true;
        }

        public void Clear() {
            Load();
            CheckModify();
            if (entities.Items != null) {
                List<TEntity> removeList = new List<TEntity>(entities.Items);
                foreach (TEntity e in removeList) {
                    Remove(e);
                }
            }
            entities = default(ItemList<TEntity>);
            OnModified();
            OnListChanged(ListChangedType.Reset, 0);
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#", Justification = "[....]: Naming the parameter entity makes it more discoverable because it is clear what type of data should be added to this collection.")]
        public bool Contains(TEntity entity) {
            return IndexOf(entity) >= 0;
        }

        public void CopyTo(TEntity[] array, int arrayIndex) {
            Load();
            if (entities.Count > 0) Array.Copy(entities.Items, 0, array, arrayIndex, entities.Count);
        }

        public IEnumerator<TEntity> GetEnumerator() {
            Load();
            return new Enumerator(this);
        }

        internal IEnumerable<TEntity> GetUnderlyingValues() {
            return new UnderlyingValues(this);
        }

        class UnderlyingValues : IEnumerable<TEntity> {
            EntitySet<TEntity> entitySet;
            internal UnderlyingValues(EntitySet<TEntity> entitySet) {
                this.entitySet = entitySet;
            }
            public IEnumerator<TEntity> GetEnumerator() {
                return new Enumerator(this.entitySet);
            }
            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#", Justification = "[....]: Naming the parameter entity makes it more discoverable because it is clear what type of data should be added to this collection.")]
        public int IndexOf(TEntity entity) {
            Load();
            return entities.IndexOf(entity);
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "1#", Justification = "[....]: Naming the parameter entity makes it more discoverable because it is clear what type of data should be added to this collection.")]
        public void Insert(int index, TEntity entity) {
            Load();
            if (index < 0 || index > Count)
                throw Error.ArgumentOutOfRange("index");
            if (entity == null || IndexOf(entity) >= 0)
                throw Error.ArgumentOutOfRange("entity");
            CheckModify();
            entities.Insert(index, entity);
            OnListChanged(ListChangedType.ItemAdded, index);

            OnAdd(entity);
        }

        /// <summary>
        /// Returns true if this entity set has a deferred query
        /// that hasn't been executed yet.
        /// </summary>
        public bool IsDeferred
        {
            get { return HasSource; }
        }

        /// <summary>
        /// Returns true if values have been either assigned or loaded.
        /// </summary>
        internal bool HasValues {
            get { return this.source == null || this.HasAssignedValues || this.HasLoadedValues; }
        }

        /// <summary>
        /// Returns true if the entity set has been modified in any way by the user or its items
        /// have been loaded from the database.
        /// </summary>
        public bool HasLoadedOrAssignedValues {
            get { return this.HasAssignedValues || this.HasLoadedValues; }
        }

        /// <summary>   
        /// Returns true if the set has been modified in any way by the user.
        /// </summary>
        internal bool HasAssignedValues {
            get { return this.isModified; }
        }

        /// <summary>
        /// Returns true if the set has been loaded from the database.
        /// </summary>
        internal bool HasLoadedValues {
            get { return this.isLoaded; }
        }

        /// <summary>
        /// Returns true if the set has a deferred source query that hasn't been loaded yet.
        /// </summary>
        internal bool HasSource {
            get { return this.source != null && !this.HasLoadedValues; }
        }

        /// <summary>
        /// Returns true if the collection has been loaded.
        /// </summary>
        internal bool IsLoaded {
            get {
                return this.isLoaded;
            }
        }

        internal IEnumerable<TEntity> Source {
            get { return this.source; }
        }

        public void Load() {
            if (this.HasSource) {
                ItemList<TEntity> addedEntities = entities;
                entities = default(ItemList<TEntity>);
                foreach (TEntity e in source) entities.Add(e);
                foreach (TEntity e in addedEntities) entities.Include(e);
                foreach (TEntity e in removedEntities) entities.Remove(e);
                source = SourceState<TEntity>.Loaded;
                isLoaded = true;
                removedEntities = default(ItemList<TEntity>);
            }
        }

        private void OnModified() {
            isModified = true;
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#", Justification = "[....]: Naming the parameter entity makes it more discoverable because it is clear what type of data should be added to this collection.")]
        public bool Remove(TEntity entity) {
            if (entity == null || entity == onRemoveEntity) return false;
            CheckModify();
            int index = -1;
            bool removed = false;
            if (this.HasSource) {
                if (!removedEntities.Contains(entity)) {
                    OnRemove(entity);
                    // check in entities in case it has been pre-added
                    index = entities.IndexOf(entity);
                    if (index != -1) {
                        entities.RemoveAt(index);
                    }
                    else {
                        removedEntities.Add(entity);
                    }
                    removed = true;
                }
            } else {
                index = entities.IndexOf(entity);
                if (index != -1) {
                    OnRemove(entity);
                    entities.RemoveAt(index);
                    removed = true;
                }
            }
            if (removed) {
                OnModified();
                // If index == -1 here, that means that the entity was not in the list before Remove was called,
                // so we shouldn't fire the event since the list itself will not be changed, even though the Remove will still be tracked
                // on the removedEntities list in case a subsequent Load brings in this entity.
                if (index != -1) {                    
                    OnListChanged(ListChangedType.ItemDeleted, index);
                }
            }
            return removed;
        }

        public void RemoveAt(int index) {
            Load();
            if (index < 0 || index >= Count) {
                throw Error.ArgumentOutOfRange("index");
            }
            CheckModify();
            TEntity entity = entities[index];
            OnRemove(entity);
            entities.RemoveAt(index);
            OnModified();
            OnListChanged(ListChangedType.ItemDeleted, index);
        }

        public void SetSource(IEnumerable<TEntity> entitySource) {
            if (this.HasAssignedValues || this.HasLoadedValues)
                throw Error.EntitySetAlreadyLoaded();
            this.source = entitySource;
        }

        void CheckModify() {
            if (onAddEntity != null || onRemoveEntity != null)
                throw Error.ModifyDuringAddOrRemove();
            version++;
        }

        void OnAdd(TEntity entity) {
            if (onAdd != null) {
                TEntity e = onAddEntity;
                onAddEntity = entity;
                try {
                    onAdd(entity);
                } finally {
                    onAddEntity = e;
                }
            }
        }

        void OnRemove(TEntity entity) {
            if (onRemove != null) {
                TEntity e = onRemoveEntity;
                onRemoveEntity = entity;
                try {
                    onRemove(entity);
                } finally {
                    onRemoveEntity = e;
                }
            }
        }

        class Enumerable : IEnumerable<TEntity> {
            EntitySet<TEntity> entitySet;
            public Enumerable(EntitySet<TEntity> entitySet) {
                this.entitySet = entitySet;
            }
            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }
            public IEnumerator<TEntity> GetEnumerator() {
                return new Enumerator(this.entitySet);
            }
        }

        class Enumerator : IEnumerator<TEntity> {
            EntitySet<TEntity> entitySet;
            TEntity[] items;
            int index;
            int endIndex;
            int version;

            public Enumerator(EntitySet<TEntity> entitySet) {
                this.entitySet = entitySet;
                this.items = entitySet.entities.Items;
                this.index = -1;
                this.endIndex = entitySet.entities.Count - 1;
                this.version = entitySet.version;
            }

            public void Dispose()
            {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
            }

            public bool MoveNext() {
                if (version != entitySet.version)
                    throw Error.EntitySetModifiedDuringEnumeration();
                if (index == endIndex) return false;
                index++;
                return true;
            }

            public TEntity Current {
                get { return items[index]; }
            }

            object IEnumerator.Current {
                get { return items[index]; }
            }

            void IEnumerator.Reset() {
                if (version != entitySet.version)
                    throw Error.EntitySetModifiedDuringEnumeration();
                index = -1;
            }
        }

        int IList.Add(object value) {
            TEntity entity = value as TEntity;
            if (entity == null || IndexOf(entity) >= 0) {
                throw Error.ArgumentOutOfRange("value");
            }
            CheckModify();
            int i = entities.Count;
            entities.Add(entity);
            OnAdd(entity);
            return i;
        }

        bool IList.Contains(object value) {
            return Contains(value as TEntity);
        }

        int IList.IndexOf(object value) {
            return IndexOf(value as TEntity);
        }

        void IList.Insert(int index, object value) {
            TEntity entity = value as TEntity;
            if (value == null)
                throw Error.ArgumentOutOfRange("value");
            Insert(index, entity);
        }

        bool IList.IsFixedSize {
            get { return false; }
        }

        bool IList.IsReadOnly {
            get { return false; }
        }

        void IList.Remove(object value) {
            Remove(value as TEntity);
        }

        object IList.this[int index] {
            get {
                return this[index];
            }
            set {
                TEntity entity = value as TEntity;
                if (value == null) throw Error.ArgumentOutOfRange("value");
                this[index] = entity;
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            Load();
            if (entities.Count > 0) Array.Copy(entities.Items, 0, array, index, entities.Count);
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return this; }
        }

        bool ICollection<TEntity>.IsReadOnly {
            get { return false; }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        void OnListChanged(ListChangedType type, int index) {
            listChanged = true;
            if (onListChanged != null) {
                onListChanged(this, new ListChangedEventArgs(type, index));
            }
        }

        public event ListChangedEventHandler ListChanged {
            add {
                onListChanged += value;
            }
            remove {
                onListChanged -= value;
            }
        }

        bool IListSource.ContainsListCollection {
            get { return true; }
        }

        private IBindingList cachedList = null;

        IList IListSource.GetList() {
            if (cachedList == null || listChanged) {
                cachedList = GetNewBindingList();
                listChanged = false;
            }
            return cachedList;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification="Method doesn't represent a property of the type.")]
        public IBindingList GetNewBindingList() {
            return new EntitySetBindingList<TEntity>(this.ToList(), this);
        }
    }

    struct ItemList<T> where T : class {
        T[] items;
        int count;

        public int Count {
            get { return count; }
        }

        public T[] Items {
            get { return items; }
        }

        public T this[int index] {
            get { return items[index]; }
            set { items[index] = value; }
        }

        public void Add(T item) {
            if (items == null || items.Length == count) GrowItems();
            items[count] = item;
            count++;
        }

        public bool Contains(T item) {
            return IndexOf(item) >= 0;
        }

        public Enumerator GetEnumerator() {
            Enumerator e;
            e.items = items;
            e.index = -1;
            e.endIndex = count - 1;
            return e;
        }

        public bool Include(T item) {
            if (LastIndexOf(item) >= 0) return false;
            Add(item);
            return true;
        }

        public int IndexOf(T item) {
            for (int i = 0; i < count; i++) {
                if (items[i] == item) return i;
            }
            return -1;
        }

        public void Insert(int index, T item) {
            if (items == null || items.Length == count) GrowItems();
            if (index < count) Array.Copy(items, index, items, index + 1, count - index);
            items[index] = item;
            count++;
        }

        public int LastIndexOf(T item) {
            int i = count;
            while (i > 0) {
                --i;
                if (items[i] == item) return i;
            }
            return -1;
        }

        public bool Remove(T item) {
            int i = IndexOf(item);
            if (i < 0) return false;
            RemoveAt(i);
            return true;
        }

        public void RemoveAt(int index) {
            count--;
            if (index < count) Array.Copy(items, index + 1, items, index, count - index);
            items[count] = default(T);
        }

        void GrowItems() {
            Array.Resize(ref items, count == 0 ? 4 : count * 2);
        }

        public struct Enumerator {
            internal T[] items;
            internal int index;
            internal int endIndex;

            public bool MoveNext() {
                if (index == endIndex) return false;
                index++;
                return true;
            }

            public T Current {
                get { return items[index]; }
            }
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "[....]: The name clearly describes function and the namespace is under a DLinq namespace which will make the distinction clear.")]
    [DataContract]
    [Serializable]
    public sealed class Binary : IEquatable<Binary> {
        [DataMember(Name="Bytes")]
        byte[] bytes;
        int? hashCode;

        public Binary(byte[] value) {
            if (value == null) {
                this.bytes = new byte[0];
            }
            else {
                this.bytes = new byte[value.Length];
                Array.Copy(value, this.bytes, value.Length);
            }
            this.ComputeHash();
        }

        public byte[] ToArray() {
            byte[] copy = new byte[this.bytes.Length];
            Array.Copy(this.bytes, copy, copy.Length);
            return copy;
        }

        public int Length {
            get { return this.bytes.Length; }
        }

        public static implicit operator Binary(byte[] value) {
            return new Binary(value);
        }

        public bool Equals(Binary other) {
            return this.EqualsTo(other);
        }

        public static bool operator ==(Binary binary1, Binary binary2) {
            if ((object)binary1 == (object)binary2)
                return true;
            if ((object)binary1 == null && (object)binary2 == null)
                return true;
            if ((object)binary1 == null || (object)binary2 == null)
                return false;
            return binary1.EqualsTo(binary2);
        }

        public static bool operator !=(Binary binary1, Binary binary2) {
            if ((object)binary1 == (object)binary2)
                return false;
            if ((object)binary1 == null && (object)binary2 == null)
                return false;
            if ((object)binary1 == null || (object)binary2 == null)
                return true;
            return !binary1.EqualsTo(binary2);
        }

        public override bool Equals(object obj) {
            return this.EqualsTo(obj as Binary);
        }

        public override int GetHashCode() {
            if (!hashCode.HasValue) {
                // hash code is not marked [DataMember], so when
                // using the DataContractSerializer, we'll need
                // to recompute the hash after deserialization.
                ComputeHash();
            }
            return this.hashCode.Value;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            sb.Append(System.Convert.ToBase64String(this.bytes, 0, this.bytes.Length));
            sb.Append("\"");
            return sb.ToString();
        }

        private bool EqualsTo(Binary binary) {
            if ((object)this == (object)binary)
                return true;
            if ((object)binary == null)
                return false;
            if (this.bytes.Length != binary.bytes.Length)
                return false;
            if (this.GetHashCode() != binary.GetHashCode())
                return false;
            for (int i = 0, n = this.bytes.Length; i < n; i++) {
                if (this.bytes[i] != binary.bytes[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Simple hash using pseudo-random coefficients for each byte in 
        /// the array to achieve order dependency.
        /// </summary>
        private void ComputeHash() {
            int s = 314, t = 159;
            hashCode = 0;
            for (int i = 0; i < bytes.Length; i++) {
                hashCode = hashCode * s + bytes[i];
                s = s * t;
            }
        }
    }
}
