//
// Table.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Linq
{
	public sealed class Table<TEntity> : IQueryable<TEntity>, IQueryProvider, IEnumerable<TEntity>, ITable, IQueryable, IEnumerable, IListSource where TEntity : class
	{
		internal Table ()
		{
		}

		[MonoTODO]
		public DataContext Context {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool IsReadOnly {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool IListSource.ContainsListCollection {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		Type IQueryable.ElementType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		Expression IQueryable.Expression {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		IQueryProvider IQueryable.Provider {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public void Attach (TEntity entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Attach (TEntity entity, bool asModified)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Attach (TEntity entity, TEntity original)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AttachAll<TSubEntity> (IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AttachAll<TSubEntity> (IEnumerable<TSubEntity> entities, bool asModified) where TSubEntity : TEntity
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DeleteAllOnSubmit<TSubEntity> (IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DeleteOnSubmit (TEntity entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerator<TEntity> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IBindingList GetNewBindingList ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ModifiedMemberInfo [] GetModifiedMembers (TEntity entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public TEntity GetOriginalEntityState (TEntity entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InsertAllOnSubmit<TSubEntity> (IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InsertOnSubmit (TEntity entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ITable.Attach (object entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ITable.Attach (object entity, bool asModified)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ITable.Attach (object entity, object original)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ITable.AttachAll (IEnumerable entities)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ITable.AttachAll (IEnumerable entities, bool asModified)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ITable.DeleteAllOnSubmit (IEnumerable entities)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ITable.DeleteOnSubmit (object entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		ModifiedMemberInfo [] ITable.GetModifiedMembers (object entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ITable.GetOriginalEntityState (object entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ITable.InsertAllOnSubmit (IEnumerable entities)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ITable.InsertOnSubmit (object entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IList IListSource.GetList ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IQueryable<TResult> IQueryProvider.CreateQuery<TResult> (Expression expression)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IQueryable IQueryProvider.CreateQuery (Expression expression)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		TResult IQueryProvider.Execute<TResult> (Expression expression)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object IQueryProvider.Execute (Expression expression)
		{
			throw new NotImplementedException ();
		}

	}
}
