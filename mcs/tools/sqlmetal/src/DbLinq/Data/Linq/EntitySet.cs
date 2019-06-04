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
using System.Reflection;
using DbLinq;
using DbLinq.Util;

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

        private IEnumerable<TEntity> deferredSource;
        private bool deferred;
		private bool assignedValues;
        private List<TEntity> source;
        private List<TEntity> Source
        {
            get
            {
                if (source != null)
                    return source;
                if (deferredSource != null)
                    return source = deferredSource.ToList();
                if (nestedQueryPredicate != null && context != null)
                {
                    var otherTable = context.GetTable(typeof(TEntity));
                    var query = (IQueryable<TEntity>) context.GetOtherTableQuery(nestedQueryPredicate, nestedQueryParam, typeof(TEntity), otherTable);
                    return source = query.ToList();
                }
                return source = new List<TEntity>();
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance has loaded or assigned values.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has loaded or assigned values; otherwise, <c>false</c>.
        /// </value>
        public bool HasAssignedValues
        {
			get { return assignedValues; }
        }

        public bool HasLoadedValues
        {
            get { return source != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has loaded or assigned values.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has loaded or assigned values; otherwise, <c>false</c>.
        /// </value>
        public bool HasLoadedOrAssignedValues
        {
            get { return HasLoadedValues || HasAssignedValues; }
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
        }

        DataContext context;
        internal EntitySet(DataContext context)
            : this()
        {
            this.context = context;
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<TEntity> GetEnumerator()
        {
            deferred = false;
            return Source.GetEnumerator();
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
                if (deferred && this.deferredSource is IQueryable<TEntity>)
                    return (deferredSource as IQueryable<TEntity>).Expression;
                else
                    return Expression.Constant(this);
            }
        }

        /// <summary>
        /// Adds a row
        /// </summary>
        public void Add(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (Source.Contains (entity))
                return;
            Source.Add(entity);
            OnAdd(entity);
            ListChangedEventHandler handler = ListChanged;
            if (!deferred && deferredSource != null && handler != null)
                handler(this, new ListChangedEventArgs(ListChangedType.ItemAdded, Source.Count - 1));
        }

        ParameterExpression nestedQueryParam;
        BinaryExpression nestedQueryPredicate;
        internal void Add(KeyValuePair<object, MemberInfo> info)
        {
            var value = info.Key;
            var member = info.Value;
            if (nestedQueryParam == null)
                nestedQueryParam = Expression.Parameter(typeof(TEntity), "other");
            var propType = member.GetMemberType();
            BinaryExpression comp;
            if (!propType.IsNullable())
            {
                comp = Expression.Equal(Expression.Constant(value),
                        Expression.MakeMemberAccess(nestedQueryParam, member));
            }
            else
            {
                var valueProp = propType.GetProperty("Value");
                comp = Expression.Equal(Expression.Constant(value),
                        Expression.MakeMemberAccess(
                            Expression.MakeMemberAccess(nestedQueryParam, member),
                            valueProp));
            }
            nestedQueryPredicate = nestedQueryPredicate == null
                ? comp
                : Expression.And(nestedQueryPredicate, comp);
        }

        [DbLinqToDo]
        bool IListSource.ContainsListCollection
        {
            get { throw new NotImplementedException(); }
        }

        IList IListSource.GetList()
        {
			//It seems that Microsoft is doing a similar thing in L2SQL, matter of fact, after doing a GetList().Add(new TEntity()), HasAssignedValues continues to be false
			//This seems like a bug on their end, but we'll do the same for consistency
            return this;
        }

        #region IList<TEntity> Members

        /// <summary>
        /// Returns entity's index
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public int IndexOf(TEntity entity)
        {
            deferred = false;
            return Source.IndexOf(entity);
        }

        /// <summary>
        /// Inserts entity at specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="entity">The entity.</param>
        public void Insert(int index, TEntity entity)
        {
            if (Source.Contains(entity))
                throw new ArgumentOutOfRangeException();
            OnAdd(entity);
            deferred = false;
            Source.Insert(index, entity);
            ListChangedEventHandler handler = ListChanged;
            if (handler != null)
                handler(this, new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }

        /// <summary>
        /// Removes entity at specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            deferred = false;
            var item = Source[index];
            Source.RemoveAt(index);
            OnRemove(item);
            ListChangedEventHandler handler = ListChanged;
            if (handler != null)
                handler(this, new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        }

        /// <summary>
        /// Gets or sets the <see cref="TEntity"/> at the specified index.
        /// </summary>
        /// <value></value>
        public TEntity this[int index]
        {
            get
            {
                deferred = false;
                return Source[index];
            }
            set
            {
                OnRemove(Source[index]);
                OnAdd(value);
                deferred = false;
                var handler = ListChanged;
                if (handler != null)
                {
                    handler(this, new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
                    handler(this, new ListChangedEventArgs(ListChangedType.ItemAdded, index));
                }
                Source[index] = value;
            }
        }

        #endregion

        #region ICollection<TEntity> Members

        /// <summary>
        /// Removes all items in collection
        /// </summary>
        public void Clear()
        {
            ListChangedEventHandler handler = ListChanged;
            deferred = false;
			assignedValues = true;
            if (deferredSource != null && handler != null)
            {
                foreach (var item in Source)
                    handler(this, new ListChangedEventArgs(ListChangedType.ItemDeleted, 0));
            }
            if (handler != null)
                handler(this, new ListChangedEventArgs(ListChangedType.Reset, 0));
            Source.Clear();
        }

        /// <summary>
        /// Determines whether [contains] [the specified entity].
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///     <c>true</c> if [contains] [the specified entity]; otherwise, <c>false</c>.
        /// </returns>
        [DbLinqToDo]
        public bool Contains(TEntity entity)
        {
            deferred = false;
            return Source.Contains(entity);
        }

        /// <summary>
        /// Copies items to target array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            deferred = false;
            Source.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns entities count
        /// </summary>
        public int Count
        {
            get
            {
                deferred = false;
                return Source.Count;
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
            int i = Source.IndexOf(entity);
            if(i < 0)
                return false;
            deferred = false;
            Source.Remove(entity);
            OnRemove(entity);
            ListChangedEventHandler handler = ListChanged;
            if (deferredSource != null && handler != null)
                handler(this, new ListChangedEventArgs(ListChangedType.ItemDeleted, i));
            return true;
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            var v = value as TEntity;
            if (v != null && !Contains(v))
            {
                Add(v);
                return Count - 1;
            }
            throw new ArgumentOutOfRangeException("value");
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            var v = value as TEntity;
            if (v != null)
                return Contains(v);
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
            for (int i = 0; i < Source.Count; ++i)
            {
                array.SetValue(this[i], index + i);
            }
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
        ///     <c>true</c> if this instance is deferred; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeferred
        {
            get { return deferred; }
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public void AddRange(IEnumerable<TEntity> collection)
        {
            foreach (var entity in collection)
            {
                Add(entity);
            }
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
            {
                OnAdd(entity);
            }
            this.source = entitySource.ToList();
            // this.SourceInUse = sourceAsList;
        }

        /// <summary>
        /// Sets the entity source.
        /// </summary>
        /// <param name="entitySource">The entity source.</param>
        public void SetSource(IEnumerable<TEntity> entitySource)
        {
#if false
            Console.WriteLine("# EntitySet<{0}>.SetSource: HashCode={1}; Stack={2}", typeof(TEntity).Name,
                GetHashCode(), new System.Diagnostics.StackTrace());
#endif
            if(HasLoadedOrAssignedValues)
                throw new InvalidOperationException("The EntitySet is already loaded and the source cannot be changed.");
            deferred = true;
            deferredSource = entitySource;
        }

        /// <summary>
        /// Loads all entities.
        /// </summary>
        public void Load()
        {
            deferred = false;
            var _ = Source;
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
			assignedValues = true;
            if (onAdd != null)
                onAdd(entity);
        }
        /// <summary>
        /// Called when entity is removed
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void OnRemove(TEntity entity)
        {
			assignedValues = true;
            if (onRemove != null)
                onRemove(entity);
        }
    }
}
