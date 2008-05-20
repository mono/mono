//
// DataContext.cs
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
using System.Data.Common;
using System.Data.Linq.Mapping;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.Data.Linq
{
	public class DataContext : IDisposable
	{
		[MonoTODO]
		public DataContext (IDbConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataContext (string fileOrServerOrConnection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataContext (IDbConnection connection, MappingSource mapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataContext (string fileOrServerOrConnection, MappingSource mapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ChangeConflictCollection ChangeConflicts {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int CommandTimeout {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DbConnection Connection {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool DeferredLoadingEnabled {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataLoadOptions LoadOptions {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public TextWriter Log {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MetaModel Mapping {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool ObjectTrackingEnabled {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DbTransaction Transaction {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public void CreateDatabase ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal IQueryable<TResult> CreateMethodCallQuery<TResult> (object instance, MethodInfo methodInfo, params object [] parameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool DatabaseExists ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DeleteDatabase ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int ExecuteCommand (string command, params object [] parameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void ExecuteDynamicDelete (object entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void ExecuteDynamicInsert (object entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void ExecuteDynamicUpdate (object entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal IExecuteResult ExecuteMethodCall (object instance, MethodInfo methodInfo, params object [] parameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerable<TResult> ExecuteQuery<TResult> (string query, params object [] parameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerable ExecuteQuery (Type elementType, string query, params object [] parameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ChangeSet GetChangeSet ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbCommand GetCommand (IQueryable query)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Table<TEntity> GetTable<TEntity> () where TEntity : class
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ITable GetTable (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Refresh (RefreshMode mode, IEnumerable entities)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Refresh (RefreshMode mode, object entity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Refresh (RefreshMode mode, params object [] entities)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SubmitChanges ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SubmitChanges (ConflictMode failureMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerable<TResult> Translate<TResult> (DbDataReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IMultipleResults Translate (DbDataReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerable Translate (Type elementType, DbDataReader reader)
		{
			throw new NotImplementedException ();
		}
	}
}
