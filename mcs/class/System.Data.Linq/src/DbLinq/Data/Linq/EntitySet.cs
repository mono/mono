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
        private Action<TEntity> onAdd;
        private Action<TEntity> onRemove;

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


        [DbLinqToDo]
        public EntitySet(Action<TEntity> onAdd, Action<TEntity> onRemove)
            : this()
        {
            this.onAdd = onAdd;
            this.onRemove = onRemove;
        }

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


        IEnumerator IEnumerable.GetEnumerator()
        {
            return Source.GetEnumerator();
        }

        

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
        public int IndexOf(TEntity entity)
        {
            return sourceAsList.IndexOf(entity);
        }

        public void Insert(int index, TEntity entity)
        {
            sourceAsList.Insert(index, entity);
        }

        public void RemoveAt(int index)
        {
            sourceAsList.RemoveAt(index);
        }

        public TEntity this[int index]
        {
            get
            {
                return Source.ElementAt(index);
            }
            set
            {
                sourceAsList[index] = value;
            }
        }

        #endregion

        #region ICollection<TEntity> Members

        public void Clear()
        {
            Source = Enumerable.Empty<TEntity>();
        }

        public bool Contains(TEntity entity)
        {
            return Source.Contains(entity);
        }

        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            array = this.Source.Skip(arrayIndex).ToArray();
        }

        public int Count
        {
            get
            {
                return sourceAsList.Count;
            }
        }

        bool ICollection<TEntity>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(TEntity entity)
        {
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

        public bool IsDeferred
        {
            get { return Source is IQueryable; }
        }

        public void AddRange(IEnumerable<TEntity> collection)
        {
            sourceAsList.AddRange(collection);
        }

        public void Assign(IEnumerable<TEntity> entitySource)
        {
            this.Source = entitySource;
            HasLoadedOrAssignedValues = true;
        }

        public void SetSource(IEnumerable<TEntity> entitySource)
        {
            this.Source = entitySource;
        }

        public void Load()
        {
            this.sourceAsList.Count();
        }

        public bool HasLoadedOrAssignedValues
        {
            get;
            private set;
        }

        public IBindingList GetNewBindingList()
        {
            return new BindingList<TEntity>(Source.ToList());
        }

        public event ListChangedEventHandler ListChanged;
    }
}
