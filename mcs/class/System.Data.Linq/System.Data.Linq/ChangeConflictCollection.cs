using System.Collections;
using System.Collections.Generic;

namespace System.Data.Linq
{
    public sealed class ChangeConflictCollection : ICollection<ObjectChangeConflict>, ICollection
    {
        #region .ctor
        internal ChangeConflictCollection()
        {
            list = new List<ObjectChangeConflict>();
        }
        #endregion

        #region Fields
        private List<ObjectChangeConflict> list;
        #endregion

        #region Properties

        public int Count
        {
            get { return list.Count; }
        }

        public ObjectChangeConflict this[int index]
        {
            get { return list[index]; }
        }

        #endregion

        #region ICollection<ObjectChangeConflict> Implementations
        bool ICollection<ObjectChangeConflict>.IsReadOnly
        {
            get { return true; }
        }

        void ICollection<ObjectChangeConflict>.Add(ObjectChangeConflict item)
        {
            throw new NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((ObjectChangeConflict[])array, index);
        }
        #endregion

        #region IEnumerable Implementations

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region ICollection Implementations
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return null; }
        }
        #endregion

        #region Public Methods

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(ObjectChangeConflict item)
        {
            return list.Contains(item);
        }

        public void CopyTo(ObjectChangeConflict[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(ObjectChangeConflict item)
        {
            return list.Remove(item);
        }

        public IEnumerator<ObjectChangeConflict> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion
    }
}