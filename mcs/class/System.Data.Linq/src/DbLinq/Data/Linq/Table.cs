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
using System.Data;
using System.Data.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;

#if MONO_STRICT
using System.Data.Linq.Implementation;
using System.Data.Linq.Sugar;
using ITable = System.Data.Linq.ITable;
#else
using DbLinq.Data.Linq.Implementation;
using DbLinq.Data.Linq.Sugar;
using ITable = DbLinq.Data.Linq.ITable;
#endif

using DbLinq;


#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    /// <summary>
    /// T may be eg. class Employee or string - the output
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public sealed partial class Table<TEntity> :
            ITable,
            IQueryProvider,
            IListSource,
            IEnumerable<TEntity>,
            IEnumerable,
            IQueryable<TEntity>,
            IQueryable
            where TEntity : class
    {
        /// <summary>
        /// the parent DataContext holds our connection etc
        /// </summary>
        public DataContext Context { get; private set; }

        // QueryProvider is the running entity, running through nested Expressions
        private readonly QueryProvider<TEntity> _queryProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Table&lt;TEntity&gt;"/> class.
        /// </summary>
        /// <param name="parentContext">The parent context.</param>
        internal Table(DataContext parentContext)
        {
            Context = parentContext;
            _queryProvider = new QueryProvider<TEntity>(parentContext);
        }

        /// <summary>
        /// 'S' is the projected type. If you say 'from e in Employees select e.ID', then type S will be int.
        /// If you say 'select new {e.ID}', then type S will be something like Projection.f__1
        /// </summary>
        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expr)
        {
            return _queryProvider.CreateQuery<S>(expr);
        }

        /// <summary>
        /// this is only called during Dynamic Linq
        /// </summary>
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            return _queryProvider.CreateQuery(expression);
        }

        /// <summary>
        /// the query '(from o in Orders select o).First()' enters here
        /// </summary>
        S IQueryProvider.Execute<S>(Expression expression)
        {
            return _queryProvider.Execute<S>(expression);
        }

        /// <summary>
        /// Executes the current expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        object IQueryProvider.Execute(Expression expression)
        {
            return _queryProvider.Execute(expression);
        }

        /// <summary>
        /// entry point for 'foreach' statement.
        /// </summary>
        public IEnumerator<TEntity> GetEnumerator()
        {
            var queryable = this as IQueryable<TEntity>;
            var query = queryable.Select(t => t);
            return query.GetEnumerator();
        }

        /// <summary>
        /// Enumerates all table items
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            var enumT = GetEnumerator();
            return enumT;
        }

        Type IQueryable.ElementType
        {
            get { return _queryProvider.ElementType; }
        }

        /// <summary>
        /// Returns this table as an Expression
        /// </summary>
        Expression IQueryable.Expression
        {
            get { return Expression.Constant(this); } // do not change this to _queryProvider.Expression, Sugar doesn't fully handle QueryProviders by now
        }

        /// <summary>
        /// IQueryable.Provider: represents the Table as a IQueryable provider (hence the name)
        /// </summary>
        IQueryProvider IQueryable.Provider
        {
            get { return _queryProvider.Provider; }
        }

        #region Insert functions

        void ITable.InsertOnSubmit(object entity)
        {
            Context.RegisterInsert(entity);
        }

        public void InsertOnSubmit(TEntity entity)
        {
            Context.RegisterInsert(entity);
        }

        void ITable.InsertAllOnSubmit(IEnumerable entities)
        {
            foreach (var entity in entities)
                Context.RegisterInsert(entity);
        }

        public void InsertAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            foreach (var entity in entities)
                Context.RegisterInsert(entity);
        }

        #endregion

        #region Delete functions

        void ITable.DeleteAllOnSubmit(IEnumerable entities)
        {
            foreach (var entity in entities)
                Context.RegisterDelete(entity);
        }

        /// <summary>
        /// required by ITable interface
        /// </summary>
        /// <param name="entity"></param>
        void ITable.DeleteOnSubmit(object entity)
        {
            Context.RegisterDelete(entity);
        }

        public void DeleteOnSubmit(TEntity entity)
        {
            Context.RegisterDelete(entity);
        }

        public void DeleteAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            foreach (var row in entities)
                Context.RegisterDelete(row);
        }

        #endregion

        #region Attach functions

        /// <summary>
        /// required for ITable
        /// </summary>
        /// <param name="entity"></param>
        void ITable.Attach(object entity)
        {
            Context.RegisterUpdate(entity);
        }

        void ITable.Attach(object entity, object original)
        {
            Context.RegisterUpdate(entity, original);
        }

        void ITable.Attach(object entity, bool asModified)
        {
            Context.RegisterUpdate(entity, asModified ? null : entity);
        }

        void ITable.AttachAll(IEnumerable entities)
        {
            foreach (var entity in entities)
                Context.RegisterUpdate(entity);
        }
        void ITable.AttachAll(IEnumerable entities, bool asModified)
        {
            foreach (var entity in entities)
                Context.RegisterUpdate(entity);
        }

        /// <summary>
        /// Attaches an entity from another Context to a table,
        /// with the intention to perform an update or delete operation
        /// </summary>
        /// <param name="entity">table row object to attach</param>
        public void Attach(TEntity entity)
        {
            Context.RegisterUpdate(entity);
        }

        [DbLinqToDo]
        public void Attach(TEntity entity, bool asModified)
        {
            throw new NotImplementedException();
        }

        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            foreach (var entity in entities)
                Context.RegisterUpdate(entity);
        }

        [DbLinqToDo]
        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities, bool asModified) where TSubEntity : TEntity
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attaches existing entity with original state
        /// </summary>
        /// <param name="entity">live entity added to change tracking</param>
        /// <param name="original">original unchanged property values</param>
        public void Attach(TEntity entity, TEntity original)
        {
            Context.RegisterUpdate(entity, original);
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly { get { return false; } }

        // PC: this will probably required to recreate a new object instance with all original values
        //     (that we currently do not always store, so we may need to make a differential copy
        [Obsolete("NOT IMPLEMENTED YET")]
        [DbLinqToDo]
        ModifiedMemberInfo[] ITable.GetModifiedMembers(object entity)
        {
            throw new ApplicationException("L579 Not implemented");
        }

        // PC: complementary to GetModifiedMembers(), we probably need a few changes to the IMemberModificationHandler,
        //     to recall original values
        [Obsolete("NOT IMPLEMENTED YET")]
        [DbLinqToDo]
        object ITable.GetOriginalEntityState(object entity)
        {
            throw new ApplicationException("L585 Not implemented");
        }

        bool IListSource.ContainsListCollection
        {
            get { return true; }
        }

        IList IListSource.GetList()
        {
            return this.ToList();
        }

        [DbLinqToDo]
        public TEntity GetOriginalEntityState(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public IBindingList GetNewBindingList()
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public ModifiedMemberInfo[] GetModifiedMembers(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("Table({0})", typeof(TEntity).Name);
        }
    }
}
