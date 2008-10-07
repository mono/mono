//
// EntitySet.cs
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
	public sealed class EntitySet<TEntity> : IList, ICollection, IList<TEntity>, ICollection<TEntity>, IEnumerable<TEntity>, IEnumerable, IListSource where TEntity : class
	{
		public EntitySet ()
		{
		}

		[MonoTODO]
		public EntitySet (Action<TEntity> onAdd, Action<TEntity> onRemove)
		{
			throw new NotImplementedException ();
		}

		public event ListChangedEventHandler ListChanged;

		[MonoTODO]
		public void Add (TEntity entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IList.Add (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddRange (IEnumerable<TEntity> collection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Assign (IEnumerable<TEntity> entitySource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (TEntity entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IList.Contains (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (TEntity [] array, int arrayIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ICollection.CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerator<TEntity> GetEnumerator ()
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
		public IBindingList GetNewBindingList ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf (TEntity entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IList.IndexOf (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert (int index, TEntity entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.Insert (int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Load ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Remove (TEntity entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.Remove (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSource (IEnumerable<TEntity> entitySource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IListSource.ContainsListCollection {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int Count {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool HasLoadedOrAssignedValues {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool IsDeferred {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool IList.IsFixedSize {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ICollection<TEntity>.IsReadOnly {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool IList.IsReadOnly {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ICollection.IsSynchronized {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public TEntity this [int index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		object IList.this [int index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		object ICollection.SyncRoot {
			get { throw new NotImplementedException (); }
		}
	}
}
