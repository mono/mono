#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using DbLinq;

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    public sealed class EntitySet<TEntity> : ICollection, ICollection<TEntity>, IEnumerable, IEnumerable<TEntity>, IList, IList<TEntity>, IListSource
        where TEntity : class
    {
        private readonly Action<TEntity> onAdd;
        private readonly Action<TEntity> onRemove;

        private IEnumerable<TEntity> Source;
        private List<TEntity> sourceAsList
        {
            get
            {
                if (!(Source is List<TEntity>))
                {
                    Source = Source.ToList();
                    HasLoadedOrAssignedValues = true;
                }
                return Source as List<TEntity>;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySet&lt;TEntity&gt;"/> class.
        /// </summary>
        /// <param name="onAdd">The on add.</param>
        /// <param name="onRemove">The on remove.</param>
        [DbLinqToDo]
        public EntitySet(Action<TEntity> onAdd, Action<TEntity> onRemove)
            : this()
        {
            this.onAdd = onAdd;
            this.onRemove = onRemove;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySet&lt;TEntity&gt;"/> class.
        /// </summary>
        public EntitySet()
        {
            Source = System.Linq.Enumerable.Empty<TEntity>();
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return Source.GetEnumerator();
            //vars = GetVars();
            //IEnumerator<T> enumerator = new RowEnumerator<T>(vars);
            //return (IEnumerator<T>)enumerator;
        }

        /// <summary>
        /// Enumerates all entities
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the source expression (used to nest queries)
        /// </summary>
        /// <value>The expression.</value>
        internal Expression Expression
        {
            get
            {
                if (this.Source is IQueryable<TEntity>)
                    return (Source as IQueryable<TEntity>).Expression;
                else
                    return Expression.Constant(this);
            }
        }

        /// <summary>
        /// TODO: Add(row)
        /// </summary>
        public void Add(TEntity entity)
        {
            OnAdd(entity);
            sourceAsList.Add(entity);
        }

        [DbLinqToDo]
        bool IListSource.ContainsListCollection
        {
            get { throw new NotImplementedException(); }
        }

        IList IListSource.GetList()
        {
            return sourceAsList;
        }

        #region IList<TEntity> Members

        /// <summary>
        /// Returns entity's index
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public int IndexOf(TEntity entity)
        {
            return sourceAsList.IndexOf(entity);
        }

        /// <summary>
        /// Inserts entity at specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="entity">The entity.</param>
        public void Insert(int index, TEntity entity)
        {
            OnAdd(entity);
            sourceAsList.Insert(index, entity);
        }

        /// <summary>
        /// Removes entity as specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            OnRemove(sourceAsList[index]);
            sourceAsList.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the <see cref="TEntity"/> at the specified index.
        /// </summary>
        /// <value></value>
        public TEntity this[int index]
        {
            get
            {
                return Source.ElementAt(index);
            }
            set
            {
                OnRemove(sourceAsList[index]);
                OnAdd(value);
                sourceAsList[index] = value;
            }
        }

        #endregion

        #region ICollection<TEntity> Members

        /// <summary>
        /// Removes all items in collection
        /// </summary>
        public void Clear()
        {
            foreach (var entity in sourceAsList)
            {
                OnRemove(entity);
            }
            Source = Enumerable.Empty<TEntity>();
        }

        /// <summary>
        /// Determines whether [contains] [the specified entity].
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified entity]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TEntity entity)
        {
            return Source.Contains(entity);
        }

        /// <summary>
        /// Copies items to target array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            array = this.Source.Skip(arrayIndex).ToArray();
        }

        /// <summary>
        /// Returns entities count
        /// </summary>
        public int Count
        {
            get
            {
                return sourceAsList.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        bool ICollection<TEntity>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Removes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public bool Remove(TEntity entity)
        {
            OnRemove(entity);
            return sourceAsList.Remove(entity);
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            if (value is TEntity)
            {
                this.Add(value as TEntity);
                return this.IndexOf(value as TEntity);
            }
            else
                throw new NotSupportedException();
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            if (value is TEntity)
                return this.Contains(value as TEntity);
            else
                return false;
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf(value as TEntity);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, value as TEntity);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            this.Remove(value as TEntity);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = value as TEntity;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo(array as TEntity[], index);
        }

        int ICollection.Count
        {
            get { return this.Count; }
        }

        [DbLinqToDo]
        bool ICollection.IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        object ICollection.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether this instance is deferred.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is deferred; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeferred
        {
            get { return Source is IQueryable; }
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public void AddRange(IEnumerable<TEntity> collection)
        {
            if (onAdd != null)
            {
                foreach (var entity in collection)
                {
                    OnAdd(entity);
                }
            }
            sourceAsList.AddRange(collection);
        }

        /// <summary>
        /// Assigns the specified entity source.
        /// </summary>
        /// <param name="entitySource">The entity source.</param>
        public void Assign(IEnumerable<TEntity> entitySource)
        {
            // notifies removals and adds
            Clear();
            foreach (var entity in entitySource)
                OnAdd(entity);
            this.Source = entitySource;
            HasLoadedOrAssignedValues = true;
        }

        /// <summary>
        /// Sets the entity source.
        /// </summary>
        /// <param name="entitySource">The entity source.</param>
        public void SetSource(IEnumerable<TEntity> entitySource)
        {
            this.Source = entitySource;
        }

        /// <summary>
        /// Loads all entities.
        /// </summary>
        public void Load()
        {
            this.sourceAsList.Count();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has loaded or assigned values.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has loaded or assigned values; otherwise, <c>false</c>.
        /// </value>
        public bool HasLoadedOrAssignedValues
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a new binding list.
        /// </summary>
        /// <returns></returns>
        public IBindingList GetNewBindingList()
        {
            return new BindingList<TEntity>(Source.ToList());
        }

        // TODO: implement handler call
        public event ListChangedEventHandler ListChanged;

        /// <summary>
        /// Called when entity is added.
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void OnAdd(TEntity entity)
        {
            if (onAdd != null)
                onAdd(entity);
        }
        /// <summary>
        /// Called when entity is removed
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void OnRemove(TEntity entity)
        {
            if (onRemove != null)
                onRemove(entity);
        }
    }
}
