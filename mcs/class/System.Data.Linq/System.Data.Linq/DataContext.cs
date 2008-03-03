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
//
// Authors:
//        Antonello Provenzano  <antonello@deveel.com>
//

using System.Collections;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq.Provider;
using System.IO;
using System.Linq;

namespace System.Data.Linq
{
    public class DataContext : IDisposable
    {
        #region .ctor
        public DataContext(IDbConnection connection, MappingSource mapping)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            if (connection == null)
                throw new ArgumentNullException("mapping");

            this.conflicts = new ChangeConflictCollection();
            this.mapping = mapping;

            //TODO: init the IDataServices for the provider...

            MetaModel model = mapping.CreateModel(GetType());
            Type providerType = model.ProviderType;

            if (providerType == null)
                throw new InvalidOperationException();
            if (!typeof(IProvider).IsAssignableFrom(providerType))
                throw new InvalidOperationException();

            provider = (IProvider)Activator.CreateInstance(providerType, true);
            provider.Initialize(services, connection);
        }

        public DataContext(IDbConnection connection)
            : this(connection, new AttributeMappingSource())
        {
        }

        ~DataContext()
        {
            Dispose(false);
        }
        #endregion

        #region Fields
        private bool disposed;
        private IProvider provider;
        private MappingSource mapping;
        private bool objectTracking;
        private bool deferredLoading;
        private DataShape shape;
        private IDataServices services;
        private ChangeConflictCollection conflicts;
        #endregion

        #region Properties

        public ChangeConflictCollection ChangeConflicts
        {
            get
            {
                CheckIsDisposed();
                return conflicts;
            }
        }

        public IDbConnection Connection
        {
            get
            {
                CheckIsDisposed();
                return provider.Connection;
            }
        }

        public TextWriter Log
        {
            get
            {
                CheckIsDisposed();
                return provider.Log;
            }
            set
            {
                CheckIsDisposed();
                provider.Log = value;
            }
        }

        public bool ObjectTrackingEnabled
        {
            get
            {
                CheckIsDisposed();
                return objectTracking;
            }
            set
            {
                CheckIsDisposed();
                objectTracking = value;
                if (!value)
                    deferredLoading = false;
            }
        }
        #endregion

        #region Private Methods
        private void CheckIsDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
        #endregion

        #region Protected Methods
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (provider != null)
                {
                    provider.Dispose();
                    provider = null;
                }
                mapping = null;
                shape = null;
            }
        }
        #endregion

        #region Public Methods
        public void CreateDatabase()
        {
            CheckIsDisposed();
            provider.CreateDatabase();
        }

        public bool DatabaseExists()
        {
            CheckIsDisposed();
            return provider.DatabaseExists();
        }

        public void DeleteDatabase()
        {
            CheckIsDisposed();
            provider.DeleteDatabase();
        }

        public void Dispose()
        {
            this.disposed = true;
            this.Dispose(true);
        }

        public string GetQueryText(IQueryable query)
        {
            CheckIsDisposed();
            if (query == null)
                throw new ArgumentNullException("query");

            return provider.GetQueryText(query.Expression);
        }

        public IEnumerable<T> Translate<T>(IDataReader reader)
        {
            CheckIsDisposed();
            if (reader == null)
                throw new ArgumentNullException("reader");

            IEnumerator en = provider.Translate(typeof(T), reader);
            while (en.MoveNext())
                yield return (T)en.Current;
        }
        #endregion
    }
}
